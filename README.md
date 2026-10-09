# Keyboard Warrior

A keyboard-only 2D action game: fight, parry, then type a sentence to cast a Smite.

**Our goal:** one short level and the first boss, playable on Windows. The Unity project has not been created yet.

## Who does what?

Suggested starting roles; we can adjust them together.

| Person | Main focus |
| --- | --- |
| Andrew | Player controls, combat, and putting the game together |
| Michelle | Art, animation, and visual effects |
| Ace | Music, sound effects, and audio |
| Vincent | Tasks, menus, typing UI, and playtest coordination |

Everyone can help with programming.

## Get started

1. Send Andrew your GitHub username and accept the repository invitation.
2. Install Unity Hub, GitHub Desktop, and a C# editor.
3. Follow the short [sharing your work guide](CONTRIBUTING.md) to download the project.

We plan to use **Unity 6.6 + Universal 2D**. Once Andrew creates the project, everyone installs the **same full Unity version** and opens the `Game/` folder.

## How we work

**Pick a task → make a branch → make and test your changes → ask a teammate to check → merge.**

A branch keeps your unfinished work separate. A pull request (PR) asks the team to add it to `main`, our shared version.

- Choose a small [task](https://github.com/Andrewyuan34/keyboard-warrior/issues) and say you are working on it.
- Tell the team before editing a shared scene. One person edits that scene at a time.
- Show progress once a week. Raise problems early.
- AI tools are optional. Check and test any code you use.

## Build it in this order

1. Set up Unity and check that another teammate can run it.
2. Make movement, attacks, parries, and typing Smite work with simple shapes.
3. Add the level, boss, art, sound, and menus.
4. Playtest, fix problems, and share a Windows build.

Before a release, someone else should download a fresh copy, build it, and play from start to boss, including death and retry. Put the finished ZIP in **GitHub Releases**.

## Three things to remember

- Move assets inside Unity and include their `.meta` files when uploading.
- Do not upload cache folders, passwords, or personal information. For outside art/music, check that we can share it and add its [source](docs/asset-register.md).
- A green GitHub check does not mean the game works. Play the changed part before asking for review.

**Creating the project?** The [Unity setup notes](docs/unity-setup.md) are for that person; everyone else can start with the guide above.
