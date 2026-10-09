# Sharing your work

Use GitHub Desktop if you are new to Git. You do not need terminal commands.

## First time

1. Accept Andrew's repository invitation.
2. Sign in to GitHub Desktop. Choose **File → Clone repository → URL** and enter:
   `https://github.com/Andrewyuan34/keyboard-warrior`
3. Clone it to your computer. Follow any Git LFS setup prompt; LFS handles our image and audio files.
4. Once the Unity project exists, install the team's exact Unity version and open `Game/` in Unity Hub.

## Each task

1. **Get the latest work.** Commit any unfinished work on its own branch first. Close Unity, select `main` in Desktop, and click **Fetch origin**, then **Pull origin** if offered.
2. **Make a branch.** Choose **Current branch → New branch**. Give it a short name such as `add-parry`. Use a new branch for each task.
3. **Make your changes.** Open Unity and test what you changed. Tell the team before editing a shared scene.
4. **Save and upload.** Save your Unity scenes and project changes. In Desktop, check the changed files, include related `.meta` files, write a short summary, and click **Commit to…**, then **Publish branch** or **Push origin**.
5. **Open a PR.** Use **Preview/Create Pull Request** to open a PR into `main`. Say what changed and how you tested it. Ask a teammate for help if needed.
6. **Merge.** Once the checks pass, click **Squash and merge** on GitHub. Another person's approval is not required. Close Unity, return to `main`, and fetch/pull before starting the next task.

**Commit** saves a checkpoint on your computer. **Push** uploads it. **Merge** adds the PR's changes to the team's shared version. Everyone, including the owner, uses a PR instead of pushing directly to `main`.

## If something goes wrong

- **You cannot push:** check that you accepted the invitation.
- **You see a conflict:** stop and ask Andrew or the other author to help. Do not overwrite their work.
- **Images or audio are missing:** check Git LFS with Andrew.
- **A GitHub check fails:** open its details and fix the reported file, or ask for help.

The repository already ignores Unity caches and uses LFS for common art/audio formats. Keep new file extensions lowercase, and ask Andrew before adding a new large file type.
