# Creating the Unity project

**For the person setting up the project.** Everyone else can follow the [main page](../README.md).

The project is not created yet. Start with a formal **Unity 6.6** release and the **Universal 2D** template.

## First setup

1. Create the Unity project inside this repository as `Game/`.
2. Confirm **Visible Meta Files** and **Force Text** in Project Settings.
3. Use Input System for controls, Cinemachine for the camera, and uGUI/TextMeshPro for menus and text. Keep package versions compatible with this editor.
4. Make a simple test scene and build it for Windows x64.
5. Commit `Game/Assets/` with its `.meta` files, `Game/Packages/`, and `Game/ProjectSettings/`. Caches and builds are ignored.
6. Add the exact Unity version, starting scene, controls, and build steps to the README. Ask another teammate to download a fresh copy and run/build it.

Unity records the editor version in `Game/ProjectSettings/ProjectVersion.txt`. Keep both `manifest.json` and `packages-lock.json` in `Game/Packages/`. Coordinate upgrades with the team.

## Keep the game simple

- Put our files in `Game/Assets/_Game/`: Scenes, Scripts, Prefabs, Art, Audio, UI, and Data.
- Agree on character sizes, art style, and sound levels before making lots of assets.
- Test individual features in small scenes; one person puts them into the main level.
- While typing a Smite, stop combat controls. Start by freezing the fight while the typing timer runs; pause the timer too when the game is paused or loses focus.
- Test wrong input, timeout, death, and retry. Check that a parry gives energy only once per attack.

## Before sharing a build

Use `Builds/Windows/<version>/` for output. Ask another person to test the complete game, including typing and retry, at different frame rates.

ZIP the whole build folder, including the files beside the `.exe`. Add it to GitHub Releases with controls, known problems, and a version tag pointing to the tested code.

## Repository owner

Invite teammates through **Settings → Collaborators → Add people**. Everyone, including the owner, must use a PR with passing checks. No other person's approval is required. Keep `main` protected.
