# Typing Combat Experiment: Complete Plain-Language Guide

Version **0.1**, October 9, 2026. Status: **a design for future implementation and playtesting; not implemented or player-tested yet.**

This guide and the [implementation specification](typing-combat-spec.md) describe the same design in two ways. This edition explains the rules in everyday language while retaining every important value, exception, acceptance check, and decision criterion. The D01–D18 identifiers match one to one so discussions and revisions can stay aligned. The existing `Validation` scene is a different prototype; its previous test results do not validate this design.

The idea in one sentence: **find an opening between enemy warnings and attacks, type a short spell, and land the attack as soon as the final letter is correct. A typo or a hit removes some progress, but completed words stay saved.**

Reading route: D01–D03 explain the goal and flow; D04–D12 explain which inputs do what; D13–D16 explain how to check functionality and experience; D17–D18 explain the implementation sequence and evidence limits. Implementation and logging details are retained here, so understanding an important rule does not require switching documents.

## D01 — What are we trying to find out?

The central question is: **can players make understandable, satisfying combat choices while reading a short phrase, typing letters, and responding to a visible threat?**

The complete experience should be: notice danger → decide to start or continue typing → enter letters → recover from mistakes or hits → finish the spell and attack immediately → reassess the situation. High typing accuracy alone does not establish that combat is enjoyable. Passing software tests or looking like a reference game does not establish that either.

The future deliverable is a separate Windows keyboard experiment with one enemy, one spell, practice, two comparable time modes, a results screen, local records, and acceptance tests. The player moves left and right along flat ground, retaining the project's side-view format. Jumping and platforming challenges are unnecessary here.

This round excludes parry-earned energy, regular melee, multiple spell selection, inventory, book pickup, puzzles, loot, story progression, production art, generated content, translated spells, controllers, networking, and cloud analytics. That isolates the typing interaction; it does not remove those features from the full game. Character feedback, danger cues, and attack feedback remain required because they directly affect the experience.

This document does not claim evidence obtained by purchasing, installing, or personally playing *The Textorcist*. Research uses public material. Every value below is **a design choice for this Keyboard Warrior experiment**, not a parameter extracted from the reference game or a balance result already established by testing.

## D02 — What does the reference game tell us?

*The Textorcist: The Story of Ray Bibbia* provides design ideas, not permission to assume its undocumented internal rules or reuse its art, text, or other assets. The sources below were consulted on **October 9, 2026**. The project's implications are design inferences, listed separately from directly supported facts.

| Source | What the material supports | Implication for this project (inference) |
| --- | --- | --- |
| [S1: Headup official game page](https://www.headupgames.com/game/the-textorcist-the-story-of-ray-bibbia) | Typing and bullet dodging coexist; keyboard and controller experiences differ. | Test divided attention with a real keyboard and real threats. |
| [S2: Official Steam description](https://store.steampowered.com/app/940680/The_Textorcist_The_Story_of_Ray_Bibbia/) | Combat requires typing exorcisms, with English, Latin, and boss-specific variations. | Start with familiar English without adding language difficulty. Store user reviews are not an official specification. |
| [S3: Developer Diego Sacchetti's Let's Jam!](https://www.codemotion.com/magazine/frontend/gamedev/lets-jam/) | Early players focused on text at the bottom and neglected the character; showing the current word above the character helped. Development progressed through a game-jam prototype, a design document, and a complete boss prototype, incorporating public feedback. | Show both the full phrase and the current word near the character; use a playable experiment to guide further design. This is a development account, not a current input manual. |
| [S4: Direct designer interview, page 3](https://david-bailly.com/portfolio/typing-games-how-and-why/3/) | Meaningful phrases fit the task; danger makes chanting harder; hand switching caused by arrow-key movement was intentional. | Retain phrase meaning and observe whether hand switching is comfortable instead of assuming it is. |
| [S5: Same interview, page 11](https://david-bailly.com/portfolio/typing-games-how-and-why/11/) | Language changes and boss-specific text interference affect difficulty. | Exclude these extra variables from the first comparison. |
| [S6: Same interview, page 16](https://david-bailly.com/portfolio/typing-games-how-and-why/16/) | Translating spells changes challenge and progression. | Keep interface labels and the experiment corpus in English; translated phrases cannot be assumed mechanically equivalent. |
| [S7: Official Steam updates 1–3](https://store.steampowered.com/news/posts/?appids=940680&enddate=1552657509&feed=steam_community_announcements) | Updates added Shift with configurable movement keys, changed ambiguous overhead glyphs and text styling, and fixed some fast-input delays and the Bible leaving the screen. | Explicitly check fast input and glyph readability. Historical fixes do not validate every current configuration. |
| [S8: Control-scheme reply marked as developer](https://steamcommunity.com/app/940680/discussions/0/3203652426708107869/) | Developer Zoltac confirmed stick movement combined with keyboard typing. | Alternative controls belong in a separate experiment; other replies in the thread are player opinions. |

These sources **do not confirm** the exact typo rollback, word checkpoints, space/case/punctuation handling, Enter versus automatic firing, damage per letter/word/phrase, book drop and pickup distances or protection time, hit/input ordering, a universal phrase timer, focus-loss behavior, or input buffering. Reviews may offer research leads but cannot fill these gaps. D04–D14 define this project's own rules and must not be described as reproducing the reference game's internals. Exact reproduction would require direct observation of an identified game version.

Hypotheses still awaiting testing include: one-letter rollback is understandable; three-word phrases have a suitable length; saving completed words reduces interruption frustration; attacking on the last letter feels like combat; and the chosen threat rhythm leaves meaningful typing opportunities.

## D03 — How does a run work, and what are we comparing?

The flow is fixed: **Setup → Practice → Ready screen → 3-second countdown → Encounter → Results**. Retrying from Results returns to Ready with the same experimental condition. Mode and accessibility settings can change only outside an encounter; changes require a new run record.

At the start, the player is at x=0 with 3 HP, the enemy has 100 HP, no spell is active, and held keys from the previous screen do not carry over. The first warning begins after 2 seconds of enemy simulation time.

Press **1** once to start the spell. Once shown, the phrase remains fixed for that attempt. The final correct letter automatically deals **25 damage**, followed by a **0.60-second** cooldown. After cooldown, a fresh press of 1 is required to start again. Four successful casts defeat the enemy; three damaging hits defeat the player. At **120 seconds** of active encounter time, the result is `TimeLimit`: incomplete, not a death.

| Mode | While typing a spell | Purpose |
| --- | --- | --- |
| `RT`, real time, the primary design | The enemy continues acting and the player can move; stopping typing does not stop combat. | Test timing decisions, divided attention, and simultaneous controls. |
| `PAUSED`, combat paused during typing, the comparator | Enemy phase, damage checks, and player position freeze for the entire chanting phase; text input continues. | Observe the difference caused by suspending combat while typing. Frozen movement is part of this mode's rule. |

Both modes use the same phrases, damage, input rules, and enemy parameters. Only combat advancement during chanting differs. Do not substitute slower enemies, easier words, or different typo penalties. Outside chanting, combat advances normally in both modes.

Neither mode has a **per-phrase countdown**. The 120-second encounter limit continues during chanting. Lower enemy exposure in `PAUSED` is expected and must be recorded separately; fewer hits cannot simply be attributed to better controls or performance (D14).

Practice has three explicit steps. Each displays `PRACTICE`, uses the same input/pause/feedback rules, begins with a 3-second countdown, and has **no 120-second limit**. Its elapsed clocks and events are marked practice and excluded from measured encounters. There is no hidden automatic difficulty adjustment in measured encounters.

| Practice step | Active rules | How to continue |
| --- | --- | --- |
| Typing only | Repeat P0 after each successful cast. Player position is fixed and there are no enemy attacks; spell and cooldown work normally. The dummy starts at 100 HP and immediately resets to 100 whenever damage would reduce it to zero, without producing a win. The player stays at 3 HP. | Continue only when the player or facilitator selects Next step. |
| Dodging only | RT movement with the same warning/strike cycle. Spell starts and all letters are disabled; no phrase is active. Hits produce feedback, increment the practice-hit count, and grant normal invulnerability, but do not reduce the displayed training HP. | Continue only when Next step is selected. |
| Combined RT | Repeat P0 with all RT rules, including 3 player HP, hit word resets, and reaction lock. The dummy still resets instead of producing a win. Player death opens practice Results; Enter cleanly restarts this step. | Next step ends practice and opens the measured Ready screen. No practice state carries over. |

Escape pauses each step. The menu offers Resume, Restart step, Next step, Return to setup, and Exit. Up/Down and Enter select actions. Practice Results offer the same actions except Resume, with Restart selected by default. Restart stays in the same step; Next step remains available before the suggested practice duration. D15's teaching durations are managed by the facilitator, not automatic timers. Practice records include `practiceStage`; resets create a new run ID without counting as a measured failure penalty.

## D04 — Which keys work, and which inputs count?

| Key or input | Exact encounter behavior |
| --- | --- |
| Hold Left/Right arrows | Move horizontally, including during RT chanting and hit reaction. Holding both directions gives zero speed. |
| Fresh press of top-row 1 or Numpad 1 | Start a spell only when ready. Ignore during chanting, reaction lock, cooldown, or a terminal state. |
| A–Z/a–z produced by a physical key press | Match letters without case sensitivity during chanting when not reaction-locked. A/D/J/K/E/R and other letters only enter text; they do not trigger legacy combat actions. |
| Space, punctuation, and digits not used as the start command | No progress gain/loss or typo penalty; record an ignored-input category. Do not silently insert them into the phrase. |
| Backspace/Delete | No editing or voluntary rollback. This is game progress, not a text field. |
| Enter | No submission during combat. Confirm menu items; on Results, release previous keys before a fresh Enter press can retry. |
| Escape | Pause, or request resume when already manually paused. Never cancel the current phrase. |
| Up/Down arrows | Select menu options outside combat; no encounter action. |
| Shift/Caps Lock | Do not change letter identity. Shift is not a movement modifier in v0.1. |
| Ctrl/Alt/Windows-key shortcuts, Tab, function keys | Do not enter letters or trigger spells. System shortcuts remain system behavior; switching away pauses as specified in D06. |

The baseline is **Windows with an English US keyboard/input layout**. Match the Latin character actually produced rather than assuming physical key names work identically across layouts. Other layouts, IME composition, dead-key combinations, and non-Latin text are unvalidated in v0.1. Unsupported characters neither advance progress nor count as typos. Show a rate-limited instruction to switch to English input; do not approximate unknown Unicode characters as English letters. Interface labels must be in English; the experimental phrases remain unchanged.

Do not read the clipboard or accept paste, autocomplete, speech input, or OS key-repeat advancement. **Holding a letter counts once; entering the same letter twice requires release and another press.** Holding 1 must not start successive spells.

Input handling must distinguish text produced by a key from a distinct physical press, correlate the two, and preserve rapid adjacent presses. Connecting Unity's `onTextInput` alone does not prove that repeat, paste, and input-origin filtering work; the first implementation step must verify this explicitly (D17). Reject unsupported composed or multi-character commits. Multiple ordinary key presses arriving in the same rendered frame must still be processed individually in order.

All gameplay input enters one ordered queue; a UI text field must not change game state independently. Movement and typing may happen together. Consume menu confirmation events, clear pending gameplay input, and require release before that control works again. Once combat is active, a press of 1 followed by a genuinely later first-letter press in the same input update is valid; sharing a render frame must not cause that letter to be dropped.

## D05 — Phrases, saved progress, and mistakes

Each phrase is fixed data with an ID, version, display text, and word list: `{id, version, displayText, words[]}`. The initial corpus version is `prompts-v0.1`; every phrase has integer `version: 1`. Use only lowercase ASCII English letters, exactly one space between words, no leading/trailing spaces, and no punctuation. Empty words, nonletters, duplicate IDs, or disagreement between the joined words and display text are invalid. Do not generate phrases at runtime or with AI during measured encounters.

| Bank | ID | Displayed phrase | Letters actually required |
| --- | --- | --- | --- |
| Practice | P0 | `burn the seal` | 11 |
| A | A1 | `break the seal` | 12 |
| A | A2 | `light the path` | 12 |
| A | A3 | `guard the gate` | 12 |
| B | B1 | `break the lock` | 12 |
| B | B2 | `light the hall` | 12 |
| B | B3 | `guard the wall` | 12 |

These original experimental phrases are placeholders for later narrative writing. A/B phrases all have word lengths 5/3/4. This establishes matching length, not equal vocabulary familiarity or finger-movement difficulty.

A run uses its bank in order 1, 2, 3, 1 for four successful spells. Starting an attempt selects the current entry; only a successful cast advances the bank position. Hits and typos do not change the phrase. Retry returns to the bank's first entry.

The game tracks two positions: which word is current and how many letters of it are correct. In code these are zero-based `wordIndex` and `letterIndex`.

- A correct letter advances the current word by one letter.
- Completing a nonfinal word immediately saves it, moves to the next word, and resets letter progress to zero. **No Space or confirmation is required.**
- Completing the final word emits one completion event and attacks immediately (D09).
- A wrong supported English letter removes only the last correct letter of the current word, stopping at zero. It never undoes completed words. Consume that wrong letter once; do not re-match it after rolling back.
- A typo does not remove HP, lock input, cost extra resources, or change the phrase. Show the newly expected letter. Each subsequent physical press is checked against the new position, even if the player intended to continue the old suffix.

Example with the practice phrase `burn the seal`:

| Action | Result |
| --- | --- |
| Start, then type B, U | Current word `burn`; `bu` is complete; `r` is next. |
| Type X next | Progress becomes `b`; `u` is next. X is not processed again. |
| Type U, R, N | `burn` is saved. Move to `the` with no letters entered yet. |
| Type X at the start of `the` | Remain at the start of `the`; completed `burn` stays saved. |
| Press Space or Backspace | No change. |
| Type `the`, then `seal` | The final L immediately triggers the spell; Enter is unnecessary. |
| Finish `burn`, type the t in `the`, then get hit | `burn` remains saved; `the` returns to its start, and reaction lock begins. |

Stopping, moving, watching danger, or pausing retains progress; there is no automatic decay. A damaging hit clears the current word's partial progress (D08) but retains completed words. This experiment has no book drop or pickup.

## D06 — State, time, and resuming after a pause

Keep the encounter's phase separate from the spell's phase. The encounter can be counting down, active, paused, or showing results. The spell can be ready, chanting, or cooling down, with separate reaction-lock and invulnerability timers. Chanting cannot always mean combat is inactive, because RT must keep combat running.

| Event | Text progress / spell phase | HP, enemy, and time |
| --- | --- | --- |
| Move or stop typing | Retained | RT combat continues; PAUSED freezes combat only during chanting. |
| Wrong letter | Current word loses one letter of progress, minimum zero | Other time continues normally. |
| Damaging hit | Clear current word's partial progress; spell phase stays unchanged | Lose 1 HP; lock typing/start for 0.35 s; gain 1 s invulnerability. |
| Manual pause, focus loss, or keyboard disconnect | Retain progress; discard queued gameplay input | Stop all gameplay clocks and movement. |
| Resume | Retain progress; do not replay old input | Meet release/confirmation requirements, count down 1 s, then continue the exact saved enemy phase. |
| Successful spell | Close this attempt and begin cooldown | Enemy loses 25 HP exactly once. |
| Win, death, time limit, or leaving the encounter | Stop accepting input and attacks | Fix the result and clear hazards and pending events. |
| Retry | Clear progress; return spell to ready | Reset both HP values, position, bank position, enemy phase, timers, input queue, and run statistics; create a new run ID. |

Several different kinds of time are needed:

| Clock | How it advances | What uses it |
| --- | --- | --- |
| Real elapsed time, `wallSeconds` | Monotonically, including pauses; system clock adjustments cannot move it backward. | Diagnostics, never damage. |
| Active encounter time, `encounterSeconds` | Includes chanting, cooldown, and hit reaction; excludes initial/resume countdowns and explicit pauses. | End the run at 120 s; reaction lock and cooldown. |
| Enemy simulation time, `enemySeconds` | Advances only when combat advances. Equals encounter time in RT; excludes chanting in PAUSED. | Enemy phases, invulnerability, player movement. |
| Reaction/cooldown timers | Advance with encounter time; stop during explicit pause. | Can expire even when overlapping PAUSED chanting, preventing permanent locks. |
| UI cue timers | Preserve remaining duration during explicit pause; advance during PAUSED chanting. | Cue durations. Countdown uses separate time while the window is focused and the keyboard connected. |

**Returning to the window does not automatically resume play.** Focus loss, keyboard removal, or a simulation gap longer than 250 ms must pause with a visible reason. Resume requires focus, a connected keyboard, release of previously held gameplay controls, and a fresh Enter/Escape confirmation, followed by a 1-second countdown. The confirmation key cannot enter the phrase.

Focus recovery cannot cancel an existing manual pause. No countdown accepts letters, movement, damage, or spell starts. Remember the countdown's kind if interrupted: after renewed confirmation, restart the full **3 seconds** for an initial countdown or the full **1 second** for a resume countdown. Do not shorten preparation time. Gameplay keys still held when combat starts, including keys newly pressed during countdown, stay inactive until released and pressed again. They cannot automatically move, type, or cast. Menus, pause, and confirmation also require fresh presses, not held-key repeats.

The pause menu offers Resume, Restart encounter, Return to setup, and Exit application. Up/Down select; Enter confirms. Escape requests Resume only when prerequisites are met. Restart records the old run as `Aborted`, returns through Ready, and creates a new run. Returning to setup also records `Aborted`. There is no letter-key restart during combat. Closing the window through the OS must safely end the process. If a saved start has no matching end, later analysis classifies it as `Interrupted`, not a win or loss.

## D07 — What happens first when events coincide?

Game logic runs **60 times per second**; each step is a tick. Every input has a monotonic timestamp and sequence number. Remove countdown/explicit-pause intervals to obtain active encounter time `t`; discard gameplay events occurring inside those intervals. Assign each event to `max(1, ceil(t × 60))`: tick n processes events in `((n-1)/60, n/60]`, and a start-time event at zero belongs to tick 1. Within each category, order by timestamp, then sequence number. Use integer ticks for boundaries rather than unreliable floating-point equality comparisons.

Movement also uses queued press/release events, not the latest rendered frame's keyboard state. Apply movement events through the current tick in order, keep the final held state, then move once by `direction × 5/60` units. A press and release in the same tick leave the key released for that movement step. This is the defined 60 Hz movement precision. Rendering at 30/60/120 FPS must not change movement speed, phrase length, or input correctness. Catch up from normal delays using fixed ticks. A gap over 250 ms pauses and flags a performance issue instead of fast-forwarding attacks the player never saw.

Each tick follows this exact order:

1. **Handle pause first.** Focus loss, device/performance problems, and explicit pause commands take priority. Freeze this tick, discard its gameplay input, and retain existing state.
2. **Check the time this tick would reach before changing gameplay state.** At or beyond 120 seconds (tick 7200), set time to 120 and record `TimeLimit`; process none of this tick's movement, damage, or text. A tick advancing from 119 + 59/60 seconds to 120 cannot accept a final rescue letter. Terminal states reject all gameplay events.
3. Advance eligible timers, expire locks/cooldowns, and, when combat advances, process movement within the arena bounds.
4. Advance the enemy phase, check hits, and resolve damage/death first. **A hit takes priority over a final letter in the same tick.** A hit rejects that tick's letters; timer boundaries cannot bypass this.
5. Process spell starts and letters by timestamp and sequence. A valid start can precede a letter in the same tick. Ignore letters during reaction lock, outside chanting, or after the run ends. Phrase completion and enemy damage happen together, never partly. Enemy HP reaching zero immediately wins.
6. Publish one read-only presentation snapshot and measurement events. Rendering itself cannot advance progress or cause damage.

Threat phase intervals include their start but exclude their end: warning `[0, 0.90)` seconds, strike `[0.90, 1.05)`, recovery `[1.05, 3.50)`. Every strike tick checks the player's **post-movement** hurtbox until that attack has damaged the player once or ends. Each attack ID can deal damage at most once. Equal-timestamp inputs use recorded sequence order; pause retains its higher priority.

Each completion event includes `{runId, attemptId, castId, tick}`, identifying the run, attempt, cast, and tick. Repeated callbacks cannot apply damage again for the same cast ID. Enter cooldown immediately after completion and discard the closed attempt's remaining text events; they cannot feed the next phrase. Do not buffer starts during cooldown/reaction lock. Holding 1 cannot automatically start when a lock expires. Menus, scene reloads, retry, and focus changes invalidate stale queued events from old runs or attempts.

## D08 — The smallest arena and enemy attack

Use a fixed side-view orthographic camera and a flat lane. There is no scrolling, jumping, gravity challenge, cover, or damage from touching the enemy's body. The logical lane is x ∈ [-6, 6]. The player hurtbox has half-width 0.25, so its center stays in [-5.75, 5.75]. Movement speed is **5 units/second**, with immediate direction changes and no acceleration. Facing is cosmetic; fixed height, camera framing, and decorative depth must not change collision.

One stationary, visible enemy sits above and to the right of the lane; suggested art position is x=5, y=2.5. Its position does not restrict targeting or player movement. It creates a **vertical strike stripe** on the lane instead of moving projectiles. This deliberately simple enemy creates attention pressure without determining the full game's eventual bullet patterns or camera design.

When a warning begins, capture the player's current horizontal position as `strikeCenter`. Do not track the player afterward.

| Phase | Duration | What the player sees |
| --- | --- | --- |
| Warning | 0.90 s | A region 1.6 units wide, with a countdown or fill cue. |
| Strike | 0.15 s | The same region actually deals damage. |
| Recovery | 2.45 s | This attack has ended; wait for the next warning. |

After the initial 2-second grace, each cycle lasts **3.50 seconds** and captures the player's position again. PAUSED chanting freezes the phase and its captured center completely.

Overlap is `abs(playerX - strikeCenter) <= 0.25 + 0.80`. A center distance of **1.05 units or less** counts, including exact boundary contact. The displayed warning must match the same region; debug mode shows the actual player hurtbox.

A player at the warning center must move **more than** 1.05 units to escape. At 5 units/second, that takes just over 0.21 seconds, below the proposed 0.90-second warning. This proves geometric feasibility, not comfortable human reading and dodging. Both arena edges must leave an inward escape route, and both must be tested.

A damaging hit removes 1 HP, minimum zero; clears the current word's partial progress while retaining completed words; locks letters and spell starts for **0.35 seconds**; and grants **1 second of enemy simulation time** of invulnerability. RT movement remains available. There is no knockback, book drop, movement-animation lock, resource penalty, or cooldown extension.

Overlap during invulnerability does not remove HP, clear progress, or refresh reaction lock. Sort simultaneous hit candidates by attack ID and accept at most one damaging hit per tick. Reaching zero player HP immediately records `Lost`, removes hazards, and blocks any final letter not yet processed in that tick.

## D09 — How does typing feel like an attack?

The only spell is **Pulse**. It is free in this experiment: no energy, accuracy multiplier, manual target choice, range check, projectile travel time, or hit chance. Once completed, it cannot be interrupted. Finishing the phrase immediately removes 25 HP from the sole enemy, minimum zero. Visual travel is allowed but must not delay or repeat real damage. This isolates whether typing can feel like a combat action.

Every correct letter fills a progress segment; finishing a word gives a stronger saved-progress cue. Completing the phrase simultaneously produces:

| Feedback | Duration |
| --- | --- |
| Beam / impact effect | 0.20 s |
| Enemy-hit cue | 0.30 s |
| Damage number showing `25` | 0.60 s |

Do not add hit-stop that pauses gameplay or mandatory camera shake, which would change the threat rhythm being compared. Reduced-effects mode removes strong flashes and motion effects while keeping confirmation through outlines, labels, and similar cues.

Cooldown begins on the cast tick and lasts **0.60 seconds of encounter time**. Movement and enemy attacks resume during cooldown in both modes; letters and repeated starts are ineffective. When ready, show `1 — ready` and the next phrase preview; do not start automatically. Enemy HP reaching zero immediately shows victory, replacing cooldown and canceling later attacks. Typos cannot accidentally release a weaker spell.

## D10 — Presentation, sound, and readability

The baseline is **1280×720**; support **1920×1080** and at least **1024×576** without clipping. Baseline layout: top 15% for HP/mode/time, middle 60% for the lane and danger, bottom 25% for the phrase and controls.

| Element | Baseline size and requirements |
| --- | --- |
| Full phrase at the bottom | Centered, with character height at least 30 px. |
| Current word near/above the player | At least 32 px, with a backplate and clamped inside the screen. Must remain distinct from warnings and not obscure feet, hurtbox, or stripe edges. |
| Supporting labels | At least 20 px. |
| Text scale | 100% / 125% / 150%. Full phrases wrap only between words. Check every stated resolution/scale combination. |

Use clear, undecorated glyphs, especially distinct I/l/1 and O/0. Both text displays use the same actual progress: completed words have check marks; entered letters use fill or underlines; the next letter has a cursor or outline; remaining letters use a neutral style. Color supplements these cues and cannot be the only signal.

On a typo, keep the phrase in place, briefly mark the wrong letter separately, and move the cursor back once. **The error decoration/cue lasts 0.35 seconds, but the expected letter and cursor must immediately follow current progress.** If more correct letters arrive during that cue, progress must advance visibly without waiting for the cue to finish. Do not insert mistakes into an editable sentence. Hits need a different message, such as `Hit: finish this word again`, lasting **0.80 seconds** without covering the phrase.

Ready, chanting, reaction lock, cooldown, pause, and results must be visually distinct. Label modes `REAL TIME — keep dodging` or `COMBAT PAUSED WHILE TYPING`. Teaching shows `No spaces or Enter needed`, with an option to keep that hint visible. Menus explain that holding a letter does not repeat it. Results show only win/loss/time limit, successful casts, typos, damaging hits, encounter time, and mode; no leaderboard or letter grade.

Correct letters, completed words, typos, warnings, hits, and casts each need distinguishable, nonharsh sounds. Correct-letter audio allows at most four simultaneous voices, replacing the oldest when exceeded; fast typing cannot accumulate unlimited sound effects. After logic consumes input, correct/error progress must be visible in the **next rendered frame**, with no extra artificial delay. This is not a claim about total hardware keyboard-to-screen latency.

Warnings remain visible throughout their warning phase. All essential information must be understandable while muted. Before combat, allow master volume, effects volume, mute, text scale, and reduced-effects settings; preserve them across runs.

Defaults: **1280×720 windowed, target 60 FPS, English interface, 100% text scale, master and effects volume both 0.7, mute off, reduced effects on, persistent hints on.** Volume ranges from 0 to 1, with zero silent; unmuting restores saved volume. 30/120 FPS are acceptance conditions, not changes to typing speed. A target frame rate is not proof that it was achieved. Do not require flashes, shake, color-only states, or audio-only warnings.

If baseline controls are unsuitable for someone, offer PAUSED/practice exploration and record the limitation. Do not claim v0.1 is universally accessible. Arbitrary remapping, one-handed layouts, controllers, and other devices need separately defined experimental conditions instead of unrecorded substitutions during measured runs.

## D11 — Default configuration to implement

This is the first implementation contract. Tunability does not permit substituting different defaults during implementation. Save the configuration version and hash for each run. When changing the baseline, update both documents and increment the configuration version. Durations are seconds; positions and widths are logical world units.

```yaml
designVersion: 0.1
configVersion: typing-v0.1
simulationHz: 60
primaryMode: RT
comparisonMode: PAUSED
initialCountdown: 3.0
resumeCountdown: 1.0
encounterLimit: 120.0
hitchPauseThreshold: 0.250
player:
  hp: 3
  startX: 0.0
  speed: 5.0
  hurtboxHalfWidth: 0.25
arena:
  minX: -6.0
  maxX: 6.0
enemy:
  hp: 100
  initialGrace: 2.0
  telegraph: 0.90
  strike: 0.15
  recovery: 2.45
  strikeHalfWidth: 0.80
  damage: 1
  reactionLock: 0.35
  invulnerability: 1.0
spell:
  damage: 25
  cooldown: 0.60
  typoRollbackLetters: 1
  checkpoint: word
  spacesRequired: false
  caseSensitive: false
  autoCast: true
  perPhraseDeadline: null
feedback:
  typoCue: 0.35
  hitMessage: 0.80
  beam: 0.20
  enemyHit: 0.30
  damageLabel: 0.60
presentation:
  width: 1280
  height: 720
  fullscreen: false
  requestedFps: 60
  interfaceLanguage: en
  textScale: 1.0
  masterVolume: 0.7
  effectsVolume: 0.7
  muted: false
  reducedEffects: true
  persistentHints: true
```

Validation must require positive, finite durations/speeds/widths, except the intentionally null per-phrase deadline; finite and correctly ordered arena bounds; a starting player hurtbox fully inside the arena; nonempty phrase banks; and positive integer starting HP and combat damage. HP can reach zero during play, but starting HP cannot be zero. Volumes must be finite within [0,1]; text scale must be 1/1.25/1.5; declared acceptance frame-rate targets must be 30/60/120. Baseline durations are whole 1/60-second ticks. Revisions must also align or explicitly define rounding in a new version. Invalid configuration shows a setup error and blocks starting; it must not silently become a different, unrecorded experiment.

Possible later tuning ranges are warning 0.7–1.2 seconds, recovery 1.5–3.5 seconds, 8–18 letters per phrase, and typo rollback of zero or one letter. These are future experiment ranges, not random values per run. Change one main factor at a time in the next comparison. Slow motion, book pickup, longer or harder language, and more spells remain future hypotheses.

## D12 — Edge cases that are easy to miss

- Unsupported text, paste/composition, and ignored punctuation neither advance nor penalize progress and cannot take effect later. Show the explanatory hint at most once every **2 seconds of focused UI time**.
- Process rapid real presses, repeated letters, and multiple presses in one rendered frame in order. Do not solve input problems by deleting adjacent identical characters or imposing a typing-speed cap.
- Keys held across pause, start, or retry must be released and pressed again. Disconnecting a keyboard must not leave movement stuck. Reconnecting must not change the phrase or reset HP.
- When pause, damage, and input coincide, pause prevents both damage and input. The resume-confirmation tick has neither damage nor text input. Preserve the exact warning position/phase, then run the full resume countdown.
- A new keyboard replaces a disconnected one only through the resume process. Clean up old-device listeners and queues. Two-keyboard assistance is outside the baseline; record device changes.
- Escape always pauses; it does not cancel a phrase. There is no free cancel or phrase-reroll command. The player may stop typing and keep dodging or end the run through the pause menu.
- Invalid/empty phrases or missing required UI/input assets show a setup error and block starting. Never auto-win, silently substitute invisible text, or accept an empty phrase.
- Completion and attack IDs ensure duplicate callbacks take effect only once. Delayed events from an earlier run cannot affect a retry. Unsubscribe input and clear audio, visual objects, and queued events when leaving the scene.
- Do not simulate in the background during focus loss, missing keyboard, or performance pause. **Logging failure is different:** keep the game playable, mark `LoggingIncomplete`, and warn that the run cannot support formal analysis.
- Once produced, the result cannot change because of later collisions, input, clocks, or a held retry key. Only an explicit retry creates a new run; preserve settings and reset gameplay state.

## D13 — Forty-two acceptance checks

These are **requirements for a future implementation, not a report of passed tests**. Logical checks inspect exact state; input checks must exercise actual input handling; visual checks use a visible standalone build. Label automated fixtures separately from human playtests.

| ID | Action | Required observation |
| --- | --- | --- |
| AT01 | Start a fresh fixture run, press 1, and type P0 | Accept 11 letters, cast once, enemy 100→75, no Enter. |
| AT02 | Type P0 with mixed Shift/Caps case | Same progress and cast as lowercase. |
| AT03 | Type `bu`, then X | `burn` progress 2→1, typo count +1, HP unchanged. |
| AT04 | Complete `burn`, then X at the start of `the` | Previous word remains saved; current progress stays zero. |
| AT05 | Add spaces/punctuation/digits/Backspace/Enter | No progress, HP, or typo-count change; no premature cast. |
| AT06 | Hold the final required letter | Accept once per physical press; no repeated cast. |
| AT07 | Use fixture `feel the heat`, releasing before the second E | Both E presses count; holding one E does not count twice. This fixture does not replace a measured bank. |
| AT08 | Start and then type the first letter in the same update | Process the first letter exactly once. |
| AT09 | Enter multiple ordinary physical letters in one rendered frame | Ordered progress matches the same timestamped inputs at higher FPS. |
| AT10 | Complete a phrase with trailing text/start events in the same batch | One cast; discard extra text; do not buffer starts during cooldown. |
| AT11 | Hold 1 through the end of cooldown | No new attempt until release and a fresh press. |
| AT12 | Paste, IME commit, unsupported Unicode, Ctrl+letter | No progress or typo damage/penalty; clear unsupported-input feedback. |
| AT13 | Type A/D/J/K/E/R in a phrase | Text only; no movement, melee, parry, interaction, or retry. |
| AT14 | Hold both directions, then release one | Initially stationary, then move in the remaining direction at 5 units/s. |
| AT15 | Type while moving in RT | Both work; stopping or moving retains text progress. |
| AT16 | Attempt movement during PAUSED chanting | Position/enemy phase freeze; typing and encounter limit continue. |
| AT17 | Enter cooldown in both modes | Combat resumes; ready after 0.60 s; damage applied once. |
| AT18 | Move away after a warning starts | Stripe center does not track; escaping before the strike avoids damage. |
| AT19 | Stand just outside, exactly on, and inside the overlap boundary | Outside misses; boundary/inside hit; each attack damages at most once. |
| AT20 | Start warnings at both arena ends | Clear inward escape route; player center stays in bounds. |
| AT21 | Get hit after partially typing the second word | HP 3→2; first word saved; second cleared; 0.35 s typing lock; RT movement works. |
| AT22 | Overlap repeatedly during invulnerability | No further HP loss, progress reset, or lock refresh for 1 s of enemy time. |
| AT23 | Hit and final letter in the same tick | Hit wins; no cast; lethal damage ends the run. |
| AT24 | Deadline and final letter in the same tick | `TimeLimit`; no cast after the deadline. |
| AT25 | Pause and hit/text in the same tick | Pause wins; no damage/progress; resume does not replay queued letters. |
| AT26 | Switch away while holding an arrow or letter | Pause; return requires release, confirmation, and 1 s countdown; no stuck keys. |
| AT27 | Lose focus during manual pause or countdown | Cannot cancel the existing pause or shorten the resume countdown. |
| AT28 | Disconnect and reconnect the keyboard midword | Retain progress; no simulation without a device; require the resume process again. |
| AT29 | Complete four spells / take three damaging hits | Exactly one `Won` / `Lost`; later attacks and input are ineffective. |
| AT30 | Retry after each result and restart from the pause menu | Reset all gameplay state/statistics; retain condition; create new run ID. |
| AT31 | Inject duplicate/stale cast and hit events | No duplicate damage and no mutation of a newer run. |
| AT32 | Replay one timestamped scenario at 30/60/120 FPS | Identical logical result, cursor, and HP; feedback appears in the next rendered frame after processing. |
| AT33 | Introduce a simulation gap over 250 ms | Visible performance pause; no hidden attack fast-forward; flag data quality. |
| AT34 | Run both banks | Correct order/counts; typos/hits do not change phrases; retry begins with first entry. |
| AT35 | Omit phrases or use invalid phrases/configuration | Visible setup error; starting is blocked. |
| AT36 | Test 1024×576, 1280×720, 1920×1080, each at three text scales | Phrase, current letter, player, stripe edges, and status remain legible and unclipped. |
| AT37 | Mute and enable reduced effects | Correct/error/hit/attack/ready/pause remain visually distinguishable. |
| AT38 | Type rapidly and repeatedly load/unload the scene | No sustained growth in listeners, objects, or audio voices; no dropped/duplicate presses. |
| AT39 | Simulate logging failure and process interruption | Keep playing with a warning, or classify unfinished records as Interrupted; never report false success. |
| AT40 | Compare telemetry with a known short sequence | Clocks, correct/wrong counts, completed attempts, damage, and unique result match the model. |
| AT41 | Exercise all three practice stages, including next/restart/leave/death | Follow D03's active systems, immortal dummy, training-hit and transition rules; no practice state reaches a measured run. |
| AT42 | Load saved configuration; check zero-time/no-cast/excluded records | Hash matches original bytes; reproduce the same configuration and corpus; undefined metrics are N/A; invalid pairs are outside n. |

Time-based checks replay **the same timestamped input stream**, not one character mechanically injected per rendered frame. Calling `AcceptLetter` directly verifies typing logic, not real keyboard routing. A person saying it feels fine cannot replace input-origin tests either. Conversely, passing all 42 checks only opens the gate to human experience testing.

## D14 — What to record and how to interpret it

Keep JSONL records, one event per line, only under local, Git-ignored `artifacts/typing-combat/`. No network uploads, account identifiers, names, clipboard contents, or arbitrary raw keystrokes. Labels such as P01 are local participant IDs. Save phrase IDs and progress changes, not complete raw typed text.

Also save a short local session manifest with condition order, self-reported typing/English familiarity, accessibility settings, consent to local notes, and observer notes. Do not commit it to Git.

| Metric | Exact calculation / meaning |
| --- | --- |
| Input accuracy | Accepted correct-letter events ÷ (accepted correct + wrong supported-letter events). Retyped correct letters count as events; ignored input does not. Zero denominator is `N/A`. |
| Productive letter throughput | Letters in completed phrases ÷ encounter seconds. Zero encounter time is `N/A`. This is neither accuracy nor standardized WPM. |
| Completion time | Encounter time at the first terminal result, with the outcome stated. Retain wall time and enemy time too. |
| Exposure-adjusted hits | Damaging hits ÷ enemy seconds × 60, reported alongside enemy seconds. Zero exposure is `N/A`. PAUSED hits per wall minute do not represent an equal-threat comparison with RT. |
| Attempt outcomes | Completed; still open at death/time limit; aborted. A typo or hit does not create a new attempt. |
| Other separate counts | First-cast time (`N/A` if none), total casts, typos, damaging hits, ignored input, and word resets caused by hits. |

Flag coaching during measured attempts, device/layout changes, performance pauses, changed configuration, incomplete logs, and interruption. Keep these records visible but exclude them from the primary mode comparison and explain why. Report manual/focus pauses; using them strategically for thinking time invalidates timing comparison.

Software records cannot establish whether someone looked down at the keyboard or why they stopped typing. Observe and ask instead of treating log-based guesses as facts.

<details>
<summary>Complete logging fields for implementation and review</summary>

Initial `schemaVersion` is integer 1. Before a run, save the resolved configuration as **UTF-8 JSON without BOM**, including all D11 values, corpus version and complete phrase records, selected condition/bank, and effective input/accessibility/display/audio settings. Calculate lowercase SHA-256 over the exact saved bytes as `configHash`, excluding the hash itself. Save that snapshot once, named by its hash, in the local output directory and reference it from each run. Reproduction reads those same original bytes, not an unspecified reserialization. Changing mode or settings therefore changes the snapshot hash even when the design remains version 0.1.

Every event includes `schemaVersion`, `designVersion`, `configVersion`, `configHash`, `buildCommit`, `sessionId`, `participantLabel`, `runId`, `condition`, `bankId`, `practice`, `seq`, `tick`, `wallSeconds`, `encounterSeconds`, `enemySeconds`, `event`, and `payload`. These identify format, design/configuration/build, session/participant/run, mode/bank/practice status, order/tick, the three clocks, and event contents. Run-start also includes resolution, display scale, declared input layout, and settings.

Event types are `RunStarted`, `AttemptStarted`, `LetterAccepted`, `Typo`, `InputIgnored`, `WordCompleted`, `SpellCast`, `ThreatStarted`, `PlayerHit`, `PauseStarted`, `ResumeCompleted`, `PerformancePause`, `RunEnded`, and `LoggingError`. Relevant payloads contain attempt/cast/attack IDs, phrase ID, cursor before/after, HP before/after, and a categorized reason. The actual wrong letter need not be recorded.

Use one writer with increasing sequence numbers. Flush buffered data to disk at run start, run end, and pause; cap in-memory buffering. Write the end result atomically once; ignore duplicate finishes. Missing end, sequence gaps, or write failures mean incomplete data, never invented missing values. CSV summaries may be derived from JSONL but must not hide incomplete source events.

</details>

## D15 — How to run human playtests

Start with **4–6 people**, including comfortable touch typists and slower typists or people who look at the keyboard. Include people who did not implement the prototype when practical. This is a directional usability test, not a statistically powered study. Record familiarity instead of inventing a universal qualifying typing-speed cutoff.

Keep the build, configuration, keyboard, display, and volume policy consistent where practical. Explain controls and word checkpoints once using P0, then practice typing only for about **60 seconds**, dodging only for **30 seconds**, and combined RT for **60 seconds**. At that point, check whether the participant can explain auto-cast, typo rollback, and restarting the current word after a hit. **Record this first comprehension assessment before any remedial teaching**; it supplies D16's comprehension metric. Teach again if needed and record extra practice, but later learning does not turn the first failure into a pass. Once taught, the participant may still perform uncoached measured runs. Do not mislabel an instruction failure as dislike of the gameplay.

Next, each person completes one uncoached RT encounter and one PAUSED encounter, each ending on win, loss, or the 120-second limit. A break between conditions is allowed. Rotate condition and bank order:

| Participant position | First encounter | Second encounter |
| --- | --- | --- |
| 1 | RT / A | PAUSED / B |
| 2 | PAUSED / B | RT / A |
| 3 | RT / B | PAUSED / A |
| 4 | PAUSED / A | RT / B |

Repeat the rotation for more participants. Do not always test RT first or attach one bank permanently to one mode. Equal phrase lengths can still differ in familiarity, so report actual allocation and do not claim perfect isolation of every variable. Do not tune during encounters. Display accessibility settings may differ by participant but must remain fixed across each person's pair and be recorded.

If someone cannot use baseline controls, allow practice/comparator exploration and record the access limitation separately instead of forcing a standard trial. Anyone who wants to stop due to discomfort may stop immediately; retain discomfort as a usability finding, not a software error.

After each encounter, ask:

1. What caused the last typo or hit, and what would you change next time?
2. Rate control comfort, perceived fairness, and desire to replay from 1–5 each: 1 means very poor/none; 5 means very good/strong.
3. Describe a moment when you deliberately chose to keep typing or dodge first.

After both, ask which mode they prefer and why. The observer records missed warnings, uncertainty about the next letter, hand discomfort, purposeful pauses in typing, and voluntary requests to replay. Questions must not imply that RT is the correct answer. Automated fixtures cannot replace these actual encounters, and planned sessions must not be reported as completed.

## D16 — How to decide whether to continue, revise, or test again

**The first gate is reliable functionality.** Every applicable AT01–AT42 check needs passing evidence at its stated scope. Reproducible dropped/duplicated ordinary presses, false casts, unreadable required cues, old-run events affecting new runs, or unavoidable baseline attacks block the experience study until fixed. Missing real-input or visual evidence is incomplete, not a pass.

**The second gate is a promising experience.** Fix these initial team criteria before data collection rather than selecting criteria afterward. They are provisional design thresholds, not accepted scientific standards. `n` counts only people with **usable records for both RT and PAUSED**. Separately report excluded, interrupted, or discomfort-stopped participants and observations; being outside n does not remove them from the report. Require at least **four usable paired participants** before applying these proportions. `ceil` means round upward to an integer.

| Criterion | Initial threshold |
| --- | --- |
| Rule comprehension | At the first assessment after standard practice and before remedial teaching, at least `ceil(0.8 × n)` can explain auto-cast, typo letter rollback, and restarting the current word after a hit. |
| Meaningful combat choices | At least `ceil(0.6 × n)` complete at least one RT spell and can describe a deliberate type/dodge choice. |
| Physical comfort and replay interest | Median RT comfort is at least 3/5; at least `ceil(0.6 × n)` rate RT desire to replay ≥3/5. Report individual ratings alongside the median. |

With six valid pairs, the 80% criterion needs at least five people and the 60% criteria need at least four. Recurring hand discomfort remains a design issue even when a majority passes. Passing does not demonstrate broad appeal.

Interpret results carefully. If input works but rules are unclear, revise cues/teaching first. If players understand but cannot find typing windows, change one main factor next, such as a threat-timing parameter or phrase length. If hand switching is uncomfortable, define a separate remapping/slow-motion experiment. If players consistently prefer PAUSED, retain that finding instead of forcing RT to win. A small or inconclusive sample calls for another focused test, not a success claim.

Only after the typing interaction passes these gates should another experiment reconnect parry-earned energy and spell selection. That experiment must separately specify when charges are consumed, whether cancellation/refunds are possible, and how failures affect energy. Existing full-game intentions cannot silently apply to this free-cast lab. Do not add boss chapters to conceal weak typing feel.

## D17 — Existing-code differences and implementation order

The inspected legacy baseline is commit `ae97f0e`. [ValidationGameState.cs](../Game/Assets/_Game/Validation/ValidationGameState.cs) uses two case-/punctuation-sensitive sentences, a 12-second limit, energy payment when typing starts, editable text, and Enter submission. [ValidationWorld.cs](../Game/Assets/_Game/Validation/ValidationWorld.cs) freezes the player and enemies during typing; existing tests explicitly assert those behaviors. [Previous validation results](validation-results.md) apply only to that earlier prototype, not this design.

Create a future `Game/Assets/_Game/Scenes/TypingCombatLab.unity` scene and separate `TypingCombat` source/test areas while keeping the old scene runnable. **These are planned paths, not files delivered by this documentation change.** Reuse the pinned Unity/URP/Input System setup, sprite fixture, build infrastructure, and testing methods. Do not inherit the old state model and accidentally retain its freeze/Enter rules.

Separate at least the following responsibilities so failures can be located; players do not need to understand the internal structure.

| Part | Responsibility and boundary |
| --- | --- |
| Input adapter | Physical presses, text/control routing, timestamps, layout checks, repeat filtering, and device lifecycle. Emits events; does not change HP or text progress. |
| Typing model | Fixed phrase and word/letter positions; correctness, typo, checkpoint, and completion decisions. No UI dependency. |
| Encounter coordinator | D06–D09 state, fixed ticks, event priority, threats, position, HP, and exactly-once effects. |
| Presenter | Both text displays, warnings, HP, cues, menus, and accessibility, based on read-only state. Does not decide whether a letter is correct. |
| Local recorder | Ordered events, quality flags, and run summaries. Write failures cannot change combat rules. |

Implementation order and exit evidence:

1. **Start with a real-input spike.** Build a visible typing-only fixture and combine real-keyboard use with input-adapter tests for held keys, repeated letters, fast batches, case, shortcuts, and unsupported input. If input origin and physical presses cannot be correlated reliably, fix that here instead of hiding it with a typing-speed cap.
2. **Complete the typing loop.** Add word checkpoints, one-letter typo rollback, automatic exactly-once Pulse, and cooldown. Verify the state examples before adding threats.
3. **Add combat pressure.** Horizontal movement, warning stripe, HP/hit reaction, deterministic event order, pause/retry, and both modes.
4. **Finish presentation and evidence.** Complete feedback/accessibility, local records, standalone acceptance, and reviewed representative screenshots.
5. **Run actual player sessions.** Execute D15, report observations separately from the design, apply D16, and version the next revision.

The same boundaries apply to AI implementation: implement only this experiment and treat definitive rules/configuration as requirements. Record deviations before making changes; do not silently replace the interaction. Do not claim unrun acceptance checks or human tests, and do not add paid services or external assets. A technical obstacle may be reported as a blocked requirement; it does not justify substituting easier-to-pass gameplay. Keep new tests separate and retain legacy validation tests. Gameplay changes follow the repository's PR/check/merge policy. This documentation change neither implements new gameplay nor provides authorization or evidence to claim that it has been validated.

Suggested coordination consistent with current roles: Vincent organizes observations and UI clarity; Ace contributes meaningful phrase writing and audio feedback; Andrew integrates input/combat; Michelle handles readable visual cues. Programming remains shared. These are work areas, not new approval gates, and no assignments have been sent to anyone.

## D18 — Keeping both documents aligned, and what remains unknown

The implementation specification and this plain-language guide describe one design. Neither may silently override the other. If they conflict, pause the dependent implementation decision, reconcile both documents, and increment design/configuration versions as appropriate. This guide may use less code-like language, but it must retain every gameplay consequence, default value, exception, test category, and decision gate.

| Matching IDs | Content required in both documents |
| --- | --- |
| D01–D03 | Goal, evidence limits, scope, modes, complete loop. |
| D04–D07 | Controls, text matching, states/clocks, event precedence. |
| D08–D12 | Geometry/hits, damage/reward, audiovisual feedback, exact configuration, edge cases. |
| D13–D16 | Acceptance, metric definitions, human protocol, interpretation. |
| D17–D18 | Existing-code gap, implementation sequence, version parity, current status. |

Before release, check: both have all 18 identifiers; banks and letter counts match; defaults and time units match; examples follow input rules; hit/final-letter and deadline precedence agree; PAUSED is not treated as equal enemy exposure; old test success is not relabeled as new evidence; source-confirmed facts stay separate from proposed rules; no provisional choice is described as known Textorcist behavior.

What remains missing is **observed evidence**, not rules for implementers to guess: whether one-letter rollback works well, phrase familiarity, comfortable warning/recovery timings, readability of the word near the player, keyboard/layout behavior on teammates' machines, accessible-control needs, RT versus PAUSED preference, and future interaction with parry energy. A first experiment or follow-up is defined above for each. Answers must come from implementation evidence and playtesting, not from finishing the document.

Change log: **0.1 — first sourced complete specification and matching plain-language guide.** No new gameplay delivery, passed acceptance results, or completed player study is claimed.
