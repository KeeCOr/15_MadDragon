# Mobile Battle HUD Quickbar Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the text-only mobile battle HUD with a tested touch-first quickbar that matches the docs gameplay preview direction.

**Architecture:** Keep the UI ownership inside `MobileBattleHud`, expose small callbacks for command buttons, and let `TestBootstrap` wire those callbacks to existing battle command state. The HUD remains a UGUI-only runtime helper so EditMode tests can construct it without loading scenes.

**Tech Stack:** Unity 2022.3.62f3, C#, UGUI, NUnit EditMode tests.

## Global Constraints

- Target project only: `C:/Development/15_MD`.
- Do not modify other projects.
- Check docs gameplay preview before visual/UX changes.
- UI choices stay within five primary buttons and 44px+ touch targets.
- Finish with tests, Windows build, portable executable placement, planning MD+HTML update, and Google Drive sync.

---

### Task 1: MobileBattleHud Test Contract

**Files:**
- Create: `Assets/_Game/Tests/EditMode/MobileBattleHudTests.cs`
- Modify: `Assets/_Game/Scripts/UI/MobileBattleHud.cs`

**Interfaces:**
- Produces: `MobileBattleHud.CommandKind` enum with `Rally`, `Attack`, `Hold`, and `Spells`.
- Produces: `MobileBattleHud.SetCommandHandler(System.Action<MobileBattleHud.CommandKind>)`.
- Produces: child buttons named `Rally`, `Attack`, `Hold`, and `Spells`.

- [ ] **Step 1: Write failing tests**

Create tests that construct `MobileBattleHud`, assert the four named command buttons exist, assert every button height is at least 44px, assert clicking each button reports the expected command, and assert `Refresh` writes compact timer, HP, and reward text.

- [ ] **Step 2: Run tests to verify RED**

Run Unity EditMode tests and expect `MobileBattleHudTests` to fail because the buttons and command API do not exist yet.

- [ ] **Step 3: Implement minimal HUD API and layout**

Change `MobileBattleHud` to build a top status panel plus four bottom command buttons. The button callbacks call the registered command handler and update a small quick status label.

- [ ] **Step 4: Run tests to verify GREEN**

Run the same EditMode test command and expect `MobileBattleHudTests` to pass.

### Task 2: Battle Wiring

**Files:**
- Modify: `Assets/_Game/Scripts/Testing/TestBootstrap.cs`
- Test: `Assets/_Game/Tests/EditMode/MobileBattleHudTests.cs`

**Interfaces:**
- Consumes: `MobileBattleHud.SetCommandHandler`.
- Consumes: existing `TestBootstrap` command state for selected units and spell panel status.

- [ ] **Step 1: Wire command callbacks**

When creating the mobile battle HUD, register a command handler. `Rally`, `Attack`, and `Hold` update the existing info/status text and clear pending spell selection. `Spells` toggles/selects spell mode feedback without changing battle outcome logic.

- [ ] **Step 2: Verify UI command behavior remains covered**

Run the focused EditMode test command and then the full EditMode suite.

### Task 3: Docs, Build, and Placement

**Files:**
- Modify: `docs/MadDragon_기획서.md`
- Modify: `docs/MadDragon_기획서.html`

**Interfaces:**
- Produces: updated v0.6 draft notes describing the mobile battle quickbar.
- Produces: refreshed `MadDragon_v0.6_portable.exe` at project root and Google Drive executable folder.

- [ ] **Step 1: Update planning docs**

Update both MD and HTML with the new HUD implementation status and version/date.

- [ ] **Step 2: Run full EditMode tests**

Run the Unity EditMode suite.

- [ ] **Step 3: Build Windows player**

Run `MedievalRTS.EditorTools.MadDragonCliBuild.BuildWindows` from Unity batchmode.

- [ ] **Step 4: Place portable executable and sync**

Keep one latest executable in `release/`, place `C:/Development/15_MD/MadDragon_v0.6_portable.exe`, copy it to `G:/내 드라이브/실행파일/15_MadDragon_v0.6_portable.exe`, and sync planning docs to `G:/내 드라이브/기획서/MadDragon/`.

## Self-Review

- Spec coverage: P1 HUD usability, docs preview alignment, tests, build, portable placement, and Google Drive sync are covered.
- Placeholder scan: no TBD/TODO placeholders.
- Type consistency: `CommandKind` and `SetCommandHandler` are used consistently across tasks.
