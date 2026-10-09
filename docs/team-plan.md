# Keyboard Warrior Team Plan

## 1. Current Status and Project Scope

- This repository currently provides a collaboration scaffold. This document describes the work still to be carried out.
- No Unity project has been created in this repository. No editor installation or Unity execution has been performed as part of this collaboration setup; there is no playable or buildable game yet.
- Passing documentation or Git checks does not demonstrate that game functionality works.
- Target: a four-person team delivers a single-player PC game with keyboard controls and a 2D side-scrolling presentation.
- Demo scope: one level leading to the first boss.
- Core loop: basic attacks, precise parries, Smite energy, number-key Smite selection, and a sentence-typing QTE.
- Complete a playable loop with placeholder assets first, then add art, music, feedback, and polish.
- Mobile support, multiplayer, multiple levels, complex equipment systems, and a general-purpose framework are outside the required demo scope.
- No deadline has been confirmed. Milestones below describe acceptance order, not committed calendar dates.

## 2. Roles, Ownership, and Deliverables

Every task has one owner and one reviewer. A primary discipline does not prevent anyone from contributing code elsewhere.

| Proposed role | Member | Main responsibilities | Programming deliverables |
| --- | --- | --- | --- |
| Lead programmer and technical integrator | Andrew | Unity bootstrap, player and combat foundations, shared interfaces, integration | Movement, attacks, parries, damage, Smite energy and activation interfaces |
| Art and animation | Michelle | Characters, environments, interface assets, animation, visual consistency | Animator integration, hit and parry effects, animation events |
| Music and audio | Ace | Music, sound effects, mixing, asset organization | AudioManager, game-event audio integration, volume controls |
| Production and interface flow | Vincent | Task priorities, scope, acceptance sessions, demo flow | Smite selection UI, typing QTE, victory, defeat, and retry screens |

- Confirm these proposed assignments together at kickoff; do not treat this document as evidence that a member has accepted a task.
- Andrew resolves technical boundaries and difficult integration issues; routine PRs need not all wait for Andrew.
- Art and audio owners deliver correct import settings and working integration, not only source files.
- Vincent maintains milestone status, blockers, and acceptance records. Implementers and reviewers remain responsible for correctness.
- Before handing over unfinished work, commit recoverable progress and describe the next step and any known problems.

## 3. Minimum Decisions to Fix During Bootstrap

- Recommended editor series: Unity 6.6 stable. The exact `6000.6.xf1` patch is not fixed yet and must be selected in Unity Hub during bootstrap.
- The bootstrap owner records the full version; all four members install that same patch and the required build modules.
- The Unity project root is `Game/`. Once it exists, `Game/ProjectSettings/ProjectVersion.txt` is the source of truth for the editor version.
- Commit `Game/Packages/manifest.json` and `Game/Packages/packages-lock.json`; add or upgrade packages through a PR.
- Install only packages needed for actual work. Handle editor and package upgrades as a separate, coordinated task.
- Freeze upgrades during demo acceptance; discuss exceptions only when needed to resolve a blocker.
- Start with the Universal 2D template unless bootstrap reveals a concrete reason to choose otherwise; record the template and render pipeline.
- Confirm the demo computer and build target during bootstrap. Use Windows x64 desktop as the initial proposed target.
- Check `Visible Meta Files` and `Force Text`. Add, remove, or rename assets together with their matching `.meta` files.
- Commit `Game/Assets/`, `Game/Packages/`, and `Game/ProjectSettings/`; ignore caches, logs, personal settings, and build output.
- Everyone installs Git LFS. Use shared rules for binary art source files, audio, and agreed large textures.
- Keep `.cs`, `.meta`, text scenes, text Prefabs, and JSON in ordinary Git for readable diffs and merging.
- Put game content under `Game/Assets/_Game/`, with `Scenes`, `Scripts`, `Prefabs`, `Data`, `Art`, `Audio`, and `UI` subfolders.
- Keep third-party packages in their original structure; do not reorganize plugin internals for appearance.
- Before collaborative gameplay work starts, the README must explain the version, setup, entry scene, controls, and build target.

## 4. Daily Git and PR Workflow

1. Choose a task from the current milestone and state its observable result, owner, acceptance steps, and shared files.
2. Start a short branch from current `main`, for example `feat/parry-window`, `fix/qte-input`, or `art/player-idle`.
3. Aim for a task that can be implemented and reviewed in about one working day; split larger tasks into playable increments.
4. Sync with `main` before starting and commit coherent steps with messages describing the behavior changed.
5. Open a draft PR early and record the current state, missing pieces, and reproduction steps.
6. After self-testing, request review and provide operating steps. Include a short video or screenshots for audiovisual changes when useful.
7. At least one teammate reads the change and verifies it. Merge after approval, then delete the completed remote branch.
8. Update the task. Changes to controls, startup, or building must update the relevant instructions in the same PR.

- After Unity initialization, `main` should remain openable and playable. Resolve known blocking errors before merging.
- Do not keep one permanent development branch per person; branch by task.
- A separate `develop` branch is unnecessary for this project. Branches should not become long-term storage for unintegrated work.
- Keep each PR focused; avoid mixing mass asset renames, package upgrades, and combat changes.
- Do not overwrite another member's commits or force-push shared `main`. Resolve conflicts with the relevant authors.
- GitHub branch protection is a configuration item to verify separately; a written agreement does not prove protection is enabled.

## 5. Scene, Prefab, and Shared Script Ownership

- Only one person edits the demo scene at a time. Andrew is the proposed initial integrator; explicitly hand this role over when needed.
- Record the file path, current owner, task, and expected handover point on the task board or agreed team channel.
- Claim a shared scene before editing. Release ownership after merging and notifying the next person.
- Other members validate independent Prefabs in their own test scenes; the current scene owner integrates them into the demo scene.
- Apply the same temporary ownership rule to shared Prefabs, Animator Controllers, audio mixers, and large binary source assets.
- Prefer independent Prefabs and test scenes before introducing a complex additive scene-loading system.
- Move or rename assets inside Unity where possible, and check that related `.meta` changes are included.
- After any scene or Prefab merge, open it in Unity and verify references, hierarchy, animation, and runtime behavior.
- `UnityYAMLMerge` may help later, but a successful automatic merge does not prove that the scene behaves correctly.
- Announce changes to shared player code, input configuration, and project settings before making them.
- Give parallel tasks separate components instead of having four people edit one oversized player script.

## 6. Small Shared Interfaces and Gameplay Decisions

- The lead programmer defines a few usable interfaces or events so art, audio, and UI can integrate independently.
- Initial boundaries include damage, basic attacks, successful parries, energy changes, and Smite start, success, and failure.
- Use one clear entry point for damage and death; avoid separate, contradictory health state in players, bosses, and UI.
- Before QTE implementation, decide whether the world pauses, whether movement or attacks remain available, and what failure or cancellation does.
- Separate number-key selection from sentence entry. Typing a sentence must not also trigger ordinary combat controls.
- Implement only the text and feedback required for the first QTE; do not build a general dialogue or text-editor framework.
- Expose parry windows, damage, energy costs, and QTE duration as tunable values, with rules that can be tested.
- Label undecided values as provisional in the task and explain how they will be evaluated.

## 7. Review and Communication Rhythm

- At the start of a work session, report the intended result, shared files being edited, and current blockers.
- At the end of each working day, update the task and link the branch or PR with reproducible progress notes.
- Raise blockers immediately. Include expected behavior, actual behavior, reproduction steps, and the help needed.
- Hold one short integration demonstration each week using the current build to check the loop and next priorities.
- At every milestone, Vincent organizes acceptance with at least one person who did not implement the feature operating the game.
- Review observable behavior, asset references, input state, and failure handling before discussing minor style preferences.
- Preserve decisions and review outcomes in the task or PR; copy important chat decisions into the relevant record.
- When scope changes, update tasks and acceptance criteria. A new idea is not automatically a commitment for the current milestone.

## 8. AI Assistance and Accountability

- AI may help explain code, propose implementations, suggest tests, review diffs, or produce candidate assets.
- Every AI coding task has a responsible member, a feature branch, and an explicit file scope.
- Parallel AI sessions use separate working directories or worktrees; they must not share the same writable files.
- Only one Unity editor instance writes to a given project directory. Avoid simultaneous scene or package edits from multiple sessions.
- AI work stays on its task branch until the responsible member reviews the diff, tests it, and submits the PR.
- Do not allow AI to merge automatically into `main`, upgrade dependencies without a task, or refactor unrelated modules.
- In the PR, summarize what AI helped with, what a person inspected, and what was actually run.
- The owner must be able to explain the core logic and remains responsible for generated code and assets.
- Confirm that generated assets may be used in the public course repository and preserve required attribution or usage notes.
- Keep the public repository focused on the project; do not commit credentials, student IDs, private messages, or unauthorized assets.

## 9. Definition of Done

A gameplay task moves to Done only when all applicable items are satisfied:

- [ ] The observable result in the task is achieved; additional work is tracked separately.
- [ ] The result has been run in the locked Unity version, with no new blocking Console errors.
- [ ] Assets, scripts, Prefab references, and `.meta` files are complete, with no dependency on local absolute paths.
- [ ] Relevant success and failure paths were checked, such as failed parries, QTE timeout, death, or retry.
- [ ] Input changes were checked for accidental actions or stuck states across combat, pause, and QTE transitions.
- [ ] The PR describes verification steps, results, and known limitations, with audiovisual evidence where useful.
- [ ] A teammate reviewed and verified the work; it is merged into `main`, and task status and instructions are updated.

Documentation and collaboration setup tasks use their own acceptance criteria; do not invent Unity test results before a project exists.
Do not require an automated test for every component. Add useful tests for recurring defects or isolated calculation rules when justified.

## 10. Testing, Builds, and Releases

1. For each PR, the implementer checks the change and the gameplay paths it affects.
2. After integration, run a short smoke test: enter the level, move, attack, parry, use Smite, die, and retry.
3. At each milestone, build `main` for the agreed PC platform; editor Play Mode alone is insufficient.
4. Give the build to another member to launch and operate without the Unity editor running.
5. After bootstrap, a teammate clones into a new directory and checks LFS downloads, import, and the entry scene.
6. Before final demonstration, clone again into a new directory, fetch LFS content, open with the fixed editor, and build without reusing an old `Library`.
7. Complete the full path: level start → basic attack → precise parry and energy → number-key Smite selection → typing QTE → first boss.
8. Also check wrong input or timeout, insufficient energy, player death, retry, and the interface state after the boss fight.
9. Once accepted, tag the exact commit and attach the build ZIP, controls, and known issues to a GitHub Release.
10. Keep build output out of source history. Further code changes require a new tag and identifiable build.

Early acceptance prioritizes behavior and complete references. Judge performance on the actual demo computer.
Record the tested commit, editor version, machine, and result; do not write only "tested the latest version."

## 11. Startup Tasks, Dependencies, and Milestones

The following are planned tasks. Completion requires the corresponding task, commit, and acceptance evidence.

| Task | Proposed owner | Dependencies | Acceptance result |
| --- | --- | --- | --- |
| S0: Confirm roles, access, and workflow | Production, with everyone | None | All four members can access the repository and agree on roles, PRs, and scene ownership |
| S1: Bootstrap Unity | Lead programmer | Initial repository; can run alongside S0 | Exact editor version, template, and build target fixed; minimal project and startup instructions committed |
| S2: Reproduce on another machine | Someone other than the bootstrap owner | S1 | Fresh clone, LFS, import, entry scene, and empty PC build succeed |
| S3: Graybox movement and basic attacks | Lead programmer | S1; S2 before integration acceptance | Player, ground, damageable target, and simple retry work |
| S4: Precise parry and energy | Lead programmer | S3 | Success and failure windows, damage, and energy changes are repeatably verifiable |
| S5: Smite selection and typing QTE | Interface flow owner | S1 and agreed input/Smite interfaces | Selection, typing, success, failure, and exit states work in a test scene |
| S6: Art, animation, and audio integration | Art and audio owners | S1; feedback integration requires S3/S4 events | Small batches of assets import correctly; animation and sound work in context |
| S7: Level and first boss | Assigned level/combat owner | S3, S4; Smite integration requires S5 | Play from level start to the boss, then finish or retry |
| S8: Full integration and public demo package | Lead programmer and production, with team acceptance | S2, S5, S6, S7 | Fresh-clone acceptance, PC build, tag, instructions, and known issues complete |

- **M0: Collaboration can start.** S0–S2 pass and another machine can rebuild the project. The current scaffold has not reached this milestone.
- **M1: Combat loop works.** S3–S5 are integrated; placeholder assets are sufficient to attack, parry, gain energy, and use Smite.
- **M2: Demo content is complete.** S6–S7 are integrated; the level and first boss are playable with the main animation, audio, and UI.
- **M3: Delivery is reproducible.** S8 passes; the demo comes from an exact commit and a non-implementer can run it using the instructions.
- Art and music sketches can begin before S1. Fix import specifications and event integration after bootstrap and interface agreement.
- When a milestone fails, repair the loop before adding content. Build a calendar schedule only after a deadline is confirmed.
