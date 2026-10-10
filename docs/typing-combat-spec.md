# Typing combat experiment — implementation specification

Version **0.1**, 9 October 2026. Status: **design for implementation and playtesting; not implemented or playtested**.

Audience: AI implementers, programmers, and design reviewers. Equivalent Chinese reading edition: [打字战斗设计说明](typing-combat-guide.zh-CN.md). Both editions cover D01–D18. This document specifies a new experiment, not the behavior of the existing `Validation` scene. It does not replace the larger game's approved scope.

## D01 — Objective and scope

**Question:** does reading and entering a short incantation while reacting to a visible threat produce understandable, satisfying combat decisions?

Validate the complete interaction: notice a threat, choose when to begin or continue a phrase, enter letters, recover from mistakes or hits, complete the phrase, see an immediate attack, then repeat. Typing accuracy alone is not the desired experience. Neither automated correctness nor resemblance to another game establishes enjoyment.

The deliverable for the future implementation is one separate Windows keyboard-only laboratory scene, one enemy, one spell, practice, two comparable timing modes, results, local measurement, and its acceptance tests. Flat horizontal movement preserves the project's side-view context. No jumping or platform navigation is needed to answer this question.

Exclude parry/energy prerequisites, ordinary melee, multiple spell selection, inventory, book retrieval, puzzles, pickups, narrative progression, final art, procedural content, localization of incantations, gamepad play, networking, and cloud analytics. These are deliberate experiment boundaries, not cancellations of the full game's features. Character feedback, clear threat cues, and attack feedback are included because they affect the interaction itself.

No purchase, installation, or hands-on observation of The Textorcist was performed for this document. Reference research is document-based. All numerical defaults below are **Keyboard Warrior experiment decisions**, not extracted Textorcist values or proven balance.

## D02 — Reference research and evidence boundary

The design borrows principles, not undocumented rules or copyrighted assets. Access date for the following sources: 9 October 2026.

| Source | Source-confirmed information | Design implication, explicitly an inference |
| --- | --- | --- |
| [S1 — Headup official game page](https://www.headupgames.com/game/the-textorcist-the-story-of-ray-bibbia) | Typing and bullet dodging coexist; keyboard and gamepad experiences differ. | Test divided attention with an actual threat and an actual keyboard. |
| [S2 — Steam, About This Game](https://store.steampowered.com/app/940680/The_Textorcist_The_Story_of_Ray_Bibbia/) | Combat includes typing exorcisms, English/Latin, and boss-specific variations. | Start with familiar English before adding linguistic difficulty. Store user reviews are not official specifications. |
| [S3 — Let's Jam!, developer Diego Sacchetti](https://www.codemotion.com/magazine/frontend/gamedev/lets-jam/) | Early testers watched bottom-screen text instead of their character; an overhead current-word display helped. The project evolved from a jam prototype to a design document and a polished boss prototype with public feedback. | Display both the whole phrase and a nearby current word; use playable experiments to inform further design. This is a historical account, not current parser documentation. |
| [S4 — Direct designer interview, page 3](https://david-bailly.com/portfolio/typing-games-how-and-why/3/) | Meaningful phrases fit the task; danger complicates recitation; arrow-key hand switching was intentional. | Preserve meaning and study hand switching instead of assuming it is comfortable. |
| [S5 — Direct designer interview, page 11](https://david-bailly.com/portfolio/typing-games-how-and-why/11/) | Language changes and boss-specific text disruptions vary difficulty. | Keep those variables out of the first comparison. |
| [S6 — Direct designer interview, page 16](https://david-bailly.com/portfolio/typing-games-how-and-why/16/) | Localizing spell text changes challenge and progression. | Separate interface language from the English prompt corpus. Do not treat translated phrases as mechanically equivalent. |
| [S7 — Official Steam updates 1–3](https://store.steampowered.com/news/posts/?appids=940680&enddate=1552657509&feed=steam_community_announcements) | Updates added Shift plus customizable movement keys, adjusted ambiguous overhead letters/text styles, fixed some fast-typing input lag, and fixed a Bible leaving the screen. | Explicitly test input speed and readable glyphs. An old fix does not establish current behavior in every configuration. |
| [S8 — Developer-marked control-scheme reply](https://steamcommunity.com/app/940680/discussions/0/3203652426708107869/) | Zoltac's developer reply confirms analog-stick movement alongside keyboard typing. | Alternative controls are a later experiment; other replies are player opinions. |

**Not verified by these sources:** exact typo rollback, word checkpoints, space/case/punctuation handling, Enter versus automatic firing, damage per letter/word/phrase, book drop/recovery distance or grace time, hit ordering, universal spell countdown, focus handling, or event buffering. Reviews offer leads but do not settle these details. Do not describe D04–D14 as a reproduction of those internals. A future fidelity study would require direct observation of an identified game version.

The following are project hypotheses awaiting playtesting: one-letter rollback is understandable, a three-word phrase is manageable, visible word checkpoints soften interruption, immediate phrase completion feels like an attack, and the chosen threat leaves useful typing opportunities.

## D03 — Playable loop and modes

The session flow is `Setup → Practice → Ready screen → 3-second countdown → Encounter → Results`. A results-screen retry returns to the Ready screen with the same condition. Changing condition or accessibility settings is allowed only outside an encounter and creates a new run record.

At encounter start: player at x=0 with 3 HP; enemy at 100 HP; no phrase active; no held input carried over. The first warning starts after 2 seconds of enemy simulation. Press **1** to ready the one spell. The displayed phrase stays fixed for that attempt. The final correct letter automatically deals 25 damage. A 0.6-second cooldown follows, then a new, fresh press of 1 can start the next attempt. Four completed spells win. Three damaging hits lose. Reaching 120 seconds of active encounter time produces `TimeLimit`, a neutral incomplete result rather than a death.

Two conditions use identical content, damage, input rules, and threat parameters:

| Condition | While chanting | Purpose |
| --- | --- | --- |
| `RT` — primary design | Enemy and player movement continue; stopping typing does not stop combat. | Evaluate timing, divided attention, and simultaneous input. |
| `PAUSED` — comparator | Enemy phase, damage checks, and player position freeze for the entire chanting phase; text entry continues. | Estimate the effect of suspending combat during typing. This includes movement as part of the combat-freeze policy. |

Outside chanting, both conditions advance combat normally. `PAUSED` is not implemented by substituting a slower enemy, easier words, or different error rules. Its lower exposure to attacks is expected and must be measured separately (D14). Neither condition has a per-phrase countdown. The 120-second encounter limit continues during chanting in both conditions.

Practice has three explicit steps. Every step shows `PRACTICE`, uses the same input/pause/feedback rules, starts with the 3-second countdown, and has no 120-second limit. Its elapsed clocks and events are marked practice and excluded from measured encounters. No hidden difficulty adaptation operates during measured encounters.

| Practice step | Active systems and health | Progression |
| --- | --- | --- |
| Typing only | Repeat P0 after every successful cast. Player position is fixed; no enemy attacks. Spell/cooldown work normally. Dummy starts at 100 HP and instantly resets to 100 whenever damage would reduce it to zero; it never wins/ends practice. Player remains at 3 HP. | Continue until the facilitator/player selects Next step. |
| Dodging only | RT movement and the same warning/strike cycle. Spell start and all letters are disabled; no prompt is active. Hits show feedback, count a practice hit and grant normal invulnerability, but do not subtract the displayed training HP. | Continue until Next step. |
| Combined RT | Repeat P0; all RT rules including player 3 HP, word reset and reaction lock. Dummy resets as above instead of producing a win. Player death opens practice Results; Enter restarts this step cleanly. | Next step finishes practice and opens the measured Ready screen; no practice state carries over. |

Escape pauses each practice step and exposes Resume, Restart step, Next step, Return to setup, Exit. Up/Down and Enter select actions. On practice Results, the same actions except Resume are available, with Restart selected by default. Restart repeats the same step; Next advances even if the suggested teaching duration is not reached. Teaching durations in D15 are a facilitator protocol, not automatic timers. Practice runs have a `practiceStage` field; their resets create new run IDs but no recorded measured failure penalty.

## D04 — Controls and accepted input

| Input | Encounter behavior |
| --- | --- |
| Left/Right arrows, held | Horizontal movement, including during RT chanting and hit reaction. Both held means zero movement. |
| Top-row 1 or Numpad 1, new press | Start a phrase only in `ReadyToCast`; ignored during chanting, reaction lock, cooldown, or terminal state. |
| A–Z/a–z text produced by a physical key press | Case-insensitive letter matching while chanting and not reaction-locked. All letters, including A/D/J/K/E/R, belong to typing; none activate legacy combat actions. |
| Space, punctuation, digits other than the start command | Ignored for progress and error penalty. Log an ignored-input category; do not silently insert them. |
| Backspace/Delete | No editing and no rollback command. Progress is a game cursor, not a text field. |
| Enter | No submit action during combat. On menus, confirm the selected item; on Results, retry after all previous keys have been released. |
| Escape | Pause; when already manually paused, request resume. Never cancel the phrase. |
| Up/Down arrows | Menu navigation outside encounters; no encounter action. |
| Shift/Caps Lock | Do not change letter identity. Shift is not a movement modifier in v0.1. |
| Ctrl/Alt/Windows-key shortcuts, Tab, function keys | Never type letters or trigger spells. OS shortcuts remain OS behavior; switching away pauses as below. |

The supported baseline environment is Windows with an English US keyboard/input layout. Text matching must use the produced Latin character rather than assume physical key names match every layout. Other layouts, IME composition, dead-key sequences, and non-Latin text are unvalidated in v0.1. Unsupported text makes no progress and causes no typo penalty; show a rate-limited instruction to switch to English input. Do not convert unknown Unicode characters to approximate Latin letters. Interface labels may be Chinese or English; the experiment corpus remains unchanged.

No clipboard ingestion, paste, autocomplete, speech input, or OS key-repeat advancement. Holding a letter produces one logical letter until release and another press; a repeated letter in a word requires two presses. Holding 1 must never repeatedly start attempts. The input adapter must associate accepted text with distinct physical press transitions without dropping fast adjacent presses. `onTextInput` alone is not proof that repeat/paste/origin filtering is correct: perform the input spike in D17 before relying on it. Reject unsupported composed/multi-character commits; distinct ordinary key presses arriving in one rendered frame remain valid and ordered.

All gameplay input enters a single ordered queue, not a UI text widget that separately changes the game. Movement events and typing events can coexist. Menus consume their confirmation event, clear pending gameplay events, and require release before that control can operate again. Once in an encounter, a start event followed by genuinely later letter-press events in the same input update is valid; do not discard the first letter merely because it shares a render frame with 1.

## D05 — Prompt data and exact progress rules

Each prompt is an immutable record `{id, version, displayText, words[]}`. Initial corpus version is `prompts-v0.1`; every listed prompt has integer `version: 1`. Valid corpus entries match lowercase ASCII words separated by single spaces, with no leading/trailing spaces or punctuation. Validation rejects empty words, nonletters, duplicate IDs, or disagreement between `displayText` and joined `words`. No runtime-generated or AI-generated prompt text is allowed in a measured run.

| Bank | ID | Display text | Required letters |
| --- | --- | --- | --- |
| Practice | P0 | `burn the seal` | 11 |
| A | A1 | `break the seal` | 12 |
| A | A2 | `light the path` | 12 |
| A | A3 | `guard the gate` | 12 |
| B | B1 | `break the lock` | 12 |
| B | B2 | `light the hall` | 12 |
| B | B3 | `guard the wall` | 12 |

These short original experiment phrases are placeholders for later narrative writing. Banks match word lengths (5/3/4), not proven lexical or motor difficulty. A run cycles its bank in listed order: 1,2,3,1 for four successful casts. A hit or typo never changes the prompt. Starting an attempt selects the current bank entry; successful completion advances the bank cursor. Retry resets the cursor to 1.

Represent progress with `wordIndex` and `letterIndex`, both zero-based. Completed words are checkpoints. A correct letter increments `letterIndex` by one. Completing a nonfinal word immediately increments `wordIndex` and sets `letterIndex=0`; no space or confirmation is required. Completing the final word emits one completion event (D09).

A wrong supported letter removes **one accepted letter of the current word**, with a floor of zero. It never undoes a completed word. The wrong letter is consumed once and is not reinterpreted after rollback. There is no HP damage, time lock, extra resource cost, or phrase replacement for a typo. The error cue identifies the new expected letter. Every subsequent physical letter is evaluated against that new cursor, even if the player had planned to type the old suffix.

| Example using `burn the seal` | Result |
| --- | --- |
| Start; press B, U | Current word `burn`, accepted prefix `bu`, next `r`. |
| Then X | Prefix becomes `b`, next `u`; X does not become a second input. |
| Then U, R, N | `burn` checkpoints; current word is `the`, prefix empty. |
| Press X at the start of `the` | Still at its first letter; `burn` remains complete. |
| Press Space or Backspace | No change. |
| Type `the`, then `seal` | Final L completes the phrase and attacks; Enter is unnecessary. |
| Correctly complete `burn`, then type `t`, then get hit | `burn` stays complete; `the` returns to empty; reaction lock applies. |

Idle time and movement retain all progress. There is no decay while merely waiting, moving, pausing, or looking at the threat. A damaging hit clears the current word's partial progress (D08); completed words remain. There is no dropped-book object in this experiment.

## D06 — State, clocks, pause, and recovery

Use an encounter state (`Countdown`, `Active`, `Paused`, `Results`), a cast phase (`ReadyToCast`, `Chanting`, `Cooldown`), and reaction/invulnerability timers. Do not encode chanting as globally inactive combat: RT explicitly keeps combat live.

| Event | Progress / cast phase | HP, enemy phase, timers |
| --- | --- | --- |
| Move or stop typing | Retained | RT combat continues; PAUSED chanting freezes combat only. |
| Wrong letter | Current word's letter index minus one, floor zero | All clocks otherwise continue. |
| Damaging hit | Partial current word cleared; phase unchanged | HP minus one, 0.35 s reaction lock, 1 s invulnerability. |
| Manual pause / focus loss / device loss | Retained; queued gameplay input discarded | All gameplay clocks and movement stop. |
| Resume | Retained; no old input replay | After release and a 1 s countdown, continue the exact saved enemy phase. |
| Successful cast | Completed attempt closed; cooldown begins | Enemy loses 25 HP exactly once. |
| Win, death, time limit, quit encounter | No further input or attacks | Result snapshot fixed; hazards and pending events cleared. |
| Retry | Empty; `ReadyToCast` | Both HP values, position, bank cursor, enemy phase, timers, queues, and run counters reset. New run ID. |

Clock definitions:

- **Wall clock:** monotonic elapsed time including pauses, used for diagnostics, never gameplay damage.
- **Encounter clock:** active encounter time including chanting, cooldown, and hit reaction; excludes initial/resume countdowns and all explicit pauses. End at 120 seconds.
- **Enemy clock:** time during which combat advances; equals encounter time in RT, but excludes chanting time in PAUSED. Attack phases and invulnerability use this clock. Player motion also uses this clock.
- **Reaction/cooldown clocks:** encounter time, so no deadlock if a reaction overlaps PAUSED chanting; they stop on explicit pause.
- **UI cues:** remaining durations stop on explicit pause; they continue during PAUSED chanting. Countdown uses its own focused, connected elapsed time.

Focus regain alone never resumes play. Loss of focus, keyboard removal, or a simulation hitch greater than 250 ms enters explicit pause with a visible reason. On return, require focus, a connected keyboard, release of held gameplay controls, and a fresh Enter/Escape confirmation; then a 1-second countdown. Resume input is consumed and cannot enter the phrase. A pause already requested manually cannot be overridden by focus recovery. During countdown, no characters, movement, damage, or spell start are accepted. Remember the countdown kind if it is interrupted: after confirmation restart the full 3 seconds for an initial countdown, or the full 1 second for a resume countdown. Do not shorten initial preparation. On entering Active, any gameplay key still held (including one pressed during countdown) remains suppressed until its release and a new press; it cannot start movement, type, or cast automatically. Menu/pause/confirm commands also use new press edges, never key repeat.

Paused-menu actions are Resume, Restart encounter, Return to setup, and Exit application. Up/Down select; Enter confirms; Escape requests Resume only when prerequisites hold. Restart records `Aborted` for the old active run and creates a clean run after the Ready screen. Returning to setup also records `Aborted`. No letter-key restart exists during gameplay. OS close safely ends the process; a flushed run-start without run-end is classified `Interrupted` on next analysis, not as a loss or success.

## D07 — Event ordering and timing invariants

The logical model advances at **60 Hz**. Inputs carry monotonic timestamps plus a sequence number. Convert timestamps to active encounter time by removing countdown/explicit-pause intervals; discard gameplay events occurring inside those intervals. Event time `t` belongs to tick `max(1, ceil(t × 60))`: tick n consumes events in `((n-1)/60, n/60]`, with a start-time event at zero assigned to tick 1. Preserve timestamp/sequence order within a category. Use integer tick indices for boundaries rather than floating-point equality tests.

Movement uses press/release transitions from that queue, not the keyboard's latest render-frame snapshot. Apply all movement transitions through this tick in order, retain the resulting held-state, then move once by `direction × 5/60`; a press and release within the same tick yield the final released state. This is the declared 60 Hz movement quantization. Render rate is independent. Catch up with normal fixed steps; never change spell length, speed, or correctness because rendering is 30, 60, or 120 FPS. A gap over 250 ms pauses and marks the trial as affected by performance, instead of fast-forwarding unseen attacks.

For each tick use this precedence:

1. Focus/device/hitch pause and explicit pause commands: freeze the tick, discard its gameplay inputs, and keep existing model state.
2. Determine this tick's destination encounter time before changing gameplay state. If it is at or beyond 120 seconds (tick 7200), set the clock to 120 and close with `TimeLimit`; apply none of this tick's movement, hits, or letters. Previously terminal states also reject all gameplay events. Thus a clock currently at 119 + 59/60 seconds cannot advance to 120 and still accept a cast.
3. Advance eligible clocks and expire timers. Apply movement, bounded to the arena, when combat is advancing.
4. Advance enemy phases and collect the tick's hit; resolve damaging hit/death before typing. Damage wins a same-tick tie against the final letter. Reaction lock rejects that tick's letters even if its duration would otherwise have been zero.
5. Process spell-start and character events in their timestamp/sequence order. A valid start can precede letters within this tick. Ignore letters while reaction-locked, not chanting, or terminal. Complete and apply a spell atomically; if the enemy dies, close with `Won` immediately.
6. Publish one immutable presentation snapshot and measurement events. Rendering cannot award progress or damage.

Timer intervals are half-open. For the threat's local phase time: telegraph `[0,0.90)`, strike `[0.90,1.05)`, recovery `[1.05,3.50)`. A strike checks the player's post-movement hurtbox on each strike tick until it damages them or ends; one attack ID can damage at most once. At the exact 120-second encounter boundary, the deadline rule takes precedence over timer progression and inputs. Exact-timestamp ties within input events use recorded sequence order; pause still has higher category priority.

A completion event has `{runId, attemptId, castId, tick}`. Each cast ID changes enemy HP at most once, including after duplicate callbacks. Immediately enter cooldown; discard remaining text events for that closed attempt. They cannot feed a future prompt. Do not buffer spell starts during cooldown/reaction, or auto-start when a held key becomes eligible. Menus, reloads, retry, and focus changes invalidate stale queued events through the run/attempt identity.

## D08 — Minimal arena, enemy, and hit rules

Use a fixed side-view orthographic camera and a flat lane; there is no scrolling, jump, gravity challenge, cover, or enemy body contact damage. Logical lane is x ∈ [-6,6]. Player hurtbox half-width is 0.25, so player center clamps to [-5.75,5.75]. Speed is 5 units/s with immediate direction changes and no acceleration. Facing is cosmetic. Fixed y, camera framing, and decorative depth must not change collision.

One stationary visible enemy sits above/right of the lane (suggested art position x=5,y=2.5); its position does not restrict targeting or player motion. It emits a vertical strike on the lane, not a moving projectile. This intentionally simpler attack supplies attention pressure without adding a bullet simulation or changing the full game's long-term camera design.

At telegraph start, capture the player's current x as `strikeCenter`. Do not track the player afterwards. Draw a warning region of width 1.6 and a countdown/fill cue for 0.90 s. Activate the same region for 0.15 s, then recover for 2.45 s. Repeat from a newly captured position. After the initial 2-second grace, cycle length is 3.50 s. In PAUSED chanting, the complete phase and captured center freeze.

Overlap is `abs(playerX - strikeCenter) <= 0.25 + 0.80`; touching the boundary counts as a hit. The rendered warning must correspond to that same region; show the player's actual hurtbox marker in debug mode. A player centered on a new warning needs to move more than 1.05 units to escape; at 5 units/s this takes just over 0.21 s of movement, within the proposed 0.90-second warning. This is a geometric feasibility check, not proof that humans can divide attention comfortably. Arena edges always leave an inward escape route. Test both edges.

A valid hit costs 1 HP, clamped at zero. It clears only the partial current word, preserves completed words, applies 0.35 s text/start lock, and gives 1 s of enemy-clock invulnerability. Movement remains available in RT. No knockback, book drop, animation stun of movement, resource penalty, or cooldown extension. Invulnerable overlaps produce no damage, no new progress reset, and no renewed lock. Simultaneous hit candidates are sorted by attack ID; accept at most one damaging hit per tick. At zero HP, close with `Lost`, clear hazards, and block a pending final letter.

## D09 — Spell and visible reward

The one spell, **Pulse**, has no energy cost, accuracy multiplier, targeting input, range check, projectile travel, miss chance, or interruption after completion. A completed phrase immediately subtracts 25 from the only enemy's HP, clamped at zero. Visual travel can be cosmetic but cannot delay or duplicate damage. This isolates whether typing can feel like a combat action.

Each correct letter fills one segment of the current phrase's progress indicator. Completing a word produces a stronger checkpoint cue. Completing the phrase produces a 0.20-second beam/impact, a 0.30-second enemy hit cue, and a visible `25` damage label lasting 0.60 seconds. No gameplay hit-stop or mandatory camera shake: otherwise the reward would also change the measured threat schedule. Reduced-effects mode removes bright flashes and movement effects while retaining outline/label confirmation.

Cooldown lasts 0.60 seconds of encounter time, starting on the cast tick. Movement and enemy attacks continue in both conditions during cooldown. Letters and repeated start commands do nothing during it. On expiry, show `1 — ready` and a preview of the next prompt; do not begin typing automatically. At enemy HP zero, victory immediately replaces cooldown and cancels all future attacks. A wrong key never produces a weak accidental spell.

## D10 — Screen, feedback, and accessibility

Reference canvas: 1280×720, scalable to 1920×1080 and at least 1024×576 without clipping. At the reference size, reserve top 15% for HP/mode/time, middle 60% for the lane and warning, bottom 25% for phrase and controls. Full phrase is centered in the bottom panel, at least 30 px; current word appears near/above the player, at least 32 px; secondary labels at least 20 px. Use readable nondecorative lettering with clear I/l/1 and O/0 forms. Support 100/125/150% text scale, wrapping whole phrases between words. Clamp the local word panel on-screen and give it a backing panel so it cannot visually merge with a warning. Do not cover the player's feet, hurtbox, or stripe edge. Inspect all specified resolutions/scales.

Both displays show the same authoritative cursor. Completed words have a check mark; accepted letters use a filled/underlined style; the expected letter has a caret/outline; remaining letters are plain. Color is supplemental. On error, keep the prompt in place, briefly mark the rejected letter separately, and animate the cursor back once. The error decoration/message lasts 0.35 s, but the caret and expected letter always follow the latest cursor immediately, including when later correct inputs arrive during that cue. Do not insert the bad letter into an editable sentence. On hit, distinguish `Hit: finish this word again` from `Wrong letter`; it remains visible for 0.8 s without hiding the prompt.

Ready, chanting, reaction lock, cooldown, pause, and results must be visibly distinguishable. State the experimental mode explicitly: `REAL TIME — keep dodging` or `COMBAT PAUSED WHILE TYPING`. Show `Spaces/Enter not needed` during teaching and as an optional persistent hint. The menu must state that holding a letter does not repeat it. Results show outcome, successful spells, typos, damaging hits, encounter time, and condition; no leaderboard or grade.

Provide distinct soft correct-letter, word-checkpoint, typo, threat-warning, hit, and spell sounds. Correct-letter voices are bounded (maximum four simultaneous voices, replace oldest), so fast typing does not create unbounded audio. Correct/wrong progress must update in the first rendered frame after the simulation consumes the event; any additional intentional input delay is forbidden. This is a software feedback requirement, not a claim of measured end-to-end hardware latency. Enemy warning remains visible throughout its phase. Essential cues also work muted. Separate master/effects levels, mute, text scale, and reduced effects are available before a trial and preserved between runs.

Baseline presentation defaults are windowed 1280×720, requested 60 FPS, English interface, 100% text scale, master/effects volume 0.7 each (normalized 0–1), mute off, reduced effects on, and persistent control hints on. Volume 0 means silence; mute restores the stored levels when toggled off. The 30/120 FPS settings are validation conditions, not a different typing speed. A requested FPS cap is not evidence of achieved performance.

No compulsory flashing, screen shake, color-only state, or sound-only warning. If standard controls are inaccessible, offer PAUSED/practice and record the limitation; do not claim v0.1 supports every player. Arbitrary remapping, one-handed layouts, gamepad, and alternative input devices need a separately defined condition before implementation; no silent substitution in baseline measurements.

## D11 — Configuration contract

The following snapshot is the initial implementation contract. Tunable does not mean the implementer may choose different defaults. Save a config version/hash with every run. All durations are seconds; all positions and widths are logical world units. Editing a baseline requires updating both documents and incrementing the config version.

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

Validation requires positive finite durations/speeds/widths (the deliberately null phrase deadline is exempt), ordered finite arena bounds, a starting player hurtbox fully inside those bounds, nonempty prompt banks, and positive integer initial HP and combat damage. Volumes must be finite in [0,1]; text scale must be 1/1.25/1.5; requested FPS must be 30/60/120 for the declared test conditions. Runtime HP may reach zero; initial HP may not. Baseline durations align to 1/60-second ticks; changes must retain tick alignment or explicitly declare the rounding rule in the new version. Invalid configuration stops at Setup with an error; never silently clamp it into an undocumented experiment.

After baseline data, candidate tuning ranges are telegraph 0.7–1.2 s, recovery 1.5–3.5 s, 8–18 letters per phrase, and typo rollback 0 or 1. These are future experiment ranges, not random values chosen per run. Change one major factor per follow-up comparison. Slow motion, book recovery, longer language, and additional skills remain future hypotheses.

## D12 — Edge-case contract

- Unsupported characters, paste/composition, and ignored punctuation produce no progress or typo penalty; no text is secretly queued until a later state. A short hint is rate-limited to once per 2 s of focused UI time.
- Rapid distinct presses, including doubled letters and several events within one render frame, are processed in order. Do not debounce by dropping identical adjacent characters or imposing a typing-speed cap.
- A held key across pause/start/retry must be released before it can generate a fresh gameplay action. No stuck movement after disconnect. Reconnect never swaps the prompt or resets health.
- Pause suppresses the tick's hit and typing together. Unpause has no damage/typing on the confirmation tick. Resume preserves the precise warning position and phase, followed by the full resume countdown.
- A new keyboard replaces the disconnected device only after the resume gate; clear old device subscriptions/queues. Dual-keyboard assistance is outside the baseline; flag device changes in the record.
- Escape is pause, never phrase cancel. There is no free cancel/re-roll command. A player may stop typing and keep dodging, or end the encounter through the pause menu.
- A malformed/empty prompt or missing required UI/input resource prevents encounter start with a visible setup error. No auto-win, fallback invisible text, or accepted empty phrase.
- Duplicate completion/attack callbacks are idempotent by IDs. Stale callbacks from a prior run cannot modify a retry. Scene unload unsubscribes input and clears audio/visual objects and queued events.
- With no focus, a disconnected keyboard, or a performance pause, gameplay does not simulate in the background. Logging failure is different: keep gameplay usable, mark the run `LoggingIncomplete`, and show that its data cannot qualify for analysis.
- A result snapshot cannot be changed by later collisions, text, timers, or held retry input. Explicit retry creates a fresh run; settings persist, gameplay state does not.

## D13 — Acceptance specification

These are requirements for the future implementation, **not claims of passing tests**. Logic checks use exact state assertions; input checks use the actual adapter; presentation checks require a visible player build. Automated arrangements and human sessions must be labeled separately.

| ID | Action / arrangement | Required observation |
| --- | --- | --- |
| AT01 | Fresh run, press 1, type complete P0 in a fixture | 11 letters accepted; exactly one cast; enemy 100→75; no Enter. |
| AT02 | Type mixed-case P0 with Shift/Caps | Same cursor and cast as lowercase. |
| AT03 | `bu`, then X | `burn` index 2→1; wrong count +1; HP unchanged. |
| AT04 | Complete `burn`; X at start of `the` | Previous word remains; current index stays 0. |
| AT05 | Add spaces/punctuation/digits/Backspace/Enter | No cursor, HP, or typo change; no premature cast. |
| AT06 | Hold the final required letter | One acceptance for one physical press; no repeated cast. |
| AT07 | Fixture `feel the heat`, press E twice with release | Both E presses count; holding one E does not. Fixture is not a study-bank replacement. |
| AT08 | Start and first letter in one update, correctly ordered | First letter is accepted exactly once. |
| AT09 | Several ordinary physical letters in one render frame | Ordered progress matches the same timestamped stream at higher FPS. |
| AT10 | Finish phrase plus trailing text/start events | One cast; trailing text discarded; cooldown start is not buffered. |
| AT11 | Hold 1 through cooldown | No new attempt until release and fresh press. |
| AT12 | Paste, IME commit, unsupported Unicode, Ctrl+letter | No progress; no typo damage/penalty; clear unsupported-input feedback. |
| AT13 | Type A/D/J/K/E/R in a prompt | Only text; no movement, melee, parry, interaction, or retry. |
| AT14 | Hold both arrows; release one | Zero velocity, then expected 5 units/s direction. |
| AT15 | Type while moving in RT | Both progress and movement occur; idle/movement preserves cursor. |
| AT16 | Move during PAUSED chanting | Position and enemy phase remain fixed; typing and encounter limit advance. |
| AT17 | Enter cooldown in either mode | Combat resumes; cooldown expires at 0.60 s; damage applied once. |
| AT18 | Telegraph then move away | Stripe center remains captured; escape before strike prevents damage. |
| AT19 | Positions just outside/on/inside overlap boundary | Outside misses; equality and inside hit, once per attack ID. |
| AT20 | Start warnings at both arena edges | Visible inward escape exists; no out-of-bounds player center. |
| AT21 | Hit during partially completed second word | HP 3→2; first word saved; second resets; typing locked 0.35 s; RT movement works. |
| AT22 | Repeated overlap within invulnerability | No extra HP loss/progress reset/lock refresh for 1 s of enemy time. |
| AT23 | Hit and final character in same tick | Hit wins; no cast; lethal hit ends run. |
| AT24 | Deadline and final character in same tick | `TimeLimit`; no cast after deadline. |
| AT25 | Pause and hit/text in same tick | Pause wins; no hit/progress; resume cannot replay queued letters. |
| AT26 | Focus loss while arrow/letter held | Pauses; return requires release, confirmation, 1 s countdown; no stuck input. |
| AT27 | Focus loss during a manual pause/countdown | Cannot override pause or shorten the resume countdown. |
| AT28 | Unplug/reconnect keyboard midword | Cursor retained; no simulation without device; fresh resume gate. |
| AT29 | Four spells / three damaging hits | Exactly one `Won` / `Lost`; future attacks/inputs inert. |
| AT30 | Retry after each outcome and after paused restart | All gameplay state/counters reset; same condition; new run ID. |
| AT31 | Inject duplicate/stale cast and hit events | No double damage and no changes to a newer run. |
| AT32 | Replay one timestamped scenario at 30/60/120 FPS | Identical logical outcomes/cursors/HP; feedback in next rendered frame after consumption. |
| AT33 | Introduce a >250 ms simulation gap | Visible performance pause; no unseen fast-forward; run quality flagged. |
| AT34 | Run each bank | Exact order/counts; prompt unchanged by errors/hits; retry starts at first entry. |
| AT35 | Missing/invalid prompt or invalid config | Visible setup error; encounter cannot start. |
| AT36 | UI at 1024×576, 1280×720, 1920×1080 × three text scales | Phrase, current letter, player, stripe edges and status readable and unclipped. |
| AT37 | Mute + reduced effects | Correct/error/hit/attack/ready/pause remain distinguishable visually. |
| AT38 | Fast typing and repeated scene load/unload | No growing callbacks/objects/audio voices; no missing/duplicate press events. |
| AT39 | Simulate log write failure and process interruption | Play continues with logging warning, or partial run classified Interrupted; never false success. |
| AT40 | Verify telemetry against a known short trace | Clocks, correct/wrong counts, completed attempts, damage and one result agree with model. |
| AT41 | Exercise all three practice steps, next/restart/leave/death | D03's active systems, immortal dummy, training hit handling and transitions hold; no practice state enters the measured run. |
| AT42 | Load saved snapshot; zero-time/no-cast/excluded records | Hash verifies against saved bytes; reconstructed config/corpus match; undefined metrics are N/A and invalid pairs are outside n. |

Time-based tests replay the same timestamped inputs, not one character per rendered frame. Human feedback cannot replace input-origin tests; a test that directly calls `AcceptLetter` does not establish that real keyboard routing works. Conversely, all AT checks passing only opens the human experience gate.

## D14 — Measurement and local record format

Store JSONL under ignored `artifacts/typing-combat/`. No network uploads, account identifiers, user names, clipboard text, or arbitrary raw keystrokes. Participant labels such as P01 are local study labels. Store prompt IDs and progress transitions instead of raw typed strings. A small local session manifest records condition order, self-described typing/English familiarity, accessibility settings, consent to local notes, and facilitator observations; it is not committed to Git.

Initial `schemaVersion` is integer 1. Before a run, save the resolved configuration snapshot as UTF-8 JSON without BOM: all D11 values, corpus version and complete prompt records, chosen condition/bank, and effective input/accessibility/display/audio settings. Compute `configHash` as lowercase SHA-256 of those exact saved bytes, excluding the hash itself. Store the snapshot once under its hash in the local output directory and reference that hash in the run. Reproduction reads those saved bytes, rather than reconstructing an unspecified serialization. A mode or setting change therefore changes the snapshot hash even when the underlying design version stays 0.1.

Every record has `schemaVersion`, `designVersion`, `configVersion`, `configHash`, `buildCommit`, `sessionId`, `participantLabel`, `runId`, `condition`, `bankId`, `practice`, `seq`, `tick`, `wallSeconds`, `encounterSeconds`, `enemySeconds`, `event`, and `payload`. Run-start includes resolution, display scale, input-layout declaration, and settings. Event types are `RunStarted`, `AttemptStarted`, `LetterAccepted`, `Typo`, `InputIgnored`, `WordCompleted`, `SpellCast`, `ThreatStarted`, `PlayerHit`, `PauseStarted`, `ResumeCompleted`, `PerformancePause`, `RunEnded`, and `LoggingError`. Relevant payloads contain attempt/cast/attack IDs, prompt ID, cursor before/after, HP before/after, and categorized reason. No raw incorrect letter is required.

Events have one writer and increasing sequence numbers. Flush at run start/end and pause; bound in-memory buffering. Finish writes one result atomically. A duplicate finish is ignored. Missing end, missing sequence segments, or logging failure marks data incomplete rather than inventing missing values. CSV summaries may be derived from JSONL; never use the summary to conceal incomplete source events.

Report definitions:

- **Input accuracy:** accepted correct letter events / (accepted correct + wrong supported-letter events). Re-typed correct letters count as events; ignored inputs do not. Zero denominator is `N/A`.
- **Productive letter throughput:** letters in completed phrases / encounter seconds; zero encounter seconds is `N/A`. This is throughput, not typing accuracy or standardized WPM.
- **Completion time:** encounter time at first terminal result; state the outcome. Also retain wall time and enemy time.
- **Exposure-adjusted hits:** damaging hits / enemy seconds × 60; report enemy seconds alongside it. Zero exposure is `N/A`. Do not compare raw hits per wall minute as if PAUSED supplied equal enemy exposure.
- **Attempt outcomes:** completed, open at loss/time limit, or aborted. A typo/hit does not start a new attempt. Track first-cast time (`N/A` when no cast occurred), total casts, typo count, hit count, ignored-input count, and word resets from hits separately.
- **Quality flags:** coaching during a measured attempt, device/layout changes, performance pause, altered config, incomplete logging, or interruption. Keep these runs visible in records but exclude them from the primary condition comparison and state why. Manual/focus pauses are reported; strategically using them invalidates timing comparison.

Software logs cannot tell whether a player looked at the keyboard or why they stopped typing. Record those as observations/questions, not inferred facts.

## D15 — Human playtest protocol

Initial cohort: **4–6 people** spanning comfortable touch typing and slower/look-down typing; include people who did not implement the prototype when available. This is a directional usability test, not a statistically powered study. Record familiarity rather than imposing an invented universal skill cutoff.

Use the same build, config, keyboard, display, and volume policy where practical. Explain the controls and word-checkpoint rules once using P0. Give approximately 60 s typing-only practice, 30 s dodging-only practice, and 60 s combined RT practice. At that point, before any remedial teaching, record whether the person can explain auto-cast, typo rollback, and hit word reset. That first assessment supplies D16's comprehension measure. If understanding is incomplete, teach again and record the extra practice; the first assessment remains a failure for that measure even if later explanation succeeds. The person may still perform uncoached measured encounters after learning; do not label an instruction failure as dislike of combat.

Then conduct one uncoached measured encounter in each condition, each ending on win/loss/120-second limit. Permit a break between conditions. Allocate conditions/banks in rotation:

| Participant position | First encounter | Second encounter |
| --- | --- | --- |
| 1 | RT / A | PAUSED / B |
| 2 | PAUSED / B | RT / A |
| 3 | RT / B | PAUSED / A |
| 4 | PAUSED / A | RT / B |

Repeat this rotation for additional participants. Do not always show the same words in the same mode or always test RT first. The banks match lengths but may still differ in familiarity, so report allocation and avoid claiming perfect experimental isolation. No mid-encounter tuning. Accessible display settings may differ per participant; hold them constant across that person's pair and record them. If a participant cannot use the baseline controls, allow practice/comparator exploration and record the access limitation separately rather than forcing a standard trial.

After each condition ask: (1) what caused the last mistake/hit and what would you do differently; (2) control comfort, perceived fairness, and desire to replay, each 1–5 with 1=very poor/none and 5=very good/strong; (3) one moment when they chose to type or dodge. After both, ask their preference and reason. The observer records missed telegraphs, confusion about the next letter, hand discomfort, purposeful pauses in typing, and voluntary replay requests. Avoid leading questions that suggest RT is the desired answer.

Do not substitute automated fixture runs for these encounters, or report these planned sessions as completed. A participant who stops because of discomfort may stop immediately; retain that observation as a usability finding, not a technical failure.

## D16 — Decision gates and interpretation

**Gate 1: implementation.** All applicable AT01–AT42 checks pass with evidence at their stated scope. A reproducible dropped/duplicated normal key press, false cast, unreadable required cue, stale-run mutation, or unavoidable baseline attack blocks the experience study until fixed. Missing visual/input evidence is incomplete, not pass.

**Gate 2: experience.** Before collecting data, use these initial team decision thresholds; they are provisional design criteria, not published scientific standards. `n` means participants with two usable measured condition records. Report excluded, interrupted, or discomfort-stopped participants and their observations separately; they never disappear from the study report merely because they are outside n. Require at least four usable paired participants before applying these proportions:

- At least `ceil(0.8 × n)` can explain auto-cast, typo rollback, and hit word reset at the first post-standard-practice assessment, before remedial coaching.
- At least `ceil(0.6 × n)` complete at least one spell in their RT encounter and can describe one deliberate type/dodge choice.
- Median RT comfort is at least 3/5, and at least `ceil(0.6 × n)` rate RT desire-to-replay at least 3/5. Report individual ratings as well as the median. Any recurring hand discomfort remains a design issue even if a majority passes.

With six valid pairs these proportions require five and four people respectively. Passing does not prove broad appeal. If input is correct but understanding fails, revise cues/teaching first. If people understand but cannot find typing windows, change one threat timing parameter or phrase length. If physical hand switching is the problem, design a separate remapping/slow-motion condition. If PAUSED is consistently preferred, preserve that finding; RT is a hypothesis, not a result to force. An inconclusive or too-small sample leads to another focused test rather than a success claim.

Only after the typing interaction survives these gates should a new experiment reconnect parry-earned energy and skill selection. That follow-up must independently decide charge consumption, cancel/refund, and how failed attempts interact with energy; current full-game intentions do not silently apply to this free-cast laboratory. Do not add a boss campaign to compensate for weak typing feel.

## D17 — Implementation boundaries and sequence

Current baseline inspected at commit `ae97f0e`: [ValidationGameState.cs](../Game/Assets/_Game/Validation/ValidationGameState.cs) uses two exact-case/punctuation sentences, a 12-second limit, energy payment at entry, editable text and Enter submission. [ValidationWorld.cs](../Game/Assets/_Game/Validation/ValidationWorld.cs) freezes movement/enemies during typing. Existing tests explicitly assert these behaviors. The [previous validation results](validation-results.md) apply to that earlier prototype, not this design.

Create a future scene `Game/Assets/_Game/Scenes/TypingCombatLab.unity` and a separate `TypingCombat` source/test area; keep the legacy scene runnable. Those paths are planned, not files delivered by this documentation change. Reuse the pinned Unity/URP/Input System setup, sprite fixture, build infrastructure, and testing techniques. Do not subclass the old state model and silently inherit its freeze/Enter semantics.

Minimum responsibilities:

| Component responsibility | Contract |
| --- | --- |
| Input adapter | Physical presses/text/control routing, timestamps, supported-layout checks, repeat filtering, device lifecycle. Emits events; does not change HP or cursor. |
| Typing model | Immutable prompt + word/letter indices; correct/error/checkpoint/completion decisions. No UI dependency. |
| Encounter coordinator | D06–D09 state, fixed ticks, event precedence, threat, position, HP, and exactly-once effects. |
| Presenter | Two text displays, warnings, HP, cues, menus and accessibility. Reads snapshots; does not decide correctness. |
| Recorder | Ordered local events, quality flags, run summary. Failure cannot alter combat. |

Implementation order and exit evidence:

1. **Input spike:** visible typing-only fixture; prove held keys, repeated letters, fast batches, case, shortcuts, and unsupported input handling using real keyboard plus adapter tests. If input origin/repeat correlation is unreliable, resolve it here; do not paper over it with a typing-speed cap.
2. **Typing loop:** implement checkpoints, one-letter rollback, automatic one-shot Pulse and cooldown; pass relevant state examples before adding threat.
3. **Threat loop:** implement horizontal motion, telegraphed stripe, HP/reaction, deterministic ordering, pause/retry, and RT/PAUSED policies.
4. **Presentation and evidence:** add the complete cues/accessibility, local records, standalone acceptance, and verified representative screenshots.
5. **Human sessions:** execute D15, write observed results separately from this normative design, then apply D16 and version the next revision.

For AI implementation: implement only this experiment; treat MUST-equivalent statements and the config as requirements; record deviations before making them; do not claim unrun acceptance or human tests; do not introduce paid services or external assets. A technical obstacle permits a reported blocked requirement, not a silent change of interaction. Keep new tests separate and retain existing validation tests. Gameplay work follows the repository PR/check/merge policy; this document change itself does not implement or authorize claims about the new gameplay.

Suggested coordination, consistent with current roles: Vincent organizes observations and UI clarity; Ace contributes meaningful prompt wording and audio feedback; Andrew integrates input/combat; Michelle handles readable visual cues. Programming remains shared. These are work areas, not permission gates or assignments sent to anyone.

## D18 — Version parity, review, and open evidence

The English implementation edition and Chinese reading edition are two presentations of one design. Neither may silently override the other. On contradiction, stop the dependent implementation decision, reconcile both, and increment the design/config version as appropriate. The Chinese edition must retain every gameplay consequence, default value, exception, test category, and decision gate; it may explain architecture without code-like prose.

| IDs in both editions | Required shared content |
| --- | --- |
| D01–D03 | Goal, evidence limits, scope, modes, complete loop |
| D04–D07 | Controls, parser, state/clocks, event precedence |
| D08–D12 | Geometry, damage/reward, UI/audio, exact config, edge cases |
| D13–D16 | Acceptance, measurement definitions, human protocol, interpretation |
| D17–D18 | Existing-code gap, implementation sequence, parity and status |

Review checklist: all 18 IDs exist in both; phrase banks/counts match; defaults and time units match; examples follow the parser; hit/final-letter and deadline ties agree; PAUSED exposure is not mistaken for equal enemy time; existing test success is not relabeled as new evidence; source-confirmed facts are distinct from proposed rules; no placeholder decision is presented as known Textorcist behavior.

Open evidence, not unspecified implementation rules: suitability of typo rollback; phrase familiarity; comfortable warning/recovery timing; readable local word placement; keyboard/layout behavior on teammates' machines; accessible control needs; preference between RT and PAUSED; interaction with future parry energy. All have a defined first experiment or follow-up above. Their answers must come from implementation evidence and playtesting, not from completing this document.

Change log: **0.1 — first researched specification and matched reading edition.** No new gameplay, acceptance results, or player study results are claimed.
