# Contributing

This guide is for the four-person team. Use a branch in the shared repository after accepting a collaborator invitation. Public visitors can read and fork the repository, but cannot push to it without write access.

## 1. One-time setup

1. Create your own GitHub account and enable its normal account security protections. Never share an account or token.
2. Send your GitHub username to the repository owner and accept their invitation.
3. Install Git, Git LFS, Unity Hub, and a C# editor with Unity debugging support.
4. Set your Git author name and email. A GitHub-provided private/noreply email is suitable for this public repository.
5. Clone the repository and download LFS objects:

```sh
git lfs install
git clone https://github.com/Andrewyuan34/keyboard-warrior.git
cd keyboard-warrior
git lfs pull
```

In the cloned repository, configure your author identity. Replace both example values; copy your noreply address from **GitHub Settings > Emails**:

```sh
git config user.name "Your Name"
git config user.email "YOUR_GITHUB_NOREPLY_EMAIL"
```

These settings apply to this repository. For HTTPS authentication, sign in through Git Credential Manager's browser prompt or GitHub Desktop when requested. Do not enter your GitHub account password as a Git push password.

After the Unity bootstrap is merged, install the exact version from `Game/ProjectSettings/ProjectVersion.txt`, add `Game/` in Unity Hub, and open it. Do not let a different editor version upgrade the project as part of an unrelated task.

## 2. Start a small task

Use an Issue that states the intended behavior, owner, affected files/scenes, dependencies, and acceptance criteria. Prefer work that can reach review within one working day. Agree on ownership before changing a shared scene or binary source asset.

Start from a clean working tree. Close Unity before changing branches or pulling changes that modify scenes, packages, or project settings.

```sh
git status
git switch main
git pull --ff-only
git lfs pull
git switch -c feat/typing-qte
```

Name branches by purpose: `feat/typing-qte`, `fix/parry-double-charge`, `art/player-animation`, `audio/parry-feedback`, or `chore/unity-bootstrap`. Create a new branch for each new task; do not keep a permanent branch per person.

## 3. Commit and push

Keep commits focused. Check what is being added before staging it. Unity resource changes include their `.meta` files; move or rename resources inside Unity whenever possible.

```sh
git status --short
git diff
```

Stage the relevant files in your Git client, or use `git add` with their actual paths. Then review the staged changes:

```sh
git diff --cached --stat
git diff --cached
git diff --cached --check
git commit -m "feat: add typing QTE success and timeout states"
git push -u origin HEAD
```

A **commit** saves a local checkpoint. A **push** uploads commits on your branch. Neither merges your work into `main`; the pull request does that after review.

When adding binary assets, confirm that the intended files appear in `git lfs ls-files`. The committed `.gitattributes` already tracks common image, art-source, model, and audio formats. Use lowercase file extensions for new assets. If a new binary format needs LFS, add the rule before its first commit.

## 4. Open a pull request

On GitHub, open **Compare & pull request**, with base `main` and your branch as the source. Fill out the PR template and link the Issue with `Closes #NUMBER`. If you use GitHub CLI, `gh pr create --base main` opens the same workflow.

Include:

- What the player or teammate can now do, and the linked acceptance criteria.
- What you actually tested, which scene/build and editor version you used, and any remaining limitations.
- A screenshot or short clip for visual, animation, camera, or UI changes.
- Asset sources and any package, editor, or project-setting changes.

Ask another teammate to review. Reviewers should inspect the diff and run the affected behavior for gameplay changes; an approval is not just an acknowledgement. The author fixes review feedback on the same branch and pushes again.

Merge using **Squash and merge** after one peer approval, required checks, and resolved review conversations. The owner should use administrator bypass only for documented repository recovery, not routine feature delivery.

## 5. Integrate changes from main

Commit your own work first and close Unity. Update the feature branch without rewriting shared history:

```sh
git fetch origin
git merge origin/main
git lfs pull
```

If the merge reports conflicts, resolve them before continuing. Do not run the final commands below until the conflicts are actually resolved:

```sh
git add PATHS_YOU_RESOLVED
git commit
git push
```

`PATHS_YOU_RESOLVED` is a placeholder, not a literal filename. If you cannot safely resolve a merge, `git merge --abort` returns to the pre-merge state; coordinate with the other author.

- For code, combine the intended behaviors and rerun relevant tests.
- For scenes and Prefabs, involve the current owner. Open the result in Unity and check references, objects, and behavior. Never blindly choose one entire side of a shared scene conflict.
- For art/audio binary files, decide which revision to keep with its author. These generally cannot be merged line by line.
- UnityYAMLMerge can assist scene merges later, but each computer needs its own valid editor-tool path. It is not preconfigured by this starter.

After your PR is merged, close Unity, switch to `main`, pull, and download LFS objects. Start the next task on a new branch. Remove old local branches only after confirming the PR was merged and there is no extra unpushed work.

## GitHub Desktop alternative

You can use GitHub Desktop instead of typing Git commands: clone the repository, fetch/pull `main`, create a branch, inspect the Changes panel, commit, push the branch, and create a PR. Follow the same scene ownership and review rules. Ensure Git LFS is installed and resources are real files rather than small LFS pointer text files.

## Files that belong in Git

- `Game/Assets/` with all resource and folder `.meta` files.
- `Game/Packages/`, including `manifest.json` and `packages-lock.json`.
- `Game/ProjectSettings/`, documentation, repository configuration, and necessary source assets.
- `.gitattributes` and `.gitignore` so everyone shares the same rules.

Do not commit Unity's `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, IDE caches, build output, local credentials, or machine-specific configuration. The `.gitignore` is scoped for a Unity project in `Game/`.

## AI-assisted work

Give AI one Issue and a bounded file/task scope. Use your own feature branch and checkout. Review all generated changes, include required `.meta` files, and test the result before requesting review. AI-generated output does not count as a review approval or a passed playtest.

Do not let multiple AI agents edit the same scene or working directory concurrently. Keep provider credentials out of tracked files. Record generated assets in the asset register. Package additions and editor upgrades need a dedicated setup PR.

## Getting help

Report the Issue/PR, exact editor version, error text, and steps to reproduce. Describe whether the failure occurs in the Editor, a standalone build, or only on one computer. Keep decisions that affect the project in the relevant Issue or PR so teammates can find them later.

References: [GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow), [Git LFS](https://git-lfs.com/), [Unity Smart Merge](https://docs.unity3d.com/6000.3/Documentation/Manual/SmartMerge.html).
