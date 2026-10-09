# Unity Setup and Development Conventions

**Status: planning only. Unity bootstrap is not complete.** `Game/` has not been initialized as a Unity project. No Unity compilation, gameplay test, or build has been performed.

## 1. Target environment and version lock

- Target: a single-player Windows PC game, keyboard controls, 2D side-scrolling, one level and the first boss.
- Editor target: a formal Unity 6.6 release. The bootstrap owner must verify availability in Unity Hub, course-machine support, and dependency compatibility before locking the full version.
- Full editor version: **pending bootstrap**. Everyone must use the same complete version, including the patch number.
- Template: Universal 2D with the URP 2D Renderer. Start with sprites, 2D physics, and an orthographic camera.
- First build target: Windows x64. Record the scripting backend and required build modules after the first successful build.
- The demo does not require 3D environments, networking, mobile support, or simultaneous releases on multiple platforms.
- Propose editor or package upgrades in a separate PR. Merge only after another member can open the project and core gameplay and standalone builds pass.

## 2. Bootstrap checklist (not yet executed)

1. Assign one bootstrap owner and verify the formal editor release and course-machine compatibility.
2. Create a Universal 2D project. Its final project root must be `Game/` inside this repository.
3. If created elsewhere, close Unity and move `Assets/`, `Packages/`, and `ProjectSettings/` into `Game/`, retaining all associated `.meta` files. Do not nest another project folder inside `Game/`.
4. Do not copy generated `Library/`, `Temp/`, `Obj/`, or `Logs/` content. Do not fabricate `ProjectVersion.txt`, package manifests, or empty scenes to simulate a completed bootstrap.
5. Add and open `Game/` through Unity Hub. Allow normal cache generation and resolve compilation errors.
6. Confirm Visible Meta Files for version control and Force Text for asset serialization; save the project settings.
7. Commit the actual `Game/ProjectSettings/ProjectVersion.txt`, `Game/Packages/manifest.json`, `Game/Packages/packages-lock.json`, and required project files.
8. Create a minimal greybox test scene and produce a real Windows standalone build.
9. Have another member perform a fresh clone, download LFS content, open the exact editor version, play, and build.
10. Update the README with the full editor version, package versions, startup scene, and build steps. Mark bootstrap complete only with recorded verification.

## 3. Dependency choices

| Dependency | Purpose and policy |
| --- | --- |
| Universal RP / 2D Renderer | Keep the template-compatible version; do not independently chase newer versions |
| Input System | Use one input approach for gameplay, menus, and typing |
| Cinemachine | Camera following, boundaries, and essential camera feedback |
| uGUI and TextMesh Pro | Health, energy, QTE, menus, and text; follow the selected editor's actual package structure |
| 2D Sprite / Tilemap | Enable only what the art and level require |
| Unity Test Framework | Use for meaningful logic checks such as QTE evaluation or energy settlement when such tests are implemented |

These are planned choices, not installed dependencies. Verify versions through the selected editor's Package Manager and commit the real manifest and lock file. Members must not independently upgrade packages. Avoid unused frameworks, networking SDKs, and paid plugins.

## 4. Project and asset organization

Place game content under `Game/Assets/_Game/`, grouped into `Scenes`, `Scripts`, `Prefabs`, `Art`, `Audio`, `UI`, and `Data`. Keep third-party content separate. Create these folders in Unity; move and rename assets through the editor whenever possible.

- Keep player, enemy, boss, interactive-object, and UI prefabs independent; compose them in the level.
- Assign a current editing owner for each shared scene. Develop systems in separate test scenes and prefabs first.
- Commit, move, and delete each asset together with its `.meta`. Do not delete metadata in bulk to repair references.
- Track designated binary art and audio through repository `.gitattributes` and LFS. Keep scenes, prefabs, scripts, and text-serialized settings diffable.
- Check source and redistribution rights before importing third-party content. A purchase does not automatically permit publishing the original asset pack.

## 5. Art, audio, and data conventions

Agree on one art specification before completing the first greybox. These values are **not yet locked**.

- Record pixel or non-pixel style, Pixels Per Unit, baseline character height, tile dimensions, pivots, background layers, and sorting layers.
- Use consistent filtering and compression for pixel assets; do not apply pixel settings automatically to non-pixel art. Use scalable UI layouts and check 1280 x 720 and 1920 x 1080.
- Prioritize idle, movement, attack, parry, hurt, death, and required boss telegraphs; feedback must make the rules readable.
- Group audio into Music, SFX, and UI, agree on loudness, define music loop points, and distinguish attack, parry, and QTE outcomes.
- Keep traceable source files, exports, and license notes; omit disposable production caches.
- Use ScriptableObjects for skill definitions, sentence prompts, and enemy base values where useful. Store live health, typed text, and timers in runtime instances rather than changing shared configuration assets.
- Implement the data needed for the demo before building generic skill editors or complex event frameworks.

## 6. QTE, pause, and input

Use one state owner for `Combat -> SmiteSelect -> Typing -> Resolve -> Combat`, with explicit pause and death handling. Individual systems must not independently change global time or switch input modes.

- Separate Combat, Typing, and UI inputs. Use Dynamic Update for input processing; consume cached movement input in the physics update.
- On entering Typing, disable combat actions and clear cached actions. Use a text input control or `Keyboard.onTextInput`; do not construct text from physical key names.
- Gate text callbacks by state as well: disabling a combat action map does not disable directly subscribed text events.
- Prevent the selection digit from entering the sentence and the final typed character from triggering a resumed combat action. Handle unsubscription, death, and retry.
- Initial design: freeze combat while the QTE countdown continues. A pause menu or lost window focus pauses both combat and the QTE timer.
- Use time independent of slow motion for the QTE timer, with an explicit pause condition. Do not depend on FixedUpdate or WaitForSeconds to progress it while gameplay is frozen.
- Centralize normal speed, QTE freeze, hit stop, pause, and retry so ending one effect cannot incorrectly release another pause.
- Lock the prompt language and case/punctuation rules first. If supporting Chinese, explicitly test IME composition and candidate submission.
- Complete short prompts, correction, timeout, energy settlement, and feedback before increasing sentence length, skill count, or slow-motion complexity.

## 7. AI-assisted experiments

After a minimal project opens and builds, try the in-editor Assistant on a setup/experiment branch. Link the project to the team's Unity Cloud project, install through the Editor's AI menu or Package Manager (`com.unity.ai.assistant`), and record the installed package version. Each member uses their own account. First use Ask for explanations, then Plan/Agent for a small, reviewable change. Verify current trial/credit terms before opting into a subscription; installing this repository does not subscribe anyone.

For external agent control, the current official route is Unity CLI with its MCP mode. Add it only when someone needs editor automation; ordinary collaboration does not depend on it. Do not make the playable demo depend on a developer's AI login or cloud subscription. See [Assistant installation](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.20/manual/install/install-chat.html) and [Unity CLI / MCP](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli).

- Work on AI-generated code and design experiments in separate feature branches with small commits. Do not directly replace shared scenes, settings, or the main branch.
- The contributor must understand the code, check references, and run acceptance checks; an AI completion claim is not validation evidence.
- Check experimental assets for source, redistribution rights, and personal information before committing them publicly.
- Avoid simultaneous agents editing the same scene, prefab, or settings file. The module owner integrates the result.
- PRs must describe the problem, resulting behavior, and actual validation. Preserve unexecuted checks instead of assuming they passed.

Official references: [Keyboard and text input](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/Keyboard.html), [Input update settings](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/Settings.html). These explain mechanisms; their documentation version does not prescribe the package version to install.
