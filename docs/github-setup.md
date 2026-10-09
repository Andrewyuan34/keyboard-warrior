# GitHub administration

The repository is owned by `Andrewyuan34`. It is public so teammates and reviewers can inspect the work. Public visibility does not grant write access.

## Collaborator onboarding

The other team members' GitHub usernames have not been supplied. No invitations are assumed to have been sent.

1. Collect each teammate's exact GitHub username.
2. The owner opens repository **Settings > Collaborators > Add people** and invites that account.
3. Each person accepts the invitation, clones the repository, and follows `CONTRIBUTING.md`.
4. Each person submits a small onboarding PR; another teammate reviews it. Use a useful correction or record an actual setup result rather than adding a meaningless test file.

Personal repositories have an owner and collaborators. If multiple people need repository administration later, consider transferring to a team-owned organization after the demo. This initial project does not require an organization.

## Intended main-branch controls

- Default branch: `main`.
- Pull requests: require one approving review, dismiss stale approvals when changes are pushed, and resolve conversations before merging.
- Required status check: `Repository checks` after it has been observed running successfully.
- No force pushes or deletion of `main`.
- Squash merging enabled; merge commits and rebase merging disabled; delete merged source branches automatically.
- Owner administrative bypass retained for recovery while collaborators are being onboarded. It is not the team's normal merge path. Enforce rules for administrators too after the team has a working second reviewer if desired.

Check the repository's live **Settings > Branches** and **Actions** pages for the applied state. These written rules describe the intended controls; the settings themselves determine enforcement. A protected branch cannot replace peer review or a playtest.

## Issues and milestones

Use Issues as the task list. Each active task has one owner, a reviewer, acceptance criteria, dependencies, and a milestone. Prefer a single active implementation task per person.

Milestones:

1. **M0 - Team onboarding and Unity bootstrap**: every member can clone and the initial game project opens/builds.
2. **M1 - Combat and typing loop**: attack, parry, charge, typing, success, and failure work together.
3. **M2 - Complete demo level**: tutorial, encounter, first Boss, victory, and death/retry flow.
4. **M3 - Presentation and release**: integrated art/audio, external playtest, clean-clone build, release.

Use `status:ready`, `status:in-progress`, `status:review`, and `status:blocked` labels on open Issues; close completed Issues. A blocked Issue names what or whom it needs. Milestone pages and filtered Issues are sufficient for this team; a Projects board can be added later without changing the workflow.

No calendar due dates are set until the team confirms its deadline and weekly availability. Move work between milestones only after discussing the scope change.

## Automation and billing

The initial workflow runs read-only repository checks on pull requests and pushes to `main`. It uses no Unity license, AI token, or repository secret. It does not run the Unity Editor or build the game.

After bootstrap, decide whether Unity build/test automation is worth setting up. Until it is actually configured and verified, a teammate must produce and test the standalone milestone build. Do not label repository checks as game tests.

Keep build outputs in GitHub Releases or the course submission system. Do not commit executable builds to the source branch. Git LFS storage and download bandwidth are separate from ordinary Git storage; the owner reviews the account usage before uploading large asset collections. No paid service, spending limit, AI subscription, or license purchase is enabled by this plan.

## Public content and ownership

Publish project-specific instructions and code. Keep the original course document and any student IDs or private account details outside the public repository. Use the asset register to track the source and sharing terms of every imported art/audio/font asset.

The team has not selected a project-wide license. Agree on code, original art/music, and third-party asset treatment before adding one. Do not apply a permissive license to someone else's assets by default.

## References

- [Personal repository permissions](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/repository-access-and-collaboration/permission-levels-for-a-personal-account-repository)
- [Inviting collaborators](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/repository-access-and-collaboration/inviting-collaborators-to-a-personal-repository)
- [Protected branches](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [Git LFS billing](https://docs.github.com/en/billing/concepts/product-billing/git-lfs)
