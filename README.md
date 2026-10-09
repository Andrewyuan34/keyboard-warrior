# Keyboard Warrior

A keyboard-only 2D action game: parry to gain energy, choose a Smite with number keys, then type a sentence to cast it.

**Demo scope:** one short level and the first boss, playable on Windows.

## Team

| Member | Responsibilities |
| --- | --- |
| Vincent | Tasks, menus, typing UI, and playtest coordination |
| Ace | Narrative design, music, sound effects, and audio |
| Andrew | Player controls, combat, and game integration |
| Michelle | Art, animation, and visual effects |

Programming is shared across the team.

## Development

- **Engine:** Unity **6000.6.5f1**, URP 2D.
- **Version control:** Git and GitHub, with Git LFS for common art and audio files.
- **Status:** playable greybox for checking engine compatibility and core mechanics. Final art, narrative, and the finished demo are still in development.

**Run:** open `Game/` in the pinned Unity version, open `Assets/_Game/Scenes/Validation.unity`, and press Play.

**Controls:** A/D or arrows move; Space jumps; J attacks; K parries; 1/2 select Smite; Enter submits text; Backspace corrects; E activates runes; Escape pauses; R retries after defeat or victory.

**Build:** Unity menu **Keyboard Warrior → Build Windows validation**.

**Tests:** `pwsh -File tools/validate-unity.ps1` from the repository root with the Editor closed. See [setup notes](docs/unity-setup.md) for options.

Tasks are tracked in [GitHub Issues](https://github.com/Andrewyuan34/keyboard-warrior/issues). Playable builds will be published in [Releases](https://github.com/Andrewyuan34/keyboard-warrior/releases).

## Project references

- [Contribution rules](CONTRIBUTING.md)
- [Unity setup notes](docs/unity-setup.md)
- [Asset credits and permissions](docs/asset-register.md)
