# Contributing

## Changes

Each task uses a separate branch and a pull request (PR) into `main`. Every PR includes a short summary and test results. Passing repository checks are required before squash merging; another person's approval is optional. This applies to all contributors, including the repository owner.

Changed gameplay must be tested in Unity. Repository checks do not test the game itself.

## Unity and assets

- All contributors use the exact Unity version recorded in `Game/ProjectSettings/ProjectVersion.txt`.
- Shared scenes have one editor at a time to avoid conflicts.
- Assets are moved or renamed inside Unity and committed with their `.meta` files.
- Common art and audio files use Git LFS; file extensions stay lowercase.
- Unity caches, build output, credentials, and personal information are excluded from commits.
- Third-party assets require permission for use and redistribution, with credits in the [asset register](docs/asset-register.md).
