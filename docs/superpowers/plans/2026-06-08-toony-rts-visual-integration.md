# Toony RTS Visual Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply Toony RTS assets across the current MadDragon battle screen without breaking gameplay logic.

**Architecture:** Keep existing gameplay objects as authoritative roots and attach Toony RTS prefabs/models as sanitized visual children. Centralize asset path selection in a visual library and keep `TestBootstrap` changes limited to creation-time calls.

**Tech Stack:** Unity 2022.3, C#, UGUI, existing `MedievalRTS.Runtime` assembly, EditMode NUnit tests, ToonyTinyPeople TT_RTS assets.

---

### Task 1: Asset Availability Tests

**Files:**
- Create: `Assets/_Game/Tests/EditMode/ToonyRtsVisualLibraryTests.cs`

- [ ] **Step 1: Write failing tests**

Create tests that expect key Toony RTS assets to be discoverable:

```csharp
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ToonyRtsVisualLibraryTests
{
    [Test]
    public void ToonyRts_KeyPrefabsAndModels_ArePresent()
    {
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/prefabs/TT_Swordman.prefab"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/prefabs/TT_Archer.prefab"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/prefabs/TT_Mage.prefab"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/prefabs/machines/TT_Catapult_lvl1.prefab"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/models/buildings/TownHall.FBX"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/prefabs/banners/TT_Banner_Blue_A.prefab"));
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/FX/FX_prefabs/FX_Building_Destroyed_mid.prefab"));
    }
}
```

- [ ] **Step 2: Run test and confirm it passes or identifies missing paths**

Run:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -projectPath 'C:/Development/15_MD' -runTests -testPlatform EditMode -testFilter ToonyRtsVisualLibraryTests -testResults 'C:/Development/15_MD/Logs/EditModeResults_toony_assets.xml' -logFile 'C:/Development/15_MD/Logs/EditModeTests_toony_assets.log'
```

Expected after assets are imported: `result="Passed"`.

### Task 2: Visual Library

**Files:**
- Create: `Assets/_Game/Scripts/Visuals/ToonyRtsVisualLibrary.cs`
- Test: `Assets/_Game/Tests/EditMode/ToonyRtsVisualLibraryTests.cs`

- [ ] **Step 1: Add library API**

Create a library with stable string keys and fallback-safe loading:

```csharp
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MedievalRTS.Visuals
{
    public static class ToonyRtsVisualLibrary
    {
        public const string Root = "Assets/ToonyTinyPeople/TT_RTS/TT_RTS_Standard/";

        public static GameObject LoadUnit(string assetName)
        {
            return assetName switch
            {
                "Knight" => Load("prefabs/TT_Swordman.prefab"),
                "Archer" => Load("prefabs/TT_Archer.prefab"),
                "Mage" => Load("prefabs/TT_Mage.prefab"),
                "Scout" => Load("prefabs/TT_Scout.prefab"),
                "Cavalry" => Load("prefabs/TT_Light_Cavalry.prefab"),
                "Catapult" => Load("prefabs/machines/TT_Catapult_lvl1.prefab"),
                _ => null
            };
        }

        public static GameObject LoadBuilding(string key)
        {
            return key switch
            {
                "player_castle" => Load("models/buildings/TownHall.FBX"),
                "enemy_castle" => Load("models/buildings/Keep.FBX"),
                "wall" => Load("models/buildings/Wall_A_wall.FBX"),
                "tower" => Load("models/buildings/Tower_A.FBX"),
                "barracks" => Load("models/buildings/Stables.FBX"),
                "mage_tower" => Load("models/buildings/MageTower.FBX"),
                "elixir_well" => Load("models/buildings/Temple.FBX"),
                "gold_mine" => Load("models/buildings/Granary.FBX"),
                _ => null
            };
        }

        public static GameObject LoadDecoration(string key)
        {
            return key switch
            {
                "blue_banner" => Load("prefabs/banners/TT_Banner_Blue_A.prefab"),
                "red_banner" => Load("prefabs/banners/TT_Banner_Red.prefab"),
                _ => null
            };
        }

        public static GameObject LoadFx(string key)
        {
            return key switch
            {
                "building_destroyed" => Load("FX/FX_prefabs/FX_Building_Destroyed_mid.prefab"),
                "building_burning" => Load("FX/FX_prefabs/FX_Building_burning.prefab"),
                "machine_destroyed" => Load("FX/FX_prefabs/FX_machine_destroyed.prefab"),
                _ => null
            };
        }

        private static GameObject Load(string relativePath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(Root + relativePath);
#else
            return null;
#endif
        }
    }
}
```

- [ ] **Step 2: Extend tests to cover library methods**

Add assertions:

```csharp
Assert.IsNotNull(MedievalRTS.Visuals.ToonyRtsVisualLibrary.LoadUnit("Knight"));
Assert.IsNotNull(MedievalRTS.Visuals.ToonyRtsVisualLibrary.LoadBuilding("player_castle"));
Assert.IsNotNull(MedievalRTS.Visuals.ToonyRtsVisualLibrary.LoadDecoration("blue_banner"));
Assert.IsNotNull(MedievalRTS.Visuals.ToonyRtsVisualLibrary.LoadFx("building_destroyed"));
```

### Task 3: Visual Applier

**Files:**
- Create: `Assets/_Game/Scripts/Visuals/ToonyRtsVisualApplier.cs`

- [ ] **Step 1: Implement sanitized attachment**

```csharp
using UnityEngine;

namespace MedievalRTS.Visuals
{
    public static class ToonyRtsVisualApplier
    {
        public static GameObject Attach(GameObject root, GameObject visualPrefab, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            if (root == null || visualPrefab == null) return null;

            var visual = Object.Instantiate(visualPrefab, root.transform);
            visual.name = "ToonyVisual";
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = localRotation;
            visual.transform.localScale = localScale;

            foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
                Object.Destroy(collider);

            foreach (var rigidbody in visual.GetComponentsInChildren<Rigidbody>(true))
                Object.Destroy(rigidbody);

            return visual;
        }

        public static void HideRootRenderers(GameObject root)
        {
            if (root == null) return;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name == "ToonyVisual" || renderer.transform.IsChildOf(root.transform.Find("ToonyVisual")))
                    continue;
                renderer.enabled = false;
            }
        }
    }
}
```

- [ ] **Step 2: Add EditMode test for sanitizer**

Create a test prefab-like object in memory with collider and rigidbody, attach it, and assert the visual child no longer has colliders/rigidbodies after one editor frame if immediate destruction is needed. If delayed destruction makes this awkward, keep sanitizer verified through integration smoke test.

### Task 4: Apply Toony Visuals In TestBootstrap

**Files:**
- Modify: `Assets/_Game/Scripts/Testing/TestBootstrap.cs`

- [ ] **Step 1: Add helper methods**

Add methods near existing visual helpers:

```csharp
private void ApplyToonyUnitVisual(GameObject root, UnitDef def)
{
    var prefab = ToonyRtsVisualLibrary.LoadUnit(def.assetName);
    var visual = ToonyRtsVisualApplier.Attach(root, prefab, new Vector3(0f, -0.85f, 0f), Vector3.one * 0.9f, Quaternion.identity);
    if (visual != null) ToonyRtsVisualApplier.HideRootRenderers(root);
}

private void ApplyToonyBuildingVisual(GameObject root, string key, Vector3 offset, Vector3 scale)
{
    var prefab = ToonyRtsVisualLibrary.LoadBuilding(key);
    var visual = ToonyRtsVisualApplier.Attach(root, prefab, offset, scale, Quaternion.identity);
    if (visual != null) ToonyRtsVisualApplier.HideRootRenderers(root);
}
```

- [ ] **Step 2: Call helper from unit spawn**

After unit root creation and AI setup in `SpawnUnit`, call:

```csharp
ApplyToonyUnitVisual(go, def);
```

- [ ] **Step 3: Call helper from building creation points**

After existing `ApplyGeneratedFacade` calls, call:

```csharp
ApplyToonyBuildingVisual(go, "wall", Vector3.zero, new Vector3(1.2f, 1.2f, 1.2f));
ApplyToonyBuildingVisual(go, "tower", Vector3.zero, new Vector3(1.4f, 1.4f, 1.4f));
ApplyToonyBuildingVisual(go, "barracks", Vector3.zero, new Vector3(1.3f, 1.3f, 1.3f));
ApplyToonyBuildingVisual(go, "mage_tower", Vector3.zero, new Vector3(1.3f, 1.3f, 1.3f));
ApplyToonyBuildingVisual(go, "player_castle", Vector3.zero, new Vector3(1.8f, 1.8f, 1.8f));
ApplyToonyBuildingVisual(go, "enemy_castle", Vector3.zero, new Vector3(1.9f, 1.9f, 1.9f));
```

Adjust exact key per creation method.

### Task 5: Decorations And Docs

**Files:**
- Modify: `Assets/_Game/Scripts/Testing/TestBootstrap.cs`
- Modify: `docs/MadDragon_기획서.md`
- Modify: `docs/MadDragon_기획서.html`

- [ ] **Step 1: Add banners near castles**

Instantiate blue banners near the player side and red banners near enemy towers using `ToonyRtsVisualLibrary.LoadDecoration`.

- [ ] **Step 2: Update docs**

Add implementation note under v0.5 visual section:

```markdown
- Toony RTS Standard 에셋을 전투 화면의 유닛, 공성기, 주요 건물, 배너 장식에 비주얼 레이어로 적용했다. 기존 콜라이더/AI/HP 루트는 유지해 플레이 안정성을 보존했다.
```

### Task 6: Verification And Build

**Files:**
- Build output: `release/MadDragon.exe`
- Portable output: `MadDragon_v0.5_portable.exe`

- [ ] **Step 1: Run full EditMode tests**

Run:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -projectPath 'C:/Development/15_MD' -runTests -testPlatform EditMode -testResults 'C:/Development/15_MD/Logs/EditModeResults_toony_all.xml' -logFile 'C:/Development/15_MD/Logs/EditModeTests_toony_all.log'
```

Expected: all tests pass.

- [ ] **Step 2: Build Windows player**

Run:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -quit -projectPath 'C:/Development/15_MD' -buildWindows64Player 'C:/Development/15_MD/release/MadDragon.exe' -logFile 'C:/Development/15_MD/Logs/CodexBuild_toony.log'
```

Expected: `Build Finished, Result: Success.`

- [ ] **Step 3: Refresh portable files**

Run:

```powershell
Copy-Item -LiteralPath 'C:/Development/15_MD/release/MadDragon.exe' -Destination 'C:/Development/15_MD/MadDragon_v0.5_portable.exe' -Force
Copy-Item -LiteralPath 'C:/Development/15_MD/release/UnityPlayer.dll','C:/Development/15_MD/release/UnityCrashHandler64.exe' -Destination 'C:/Development/15_MD' -Force
Copy-Item -Path 'C:/Development/15_MD/release/MadDragon_Data/*' -Destination 'C:/Development/15_MD/MadDragon_v0.5_portable_Data' -Recurse -Force
Copy-Item -Path 'C:/Development/15_MD/release/MonoBleedingEdge/*' -Destination 'C:/Development/15_MD/MonoBleedingEdge' -Recurse -Force
```

Expected: `MadDragon_v0.5_portable_Data/Managed/MedievalRTS.Runtime.dll` timestamp updates.

---

## Self-Review

- Spec coverage: unit visuals, building visuals, decoration, fallback safety, tests, build are covered.
- Placeholder scan: no TBD/TODO placeholders.
- Type consistency: helper class names and methods are consistent across tasks.
