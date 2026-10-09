# Unity setup

**Editor:** Unity **6000.6.5f1** (`3ff58d469c8a`). **Project:** `Game/`. **Scene:** `Assets/_Game/Scenes/Validation.unity`.

The validation scene is a greybox compatibility prototype with keyboard combat, typing, a rune puzzle, and a boss. It is not the finished course demo.

## Pinned packages

| Package | Version |
| --- | --- |
| Universal Render Pipeline | 17.6.0 |
| Input System | 1.20.1 |
| uGUI | 2.6.0 |
| Cinemachine | 6.6.0 |
| Test Framework | 1.8.0 |
| 2D Sprite / 2D Tilemap | 1.0.0 each |
| Unity Pipeline | 0.8.0-exp.1 |

The full dependency list and lockfile are in `Game/Packages/`. Project settings use the 2D renderer, Visible Meta Files, and Force Text serialization.

## Build and validation

**Build menu:** Keyboard Warrior → Build Windows validation.

**Output:** `Builds/Windows/Validation/KeyboardWarrior.exe`; the adjacent data files belong with the executable.

With the Editor closed, from the repository root:

```powershell
pwsh -File tools/validate-unity.ps1
```

The script runs EditMode and PlayMode tests, a Windows build, and standalone checks at requested 30/60/120 FPS limits. Game windows open briefly for real screenshot checks. `-Mode EditMode`, `PlayMode`, `Build`, or `Smoke` selects one stage. `-EditorPath` supports a custom installation. Reports are written to `artifacts/validation/`.

## Optional AI authoring

Unity Pipeline provides local Editor commands for Unity CLI. The in-editor Assistant subscription has not been activated. AI tools are optional development tools; gameplay does not require them.
