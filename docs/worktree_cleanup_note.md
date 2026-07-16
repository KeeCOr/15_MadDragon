# Worktree Cleanup Note

Date: 2026-06-24

Target worktree: `C:\Development\15_MD\.worktrees\maddragon-mobile-loop`

Main project HEAD: `aa936e8` (`master`)
Worktree HEAD: `6111a79` (`codex/maddragon-mobile-loop`)

## Decision

Do not delete this worktree.

The worktree is still registered with git and is dirty. Its uncommitted dirty state is limited to two deleted planning documents:

- `docs/MadDragon_기획서.html`
- `docs/MadDragon_기획서.md`

Those deletions look like stale document drift rather than active source edits, because the main project currently has those planning files present under `C:\Development\15_MD\docs`.

However, the worktree branch itself contains real project changes relative to `master`, including Unity source, UI, economy/resource systems, save-data updates, and edit-mode tests. Notable branch deltas include:

- Added `Assets/_Game/Scripts/Economy/*`
- Added mobile-loop UI screens such as `AttackPrepScreen`, `BaseManagementScreen`, `CampaignHubScreen`, and `MobileBattleHud`
- Added `Assets/_Game/Scripts/Visuals/MobileVisualStyle.cs`
- Modified progression save files and test bootstrap code
- Added edit-mode tests for campaign hub, raid loss calculation, and resource storage

Because the worktree is dirty and its branch contains real source/test changes, it is not unquestionably clean. It should be left untouched until a later pass either merges, archives, or explicitly discards the branch after review.

## Verification Performed

- `git -C C:\Development\15_MD worktree list --porcelain`
- `git -C C:\Development\15_MD\.worktrees\maddragon-mobile-loop status --short --untracked-files=all`
- `git -C C:\Development\15_MD\.worktrees\maddragon-mobile-loop diff --name-status`
- `git -C C:\Development\15_MD\.worktrees\maddragon-mobile-loop diff --name-status master...HEAD`

## Follow-Up

Before cleanup, review the branch changes as a mobile-loop implementation candidate. If the branch is no longer needed, first restore or intentionally discard the two deleted docs, confirm `git status --short` is empty inside the worktree, and only then remove the registered worktree with git.
