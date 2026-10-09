# Demo Acceptance Checklist

**Status: Unity bootstrap is incomplete. Every Unity and gameplay check below is unexecuted.** Repository checks passing does not mean that Unity compiles, the game is playable, or a release is ready.

For each run, record the commit SHA, full editor version, tester's first name, date, build configuration, results, and evidence. Use Pass / Fail / Not run / Not applicable, with a reason for Not applicable. Student IDs and other personal details are unnecessary.

## 1. Complete single-player PC flow

- [ ] The Windows standalone build starts, explains its controls, and enters the one-level demo.
- [ ] Movement, attacks, damage, death, pause, resume, and exit work.
- [ ] Successful parries grant energy according to the rules; sufficient energy allows Smite selection and sentence QTE entry.
- [ ] QTE success produces the selected effect. Failure, timeout, and cancellation settle energy according to defined rules, exactly once.
- [ ] Enemy and first-boss telegraphs, attacks, damage, and death are readable; the boss can be defeated normally.
- [ ] A complete path exists from starting the game to defeating the first boss, with an explicit victory outcome.
- [ ] Retry resets health, energy, enemies, QTE, input, camera, and time state correctly.
- [ ] The real/false-world story has the presentation agreed for this demo; a complete dual-world system is not an implicit requirement.

## 2. Parry and damage fairness

- [ ] A successful parry rewards one valid attack event; multiple colliders or contact frames from the same attack cannot grant repeated energy.
- [ ] Failed parries, holding parry, empty swings, and invalid targets cannot accidentally grant success rewards.
- [ ] Player and enemy damage follow their hit rules rather than damaging every frame while colliders overlap.
- [ ] Parry windows, non-parryable moves, and success/failure feedback agree with the actual judgment.
- [ ] At least one member who did not implement the system playtests basic boss and QTE difficulty.

## 3. QTE input and transitions

- [ ] While typing, letters, spaces, and digits do not trigger movement, jumping, attacks, parries, or skill reselection.
- [ ] Entering QTE while holding movement leaves no stale movement input; the selection digit does not become the sentence's first character.
- [ ] The last character or submit key does not also trigger an action after QTE ends.
- [ ] Case, punctuation, spaces, backspace, repeated presses, and held keys behave according to the prompt rules.
- [ ] Prompt, progress, mistakes, and time remaining are readable; each attempt settles only once.
- [ ] Supported IMEs distinguish composition from submitted text. English-only builds clearly state their input requirement.
- [ ] Pausing during QTE stops its countdown; resuming preserves remaining time without adding or subtracting time unexpectedly.
- [ ] Alt+Tab, focus loss, and focus recovery do not continue the countdown, accept background gameplay input, or cause an accidental action on return.
- [ ] Death, scene reload, retry, and repeated casting leave no duplicate text subscriptions, stuck input mode, or incorrect time scale.

## 4. Frame rate and standalone build

- [ ] Check movement, parry windows, hit counting, and QTE duration at 30 FPS, 60 FPS, and one explicitly recorded high frame rate.
- [ ] QTE duration agrees with real time regardless of render frame rate or slow motion; ordinary pause stops the timer as specified.
- [ ] Short stalls do not cause obvious duplicate settlement; differences have reproducible steps and tracked issues.
- [ ] HUD, long sentences, pause menus, and prompts remain readable at 1280 x 720 and 1920 x 1080.
- [ ] Complete start -> combat -> QTE -> death/retry -> boss -> victory in a standalone build, not only in the Editor.

## 5. Fresh clone and team reproducibility

- [ ] Clone into a new directory, install and initialize Git LFS, and run `git lfs pull`; assets are actual content rather than unresolved pointer files.
- [ ] Open `Game/` with the full Unity version recorded in the README; imports complete without compilation errors blocking play.
- [ ] Scene, prefab, script, and material references are intact, with no dependency on a member's absolute paths or uncommitted resources.
- [ ] Dependencies restore from the committed manifest and lock file without unrecorded local plugins.
- [ ] Another member can locate the startup scene, play the demo, and produce a Windows standalone build using the README.
- [ ] The clone check does not reuse the original Library cache or copy missing resources manually to hide an incomplete commit.

## 6. Public repository and release package

- [ ] Code, images, fonts, music, sound effects, and third-party packages have documented sources and rights for public use; required attribution is included.
- [ ] Repository content, history being published, and release files contain no student IDs, personal emails, private document links, secrets, or non-public material.
- [ ] Paid or restricted assets are not published simply because the project needs them; non-redistributable dependencies have clear replacement or installation instructions.
- [ ] The release uses a clean build. Its ZIP contains the executable, data folder, and required runtime files, rather than only the `.exe`.
- [ ] Local builds use ignored `Builds/Windows/<version>/`; release ZIPs use ignored `Builds/Releases/KeyboardWarrior-<version>-Windows-x64.zip`.
- [ ] The README records the actual version, download location, launch steps, controls, system requirements, known issues, and Git tag; until a package exists, it states that no build has been released.
- [ ] The release tag points to the tested commit, and its version agrees with the ZIP filename. Attach ZIPs to a Release rather than committing builds as source files.
- [ ] Extract the ZIP into a new directory, launch it, and complete a basic smoke test to verify the actual distributed build.

## Acceptance record template

| Field | Record |
| --- | --- |
| Commit SHA / tag | Pending |
| Full Unity version / package lock status | Pending |
| Windows version / build configuration / frame rates | Pending |
| Tester's first name / date | Pending |
| Passed / failed / unexecuted checks | Pending |
| Reproduction steps / screenshot or log location | Pending |
| Release ZIP path and version | Not released |

Check an item only after execution and recorded evidence. Record directory, Git, Markdown, and LFS configuration checks separately as repository checks; they cannot satisfy Unity or gameplay checks above.
