# Local validation

Validated on 8 October 2026 with Unity **6000.6.5f1**, Windows 11, and an RTX 4060 Laptop GPU (Direct3D 12).

| Check | Result |
| --- | --- |
| EditMode rules | 33/33 passed |
| PlayMode input and physics | 16/16 passed |
| Windows development build | Passed |
| Standalone integration | 16/16 checks at each requested 30/60/120 FPS limit |
| Observed average FPS | 29.3 / 57.6 / 116.1 respectively |
| Actual typing and victory screenshots | Visually checked |
| C# project build in .NET | Passed, no warnings or errors |
| Unity CLI + Pipeline | Created, moved, and removed a temporary Editor object |

Coverage includes movement, jumping, timed parry, melee, two Smites, typing corrections and timeout, input isolation, pause/focus handling, healing, puzzle gates, boss damage, victory, and retry. Audio checks confirm nonzero generated DSP output; listening quality needs human review.

These tests use controlled arrangements such as teleporting to fixtures. They validate the toolchain and implemented mechanics, not a complete human playthrough, final game balance, or another teammate's computer. The prototype still needs final art, narrative, and level design.

Run `pwsh -File tools/validate-unity.ps1` to repeat the game checks. Local reports and screenshots stay in ignored `artifacts/validation/`.
