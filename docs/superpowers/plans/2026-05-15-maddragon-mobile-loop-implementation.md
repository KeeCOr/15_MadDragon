# MadDragon Mobile Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first playable mobile-first attack-defense loop with building production, headquarters storage, raid loss, streamlined mobile HUDs, and a Clash of Clans-inspired visual quality direction.

**Architecture:** Implement pure economy and raid calculation classes first, covered by EditMode tests. Persist the new economy state in `SaveData`, then connect it to a mobile-first prototype UI in `TestBootstrap.cs` through focused helper classes so the current scene remains playable. Visual quality work should create reusable art direction primitives, camera framing, feedback effects, and UI styling without blocking the economy loop.

**Tech Stack:** Unity 2022.3.62f3, C#, UGUI, NUnit EditMode tests, existing `MedievalRTS.Runtime` assembly, existing `TestBootstrap` prototype scene.

---

## File Map

| File | Responsibility |
|------|----------------|
| `Assets/_Game/Scripts/Economy/ResourceType.cs` | Defines resource identifiers used by storage, rewards, and raid loss. |
| `Assets/_Game/Scripts/Economy/ResourceWallet.cs` | Pure C# resource container with add, spend, clamp, and copy behavior. |
| `Assets/_Game/Scripts/Economy/ResourceProductionBuilding.cs` | Pure C# model for generated resources waiting in a building or headquarters. |
| `Assets/_Game/Scripts/Economy/ResourceStorageSystem.cs` | Pure C# system for production ticks, collection, headquarters capacity, and protection rate. |
| `Assets/_Game/Scripts/Economy/RaidLossCalculator.cs` | Pure C# raid loss calculator for uncollected and owned resources. |
| `Assets/_Game/Scripts/Economy/RaidForecast.cs` | Result model shown by UI before and after raid defense. |
| `Assets/_Game/Scripts/Progression/SaveData.cs` | Adds owned resources, stored resources, headquarters level, and last collection timestamp. |
| `Assets/_Game/Scripts/UI/MobileUiFactory.cs` | Reusable UGUI helpers for safe-area root, top bars, bottom tabs, bottom sheets, and large mobile buttons. |
| `Assets/_Game/Scripts/UI/MobileHudTheme.cs` | Shared colors, font sizes, button dimensions, and visual quality constants. |
| `Assets/_Game/Scripts/UI/CampaignHubScreen.cs` | Prototype hub UI: resources, storage, raid risk, and next-action CTA. |
| `Assets/_Game/Scripts/UI/BaseManagementScreen.cs` | Prototype base UI: collect, build, garrison, upgrade tabs and raid forecast. |
| `Assets/_Game/Scripts/UI/AttackPrepScreen.cs` | Prototype attack prep UI: squad slots, spell slots, intel tab. |
| `Assets/_Game/Scripts/UI/MobileBattleHud.cs` | Compact battle HUD with top status and bottom quick bar. |
| `Assets/_Game/Scripts/Visuals/MobileVisualStyle.cs` | Camera, lighting, outline, shadow, and feedback defaults for the polished mobile target. |
| `Assets/_Game/Scripts/Testing/TestBootstrap.cs` | Integration harness that calls new economy/UI classes while keeping current battle logic. |
| `Assets/_Game/Tests/EditMode/ResourceStorageSystemTests.cs` | Tests production, collection, capacity, and protection. |
| `Assets/_Game/Tests/EditMode/RaidLossCalculatorTests.cs` | Tests raid loss by outcome and protection rate. |
| `Assets/_Game/Tests/EditMode/SaveSystemTests.cs` | Extends save/load coverage for new economy fields. |
| `docs/MadDragon_기획서.md` | Update if implementation changes any planning decisions. |
| `docs/MadDragon_기획서.html` | HTML version of the updated planning document. |

---

## Task 1: Economy Models and Storage Tests

**Files:**
- Create: `Assets/_Game/Scripts/Economy/ResourceType.cs`
- Create: `Assets/_Game/Scripts/Economy/ResourceWallet.cs`
- Create: `Assets/_Game/Scripts/Economy/ResourceProductionBuilding.cs`
- Create: `Assets/_Game/Scripts/Economy/ResourceStorageSystem.cs`
- Test: `Assets/_Game/Tests/EditMode/ResourceStorageSystemTests.cs`

- [ ] **Step 1: Write the failing storage tests**

Create `Assets/_Game/Tests/EditMode/ResourceStorageSystemTests.cs`:

```csharp
using NUnit.Framework;
using MedievalRTS.Economy;

public class ResourceStorageSystemTests
{
    [Test]
    public void TickProduction_AddsGeneratedGoldToStoredResources()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 1);
        storage.AddProducer(new ResourceProductionBuilding("GoldMine", ResourceType.Gold, 10, 500));

        storage.TickProduction(12f);

        Assert.AreEqual(120, storage.Stored.Get(ResourceType.Gold));
    }

    [Test]
    public void TickProduction_ClampsToBuildingCapacity()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 1);
        storage.AddProducer(new ResourceProductionBuilding("GoldMine", ResourceType.Gold, 50, 200));

        storage.TickProduction(10f);

        Assert.AreEqual(200, storage.Stored.Get(ResourceType.Gold));
    }

    [Test]
    public void CollectAll_MovesStoredResourcesToOwnedWallet()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 1);
        var owned = new ResourceWallet();
        storage.Stored.Add(ResourceType.Gold, 300);
        storage.Stored.Add(ResourceType.Honor, 20);

        var collected = storage.CollectAll(owned);

        Assert.AreEqual(300, collected.Get(ResourceType.Gold));
        Assert.AreEqual(20, collected.Get(ResourceType.Honor));
        Assert.AreEqual(300, owned.Get(ResourceType.Gold));
        Assert.AreEqual(20, owned.Get(ResourceType.Honor));
        Assert.AreEqual(0, storage.Stored.Get(ResourceType.Gold));
        Assert.AreEqual(0, storage.Stored.Get(ResourceType.Honor));
    }

    [Test]
    public void HeadquartersLevel_IncreasesStorageCapacity()
    {
        var level1 = new ResourceStorageSystem(headquartersLevel: 1);
        var level3 = new ResourceStorageSystem(headquartersLevel: 3);

        Assert.AreEqual(1000, level1.GetHeadquartersCapacity(ResourceType.Gold));
        Assert.AreEqual(3000, level3.GetHeadquartersCapacity(ResourceType.Gold));
    }

    [Test]
    public void HeadquartersLevel_IncreasesProtectionRate()
    {
        var level1 = new ResourceStorageSystem(headquartersLevel: 1);
        var level4 = new ResourceStorageSystem(headquartersLevel: 4);

        Assert.AreEqual(0.10f, level1.ProtectionRate, 0.0001f);
        Assert.AreEqual(0.40f, level4.ProtectionRate, 0.0001f);
    }
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -quit -projectPath 'C:/Development/15_MD' -runTests -testPlatform EditMode -testResults 'C:/Development/15_MD/Logs/EditModeResults.xml' -logFile 'C:/Development/15_MD/Logs/EditModeTests.log'
```

Expected: compile errors for missing `MedievalRTS.Economy` types.

- [ ] **Step 3: Add `ResourceType`**

Create `Assets/_Game/Scripts/Economy/ResourceType.cs`:

```csharp
namespace MedievalRTS.Economy
{
    public enum ResourceType
    {
        Gold,
        Honor,
        Stars
    }
}
```

- [ ] **Step 4: Add `ResourceWallet`**

Create `Assets/_Game/Scripts/Economy/ResourceWallet.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace MedievalRTS.Economy
{
    [Serializable]
    public class ResourceWallet
    {
        public Dictionary<ResourceType, int> Amounts { get; set; } = new();

        public int Get(ResourceType type)
        {
            return Amounts.TryGetValue(type, out var value) ? value : 0;
        }

        public void Set(ResourceType type, int amount)
        {
            Amounts[type] = Math.Max(0, amount);
        }

        public void Add(ResourceType type, int amount)
        {
            if (amount <= 0) return;
            Set(type, Get(type) + amount);
        }

        public int Remove(ResourceType type, int amount)
        {
            if (amount <= 0) return 0;
            int current = Get(type);
            int removed = Math.Min(current, amount);
            Set(type, current - removed);
            return removed;
        }

        public bool CanSpend(ResourceType type, int amount)
        {
            return amount >= 0 && Get(type) >= amount;
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            if (!CanSpend(type, amount)) return false;
            Remove(type, amount);
            return true;
        }

        public ResourceWallet Copy()
        {
            var copy = new ResourceWallet();
            foreach (var pair in Amounts)
                copy.Set(pair.Key, pair.Value);
            return copy;
        }

        public void Clear()
        {
            Amounts.Clear();
        }
    }
}
```

- [ ] **Step 5: Add production building model**

Create `Assets/_Game/Scripts/Economy/ResourceProductionBuilding.cs`:

```csharp
using System;

namespace MedievalRTS.Economy
{
    [Serializable]
    public class ResourceProductionBuilding
    {
        public string Id { get; }
        public ResourceType ResourceType { get; }
        public float ProductionPerSecond { get; }
        public int Capacity { get; }

        private float _fractionalCarry;

        public ResourceProductionBuilding(string id, ResourceType resourceType, float productionPerSecond, int capacity)
        {
            Id = id;
            ResourceType = resourceType;
            ProductionPerSecond = Math.Max(0f, productionPerSecond);
            Capacity = Math.Max(0, capacity);
        }

        public int Produce(float deltaSeconds, int currentStored)
        {
            if (deltaSeconds <= 0f || ProductionPerSecond <= 0f) return 0;
            float raw = ProductionPerSecond * deltaSeconds + _fractionalCarry;
            int generated = (int)Math.Floor(raw);
            _fractionalCarry = raw - generated;
            int remainingCapacity = Math.Max(0, Capacity - currentStored);
            return Math.Min(generated, remainingCapacity);
        }
    }
}
```

- [ ] **Step 6: Add storage system**

Create `Assets/_Game/Scripts/Economy/ResourceStorageSystem.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace MedievalRTS.Economy
{
    public class ResourceStorageSystem
    {
        private readonly List<ResourceProductionBuilding> _producers = new();

        public int HeadquartersLevel { get; private set; }
        public ResourceWallet Stored { get; } = new();

        public float ProtectionRate => Math.Min(0.75f, HeadquartersLevel * 0.10f);

        public ResourceStorageSystem(int headquartersLevel)
        {
            HeadquartersLevel = Math.Max(1, headquartersLevel);
        }

        public void SetHeadquartersLevel(int level)
        {
            HeadquartersLevel = Math.Max(1, level);
        }

        public void AddProducer(ResourceProductionBuilding producer)
        {
            if (producer == null) throw new ArgumentNullException(nameof(producer));
            _producers.Add(producer);
        }

        public int GetHeadquartersCapacity(ResourceType type)
        {
            return type switch
            {
                ResourceType.Gold => HeadquartersLevel * 1000,
                ResourceType.Honor => HeadquartersLevel * 100,
                ResourceType.Stars => int.MaxValue,
                _ => HeadquartersLevel * 1000
            };
        }

        public void TickProduction(float deltaSeconds)
        {
            foreach (var producer in _producers)
            {
                int currentStored = Stored.Get(producer.ResourceType);
                int generated = producer.Produce(deltaSeconds, currentStored);
                int capacity = Math.Min(producer.Capacity, GetHeadquartersCapacity(producer.ResourceType));
                int clamped = Math.Min(generated, Math.Max(0, capacity - currentStored));
                Stored.Add(producer.ResourceType, clamped);
            }
        }

        public ResourceWallet CollectAll(ResourceWallet owned)
        {
            if (owned == null) throw new ArgumentNullException(nameof(owned));
            var collected = Stored.Copy();
            foreach (var pair in collected.Amounts)
                owned.Add(pair.Key, pair.Value);
            Stored.Clear();
            return collected;
        }
    }
}
```

- [ ] **Step 7: Run storage tests**

Run the Unity EditMode command from Step 2.

Expected: `ResourceStorageSystemTests` pass.

- [ ] **Step 8: Commit**

```powershell
git add Assets/_Game/Scripts/Economy Assets/_Game/Tests/EditMode/ResourceStorageSystemTests.cs
git commit -m "feat: add resource storage economy"
```

---

## Task 2: Raid Loss Calculator

**Files:**
- Create: `Assets/_Game/Scripts/Economy/RaidOutcome.cs`
- Create: `Assets/_Game/Scripts/Economy/RaidForecast.cs`
- Create: `Assets/_Game/Scripts/Economy/RaidLossCalculator.cs`
- Test: `Assets/_Game/Tests/EditMode/RaidLossCalculatorTests.cs`

- [ ] **Step 1: Write the failing raid loss tests**

Create `Assets/_Game/Tests/EditMode/RaidLossCalculatorTests.cs`:

```csharp
using NUnit.Framework;
using MedievalRTS.Economy;

public class RaidLossCalculatorTests
{
    [Test]
    public void DefenseSuccess_LosesNoResources()
    {
        var stored = new ResourceWallet();
        var owned = new ResourceWallet();
        stored.Add(ResourceType.Gold, 1200);
        owned.Add(ResourceType.Gold, 8000);

        var forecast = RaidLossCalculator.Calculate(stored, owned, RaidOutcome.DefenseSuccess, 0.35f);

        Assert.AreEqual(0, forecast.StoredLoss.Get(ResourceType.Gold));
        Assert.AreEqual(0, forecast.OwnedLoss.Get(ResourceType.Gold));
    }

    [Test]
    public void NarrowFailure_AppliesLossRatesAfterProtection()
    {
        var stored = new ResourceWallet();
        var owned = new ResourceWallet();
        stored.Add(ResourceType.Gold, 1200);
        owned.Add(ResourceType.Gold, 8000);

        var forecast = RaidLossCalculator.Calculate(stored, owned, RaidOutcome.NarrowFailure, 0.35f);

        Assert.AreEqual(234, forecast.StoredLoss.Get(ResourceType.Gold));
        Assert.AreEqual(260, forecast.OwnedLoss.Get(ResourceType.Gold));
    }

    [Test]
    public void HeadquartersDestroyed_CanStealFromStoredAndOwned()
    {
        var stored = new ResourceWallet();
        var owned = new ResourceWallet();
        stored.Add(ResourceType.Gold, 1200);
        owned.Add(ResourceType.Gold, 8000);

        var forecast = RaidLossCalculator.Calculate(stored, owned, RaidOutcome.HeadquartersDestroyed, 0.0f);

        Assert.AreEqual(1200, forecast.StoredLoss.Get(ResourceType.Gold));
        Assert.AreEqual(1600, forecast.OwnedLoss.Get(ResourceType.Gold));
    }

    [Test]
    public void ApplyLoss_RemovesForecastAmounts()
    {
        var stored = new ResourceWallet();
        var owned = new ResourceWallet();
        stored.Add(ResourceType.Gold, 1000);
        owned.Add(ResourceType.Gold, 5000);
        var forecast = RaidLossCalculator.Calculate(stored, owned, RaidOutcome.ClearFailure, 0.5f);

        RaidLossCalculator.ApplyLoss(stored, owned, forecast);

        Assert.AreEqual(650, stored.Get(ResourceType.Gold));
        Assert.AreEqual(4625, owned.Get(ResourceType.Gold));
    }
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run the Unity EditMode command from Task 1.

Expected: compile errors for missing raid types.

- [ ] **Step 3: Add raid outcome enum**

Create `Assets/_Game/Scripts/Economy/RaidOutcome.cs`:

```csharp
namespace MedievalRTS.Economy
{
    public enum RaidOutcome
    {
        DefenseSuccess,
        NarrowFailure,
        ClearFailure,
        HeadquartersDestroyed
    }
}
```

- [ ] **Step 4: Add raid forecast model**

Create `Assets/_Game/Scripts/Economy/RaidForecast.cs`:

```csharp
namespace MedievalRTS.Economy
{
    public class RaidForecast
    {
        public RaidOutcome Outcome { get; }
        public float ProtectionRate { get; }
        public ResourceWallet StoredLoss { get; }
        public ResourceWallet OwnedLoss { get; }

        public RaidForecast(RaidOutcome outcome, float protectionRate, ResourceWallet storedLoss, ResourceWallet ownedLoss)
        {
            Outcome = outcome;
            ProtectionRate = protectionRate;
            StoredLoss = storedLoss;
            OwnedLoss = ownedLoss;
        }
    }
}
```

- [ ] **Step 5: Add raid loss calculator**

Create `Assets/_Game/Scripts/Economy/RaidLossCalculator.cs`:

```csharp
using System;

namespace MedievalRTS.Economy
{
    public static class RaidLossCalculator
    {
        public static RaidForecast Calculate(ResourceWallet stored, ResourceWallet owned, RaidOutcome outcome, float protectionRate)
        {
            if (stored == null) throw new ArgumentNullException(nameof(stored));
            if (owned == null) throw new ArgumentNullException(nameof(owned));

            var storedLoss = new ResourceWallet();
            var ownedLoss = new ResourceWallet();
            var rates = GetRates(outcome);
            float lossMultiplier = 1f - Math.Clamp(protectionRate, 0f, 0.75f);

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                storedLoss.Set(type, CalculateLoss(stored.Get(type), rates.storedRate, lossMultiplier));
                ownedLoss.Set(type, CalculateLoss(owned.Get(type), rates.ownedRate, lossMultiplier));
            }

            return new RaidForecast(outcome, protectionRate, storedLoss, ownedLoss);
        }

        public static void ApplyLoss(ResourceWallet stored, ResourceWallet owned, RaidForecast forecast)
        {
            if (stored == null) throw new ArgumentNullException(nameof(stored));
            if (owned == null) throw new ArgumentNullException(nameof(owned));
            if (forecast == null) throw new ArgumentNullException(nameof(forecast));

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                stored.Remove(type, forecast.StoredLoss.Get(type));
                owned.Remove(type, forecast.OwnedLoss.Get(type));
            }
        }

        private static (float storedRate, float ownedRate) GetRates(RaidOutcome outcome)
        {
            return outcome switch
            {
                RaidOutcome.DefenseSuccess => (0f, 0f),
                RaidOutcome.NarrowFailure => (0.30f, 0.05f),
                RaidOutcome.ClearFailure => (0.70f, 0.15f),
                RaidOutcome.HeadquartersDestroyed => (1.00f, 0.20f),
                _ => (0f, 0f)
            };
        }

        private static int CalculateLoss(int amount, float rate, float lossMultiplier)
        {
            return Math.Max(0, (int)Math.Round(amount * rate * lossMultiplier));
        }
    }
}
```

- [ ] **Step 6: Run raid tests**

Run the Unity EditMode command from Task 1.

Expected: `RaidLossCalculatorTests` pass.

- [ ] **Step 7: Commit**

```powershell
git add Assets/_Game/Scripts/Economy Assets/_Game/Tests/EditMode/RaidLossCalculatorTests.cs
git commit -m "feat: add raid loss calculator"
```

---

## Task 3: Save Data Integration

**Files:**
- Modify: `Assets/_Game/Scripts/Progression/SaveData.cs`
- Modify: `Assets/_Game/Tests/EditMode/SaveSystemTests.cs`

- [ ] **Step 1: Add failing save tests**

Append these tests to `Assets/_Game/Tests/EditMode/SaveSystemTests.cs`:

```csharp
[Test]
public void Save_Then_Load_PreservesEconomyState()
{
    var data = new SaveData();
    data.OwnedResources.Add(MedievalRTS.Economy.ResourceType.Gold, 8000);
    data.StoredResources.Add(MedievalRTS.Economy.ResourceType.Gold, 1200);
    data.HeadquartersLevel = 3;
    data.LastCollectionUnixSeconds = 1778780000;

    SaveSystem.Save(data, _testPath);
    var loaded = SaveSystem.Load(_testPath);

    Assert.AreEqual(8000, loaded.OwnedResources.Get(MedievalRTS.Economy.ResourceType.Gold));
    Assert.AreEqual(1200, loaded.StoredResources.Get(MedievalRTS.Economy.ResourceType.Gold));
    Assert.AreEqual(3, loaded.HeadquartersLevel);
    Assert.AreEqual(1778780000, loaded.LastCollectionUnixSeconds);
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run the Unity EditMode command from Task 1.

Expected: compile errors for missing save properties.

- [ ] **Step 3: Extend `SaveData`**

Replace `Assets/_Game/Scripts/Progression/SaveData.cs` with:

```csharp
using System.Collections.Generic;
using MedievalRTS.Base;
using MedievalRTS.Economy;

namespace MedievalRTS.Progression
{
    public class SaveData
    {
        public Dictionary<int, int> StageStars { get; set; } = new();
        public Dictionary<string, int> UnitLevels { get; set; } = new();
        public BaseLayoutData PlayerBase { get; set; }

        public ResourceWallet OwnedResources { get; set; } = new();
        public ResourceWallet StoredResources { get; set; } = new();
        public int HeadquartersLevel { get; set; } = 1;
        public long LastCollectionUnixSeconds { get; set; }
    }
}
```

- [ ] **Step 4: Guard old save files after deserialization**

Modify `SaveSystem.Load` in `Assets/_Game/Scripts/Progression/SaveSystem.cs` so it normalizes null fields:

```csharp
public static SaveData Load(string path = null)
{
    path ??= DefaultPath;
    if (!File.Exists(path)) return new SaveData();
    try
    {
        var data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path))
                   ?? new SaveData();
        data.StageStars ??= new System.Collections.Generic.Dictionary<int, int>();
        data.UnitLevels ??= new System.Collections.Generic.Dictionary<string, int>();
        data.OwnedResources ??= new Economy.ResourceWallet();
        data.StoredResources ??= new Economy.ResourceWallet();
        if (data.HeadquartersLevel <= 0) data.HeadquartersLevel = 1;
        return data;
    }
    catch (JsonException ex)
    {
        Debug.LogError($"Failed to load save data: {ex.Message}");
        return new SaveData();
    }
}
```

- [ ] **Step 5: Run save tests**

Run the Unity EditMode command from Task 1.

Expected: all existing `SaveSystemTests` plus the new economy save test pass.

- [ ] **Step 6: Commit**

```powershell
git add Assets/_Game/Scripts/Progression/SaveData.cs Assets/_Game/Scripts/Progression/SaveSystem.cs Assets/_Game/Tests/EditMode/SaveSystemTests.cs
git commit -m "feat: persist economy state"
```

---

## Task 4: Mobile UI Foundation

**Files:**
- Create: `Assets/_Game/Scripts/UI/MobileHudTheme.cs`
- Create: `Assets/_Game/Scripts/UI/MobileUiFactory.cs`

- [ ] **Step 1: Create the mobile HUD theme**

Create `Assets/_Game/Scripts/UI/MobileHudTheme.cs`:

```csharp
using UnityEngine;

namespace MedievalRTS.UI
{
    public static class MobileHudTheme
    {
        public static readonly Color Panel = new(0.06f, 0.08f, 0.10f, 0.88f);
        public static readonly Color PanelStrong = new(0.04f, 0.05f, 0.07f, 0.94f);
        public static readonly Color Gold = new(1.00f, 0.78f, 0.24f, 1f);
        public static readonly Color Honor = new(0.55f, 0.85f, 1.00f, 1f);
        public static readonly Color Danger = new(0.95f, 0.25f, 0.20f, 1f);
        public static readonly Color Good = new(0.30f, 0.85f, 0.45f, 1f);
        public static readonly Color PrimaryButton = new(0.12f, 0.36f, 0.62f, 1f);
        public static readonly Color SecondaryButton = new(0.20f, 0.22f, 0.25f, 1f);

        public const int TopBarFont = 18;
        public const int BodyFont = 15;
        public const int ButtonFont = 15;
        public const float BottomSheetHeight = 320f;
        public static readonly Vector2 LargeButtonSize = new(176f, 64f);
        public static readonly Vector2 QuickButtonSize = new(132f, 58f);
    }
}
```

- [ ] **Step 2: Create the UI factory**

Create `Assets/_Game/Scripts/UI/MobileUiFactory.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public static class MobileUiFactory
    {
        public static GameObject CreatePanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>().color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return go;
        }

        public static Text CreateLabel(GameObject parent, string name, Font font, string text, int fontSize, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var label = go.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = fontSize;
            var rt = label.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8f, 4f);
            rt.offsetMax = new Vector2(-8f, -4f);
            return label;
        }

        public static Button CreateButton(GameObject parent, string name, Font font, string label, Color color, System.Action onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>().color = color;
            var button = go.AddComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());
            CreateLabel(go, "Label", font, label, MobileHudTheme.ButtonFont, Color.white, TextAnchor.MiddleCenter);
            return button;
        }

        public static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
        }
    }
}
```

- [ ] **Step 3: Compile check**

Run the Unity EditMode command from Task 1.

Expected: compile succeeds, existing tests still pass.

- [ ] **Step 4: Commit**

```powershell
git add Assets/_Game/Scripts/UI/MobileHudTheme.cs Assets/_Game/Scripts/UI/MobileUiFactory.cs
git commit -m "feat: add mobile ui foundation"
```

---

## Task 5: Campaign Hub and Base Management Prototype

**Files:**
- Create: `Assets/_Game/Scripts/UI/CampaignHubScreen.cs`
- Create: `Assets/_Game/Scripts/UI/BaseManagementScreen.cs`
- Modify: `Assets/_Game/Scripts/Testing/TestBootstrap.cs`

- [ ] **Step 1: Add `CampaignHubScreen`**

Create `Assets/_Game/Scripts/UI/CampaignHubScreen.cs`:

```csharp
using MedievalRTS.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class CampaignHubScreen
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private Text _summary;

        public GameObject Root => _root;

        public CampaignHubScreen(GameObject canvas, Font font)
        {
            _font = font;
            _root = MobileUiFactory.CreatePanel(canvas, "CampaignHubScreen", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MobileHudTheme.PanelStrong);
            Build();
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(ResourceWallet owned, ResourceWallet stored, RaidForecast forecast)
        {
            string loss = forecast == null ? "예상 약탈 없음" : $"예상 약탈: 저장 {forecast.StoredLoss.Get(ResourceType.Gold)}G / 보유 {forecast.OwnedLoss.Get(ResourceType.Gold)}G";
            _summary.text = $"골드 {owned.Get(ResourceType.Gold)}  명예 {owned.Get(ResourceType.Honor)}\n저장 골드 {stored.Get(ResourceType.Gold)}\n{loss}";
        }

        private void Build()
        {
            _summary = MobileUiFactory.CreateLabel(_root, "Summary", _font, "", 20, Color.white, TextAnchor.MiddleCenter);
            var rt = _summary.rectTransform;
            MobileUiFactory.SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(620f, 180f));
        }
    }
}
```

- [ ] **Step 2: Add `BaseManagementScreen`**

Create `Assets/_Game/Scripts/UI/BaseManagementScreen.cs`:

```csharp
using MedievalRTS.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class BaseManagementScreen
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private readonly System.Action _collectAll;
        private Text _status;

        public GameObject Root => _root;

        public BaseManagementScreen(GameObject canvas, Font font, System.Action collectAll)
        {
            _font = font;
            _collectAll = collectAll;
            _root = MobileUiFactory.CreatePanel(canvas, "BaseManagementScreen", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MobileHudTheme.PanelStrong);
            Build();
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(ResourceWallet owned, ResourceWallet stored, RaidForecast forecast, int headquartersLevel)
        {
            string risk = forecast == null ? "위험도 낮음" : $"예상 손실 {forecast.StoredLoss.Get(ResourceType.Gold) + forecast.OwnedLoss.Get(ResourceType.Gold)}G";
            _status.text = $"본부 Lv.{headquartersLevel}\n보유 골드 {owned.Get(ResourceType.Gold)}\n저장 골드 {stored.Get(ResourceType.Gold)}\n{risk}";
        }

        private void Build()
        {
            _status = MobileUiFactory.CreateLabel(_root, "Status", _font, "", 20, Color.white, TextAnchor.MiddleCenter);
            MobileUiFactory.SetRect(_status.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(640f, 190f));

            var collect = MobileUiFactory.CreateButton(_root, "CollectAll", _font, "모두 수거", MobileHudTheme.Good, _collectAll);
            MobileUiFactory.SetRect(collect.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), MobileHudTheme.LargeButtonSize);
        }
    }
}
```

- [ ] **Step 3: Wire prototype screens into `TestBootstrap`**

In `Assets/_Game/Scripts/Testing/TestBootstrap.cs`, add these `using` lines:

```csharp
using MedievalRTS.Economy;
using MedievalRTS.UI;
```

Add fields near the UI fields:

```csharp
private ResourceWallet _ownedResources;
private ResourceStorageSystem _resourceStorage;
private CampaignHubScreen _campaignHubScreen;
private BaseManagementScreen _baseManagementScreen;
```

At the start of `Start()`, before `BuildWorld();`, initialize:

```csharp
_ownedResources = new ResourceWallet();
_ownedResources.Add(ResourceType.Gold, _gold);
_resourceStorage = new ResourceStorageSystem(1);
_resourceStorage.AddProducer(new ResourceProductionBuilding("GoldMine", ResourceType.Gold, 8f, 1200));
```

After `BuildUI();`, create screens:

```csharp
BuildMobileLoopScreens();
RefreshMobileLoopScreens();
```

Add methods:

```csharp
private void BuildMobileLoopScreens()
{
    _campaignHubScreen = new CampaignHubScreen(_canvas, _font);
    _baseManagementScreen = new BaseManagementScreen(_canvas, _font, CollectStoredResources);
    _campaignHubScreen.SetVisible(true);
    _baseManagementScreen.SetVisible(false);
}

private RaidForecast BuildCurrentRaidForecast()
{
    return RaidLossCalculator.Calculate(
        _resourceStorage.Stored,
        _ownedResources,
        RaidOutcome.ClearFailure,
        _resourceStorage.ProtectionRate);
}

private void RefreshMobileLoopScreens()
{
    var forecast = BuildCurrentRaidForecast();
    _campaignHubScreen?.Refresh(_ownedResources, _resourceStorage.Stored, forecast);
    _baseManagementScreen?.Refresh(_ownedResources, _resourceStorage.Stored, forecast, _resourceStorage.HeadquartersLevel);
}

private void CollectStoredResources()
{
    _resourceStorage.CollectAll(_ownedResources);
    _gold = _ownedResources.Get(ResourceType.Gold);
    RefreshMobileLoopScreens();
    RefreshPrepGold();
}
```

In `Update()`, before phase-specific returns, tick production:

```csharp
_resourceStorage?.TickProduction(Time.deltaTime);
RefreshMobileLoopScreens();
```

- [ ] **Step 4: Compile check**

Run the Unity EditMode command from Task 1.

Expected: compile succeeds, tests pass.

- [ ] **Step 5: Manual scene check**

Open `Assets/_Game/Scenes/05_TestBattle.unity` in Unity Editor and press Play.

Expected:
- A prototype hub overlay appears.
- Stored gold increases over time.
- `모두 수거` moves stored gold into owned gold when the base screen is visible during later UI wiring.

- [ ] **Step 6: Commit**

```powershell
git add Assets/_Game/Scripts/UI/CampaignHubScreen.cs Assets/_Game/Scripts/UI/BaseManagementScreen.cs Assets/_Game/Scripts/Testing/TestBootstrap.cs
git commit -m "feat: add mobile hub and base prototype"
```

---

## Task 6: Attack Prep Loadout and Spell Slots

**Files:**
- Create: `Assets/_Game/Scripts/UI/AttackPrepScreen.cs`
- Modify: `Assets/_Game/Scripts/Testing/TestBootstrap.cs`

- [ ] **Step 1: Create attack prep screen**

Create `Assets/_Game/Scripts/UI/AttackPrepScreen.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class AttackPrepScreen
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private readonly System.Action _startAttack;
        private Text _slots;

        public GameObject Root => _root;

        public AttackPrepScreen(GameObject canvas, Font font, System.Action startAttack)
        {
            _font = font;
            _startAttack = startAttack;
            _root = MobileUiFactory.CreatePanel(canvas, "AttackPrepScreen", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MobileHudTheme.PanelStrong);
            Build();
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(string squadSummary, string spellSummary, string intelSummary)
        {
            _slots.text = $"분대\n{squadSummary}\n\n마법\n{spellSummary}\n\n정보\n{intelSummary}";
        }

        private void Build()
        {
            _slots = MobileUiFactory.CreateLabel(_root, "Slots", _font, "", 18, Color.white, TextAnchor.MiddleCenter);
            MobileUiFactory.SetRect(_slots.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(650f, 300f));

            var start = MobileUiFactory.CreateButton(_root, "StartAttack", _font, "공격 시작", MobileHudTheme.PrimaryButton, _startAttack);
            MobileUiFactory.SetRect(start.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), MobileHudTheme.LargeButtonSize);
        }
    }
}
```

- [ ] **Step 2: Add attack prep screen to `TestBootstrap`**

Add field:

```csharp
private AttackPrepScreen _attackPrepScreen;
```

In `BuildMobileLoopScreens()`, add:

```csharp
_attackPrepScreen = new AttackPrepScreen(_canvas, _font, EnterBattle);
_attackPrepScreen.SetVisible(false);
```

In `RefreshMobileLoopScreens()`, add:

```csharp
_attackPrepScreen?.Refresh(
    BuildRosterSummary(),
    "화염구 / 치유 / 빙결",
    "예상 위협: 중앙 타워, 후방 성채");
```

Add:

```csharp
private string BuildRosterSummary()
{
    var sb = new StringBuilder();
    for (int i = 0; i < Defs.Length; i++)
    {
        if (_roster[i] <= 0) continue;
        if (sb.Length > 0) sb.Append(" / ");
        sb.Append($"{Defs[i].name} x{_roster[i]}");
    }
    return sb.Length == 0 ? "빈 슬롯" : sb.ToString();
}
```

- [ ] **Step 3: Manual UI rule**

Keep this first pass as a prototype overlay. Do not remove the existing prep buttons until the new flow can start, fight, and show results.

- [ ] **Step 4: Compile check**

Run the Unity EditMode command from Task 1.

Expected: compile succeeds, tests pass.

- [ ] **Step 5: Commit**

```powershell
git add Assets/_Game/Scripts/UI/AttackPrepScreen.cs Assets/_Game/Scripts/Testing/TestBootstrap.cs
git commit -m "feat: add mobile attack prep screen"
```

---

## Task 7: Compact Mobile Battle HUD

**Files:**
- Create: `Assets/_Game/Scripts/UI/MobileBattleHud.cs`
- Modify: `Assets/_Game/Scripts/Testing/TestBootstrap.cs`

- [ ] **Step 1: Create compact battle HUD**

Create `Assets/_Game/Scripts/UI/MobileBattleHud.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace MedievalRTS.UI
{
    public class MobileBattleHud
    {
        private readonly GameObject _root;
        private readonly Font _font;
        private Text _topStatus;
        private Text _quickStatus;

        public GameObject Root => _root;

        public MobileBattleHud(GameObject canvas, Font font)
        {
            _font = font;
            _root = new GameObject("MobileBattleHud");
            _root.transform.SetParent(canvas.transform, false);
            var rt = _root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            Build();
        }

        public void SetVisible(bool visible) => _root.SetActive(visible);

        public void Refresh(int secondsRemaining, int targetHp, int earnedGold, int earnedHonor)
        {
            _topStatus.text = $"{secondsRemaining}s     목표 HP {targetHp}     +{earnedGold}G +{earnedHonor}명예";
            _quickStatus.text = "집결  공격  대기  마법";
        }

        private void Build()
        {
            var top = MobileUiFactory.CreatePanel(_root, "TopStatus", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -58f), new Vector2(-8f, -8f), MobileHudTheme.Panel);
            _topStatus = MobileUiFactory.CreateLabel(top, "Label", _font, "", MobileHudTheme.TopBarFont, Color.white, TextAnchor.MiddleCenter);

            var bottom = MobileUiFactory.CreatePanel(_root, "QuickBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f), new Vector2(-8f, 90f), MobileHudTheme.Panel);
            _quickStatus = MobileUiFactory.CreateLabel(bottom, "Label", _font, "", MobileHudTheme.ButtonFont, Color.white, TextAnchor.MiddleCenter);
        }
    }
}
```

- [ ] **Step 2: Wire battle HUD**

Add field in `TestBootstrap.cs`:

```csharp
private MobileBattleHud _mobileBattleHud;
```

In `BuildMobileLoopScreens()`:

```csharp
_mobileBattleHud = new MobileBattleHud(_canvas, _font);
_mobileBattleHud.SetVisible(false);
```

In `EnterBattle()`, after existing HUD visibility logic:

```csharp
_mobileBattleHud?.SetVisible(true);
_campaignHubScreen?.SetVisible(false);
_baseManagementScreen?.SetVisible(false);
_attackPrepScreen?.SetVisible(false);
```

In battle `Update()`, after timer and target HP update:

```csharp
_mobileBattleHud?.Refresh(
    remSec,
    _defenseMode ? (_playerCastle != null ? _playerCastle.CurrentHp : 0) : (_enemyCastle != null ? _enemyCastle.CurrentHp : 0),
    _earnedGold,
    _earnedValor);
```

- [ ] **Step 3: Keep old HUD available**

Keep the existing old battle HUD active for one pass if commands are not yet duplicated. The compact HUD is the target layout and can become authoritative after input wiring is moved.

- [ ] **Step 4: Compile check**

Run the Unity EditMode command from Task 1.

Expected: compile succeeds, tests pass.

- [ ] **Step 5: Commit**

```powershell
git add Assets/_Game/Scripts/UI/MobileBattleHud.cs Assets/_Game/Scripts/Testing/TestBootstrap.cs
git commit -m "feat: add compact mobile battle hud"
```

---

## Task 8: Clash-Like Visual Quality Foundation

**Files:**
- Create: `Assets/_Game/Scripts/Visuals/MobileVisualStyle.cs`
- Modify: `Assets/_Game/Scripts/Testing/TestBootstrap.cs`
- Modify or create materials under: `Assets/_Game/Materials/`

- [ ] **Step 1: Add visual style defaults**

Create `Assets/_Game/Scripts/Visuals/MobileVisualStyle.cs`:

```csharp
using UnityEngine;

namespace MedievalRTS.Visuals
{
    public static class MobileVisualStyle
    {
        public static readonly Color FriendlyBlue = new(0.20f, 0.45f, 0.95f);
        public static readonly Color EnemyRed = new(0.85f, 0.16f, 0.12f);
        public static readonly Color GrassBase = new(0.30f, 0.58f, 0.24f);
        public static readonly Color StoneWarm = new(0.55f, 0.52f, 0.47f);
        public static readonly Color GoldAccent = new(1.00f, 0.72f, 0.20f);

        public static void ApplyCamera(Camera camera, bool defenseSide)
        {
            if (camera == null) return;
            camera.orthographic = false;
            camera.fieldOfView = 42f;
            camera.transform.SetPositionAndRotation(
                defenseSide ? new Vector3(-12f, 30f, -24f) : new Vector3(4f, 32f, -28f),
                Quaternion.Euler(52f, 0f, 0f));
        }

        public static void ApplyLight(Light light)
        {
            if (light == null) return;
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.92f, 0.82f);
            light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }
    }
}
```

- [ ] **Step 2: Apply camera and lighting style in `TestBootstrap`**

Add:

```csharp
using MedievalRTS.Visuals;
```

In `SetupCamera()`, after camera creation or lookup:

```csharp
MobileVisualStyle.ApplyCamera(Camera.main, _defenseMode);
```

In `SetupLight()`, after light creation or lookup:

```csharp
MobileVisualStyle.ApplyLight(light);
```

- [ ] **Step 3: Upgrade primitive colors to readable toy-like factions**

Change prototype material colors where objects are created:

```csharp
Paint(go, MobileVisualStyle.EnemyRed);
Paint(go, MobileVisualStyle.FriendlyBlue);
Paint(go, MobileVisualStyle.StoneWarm);
```

Use warm, saturated, readable colors. Avoid muddy dark palettes. The target is stylized clarity: chunky buildings, bright faction identity, readable silhouettes, and rewarding impact feedback.

- [ ] **Step 4: Visual acceptance pass**

In Unity Play mode, check:

- Camera shows both lanes and main objective without excessive UI overlap.
- Friendly and enemy objects are readable at mobile scale.
- Gold/resource feedback uses warm accent color.
- Top and bottom UI do not cover the main target.

- [ ] **Step 5: Commit**

```powershell
git add Assets/_Game/Scripts/Visuals/MobileVisualStyle.cs Assets/_Game/Scripts/Testing/TestBootstrap.cs Assets/_Game/Materials
git commit -m "style: add mobile visual quality foundation"
```

---

## Task 9: Documentation, Build, and Portable Placement

**Files:**
- Modify: `docs/MadDragon_기획서.md`
- Modify: `docs/MadDragon_기획서.html`
- Generated build target: `C:/Development/15_MD/release/MadDragon.exe`
- Portable copy target: `C:/Development/15_MD/MadDragon_v0.5_portable.exe`

- [ ] **Step 1: Update planning docs with implemented status**

Update `docs/MadDragon_기획서.md` and `docs/MadDragon_기획서.html` with:

```text
v0.5 implementation status:
- Resource production and storage implemented.
- Raid loss forecast implemented.
- Mobile-first prototype screens introduced.
- Clash-like visual direction started through camera, lighting, faction colors, and UI scale.
```

- [ ] **Step 2: Run all EditMode tests**

Run:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -quit -projectPath 'C:/Development/15_MD' -runTests -testPlatform EditMode -testResults 'C:/Development/15_MD/Logs/EditModeResults.xml' -logFile 'C:/Development/15_MD/Logs/EditModeTests.log'
```

Expected: EditMode test run completes with all tests passing.

- [ ] **Step 3: Build Windows player**

Close any open Unity Editor instance for `C:/Development/15_MD`, then run:

```powershell
New-Item -ItemType Directory -Force -Path 'C:/Development/15_MD/release' | Out-Null
& 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -quit -projectPath 'C:/Development/15_MD' -buildWindows64Player 'C:/Development/15_MD/release/MadDragon.exe' -logFile 'C:/Development/15_MD/Logs/CodexBuild.log'
```

Expected: `C:/Development/15_MD/release/MadDragon.exe` exists.

- [ ] **Step 4: Place portable executable**

Run:

```powershell
Copy-Item -LiteralPath 'C:/Development/15_MD/release/MadDragon.exe' -Destination 'C:/Development/15_MD/MadDragon_v0.5_portable.exe' -Force
Get-Item -LiteralPath 'C:/Development/15_MD/MadDragon_v0.5_portable.exe' | Select-Object FullName,Length,LastWriteTime
```

Expected: portable executable exists at project root.

- [ ] **Step 5: Commit final implementation docs**

```powershell
git add docs/MadDragon_기획서.md docs/MadDragon_기획서.html
git commit -m "docs: update maddragon implementation status"
```

---

## Self-Review

Spec coverage:

- Attack-defense loop: Tasks 5, 6, 7 connect hub, base, prep, and battle screens.
- Building production and headquarters storage: Task 1 implements production, storage, collection, capacity, and protection.
- Raid loss from uncollected and owned resources: Task 2 implements forecast and application.
- Save persistence: Task 3 adds economy fields to save/load.
- Mobile-first layout: Tasks 4 through 7 add bottom-sheet-oriented reusable UI and compact HUDs.
- Clash-like visual quality: Task 8 adds camera, lighting, faction readability, and toy-like palette direction.
- Docs/build: Task 9 updates planning docs and follows project build/portable placement rules.

Placeholder scan:

- The plan intentionally avoids open placeholders. Numeric rates, file names, type names, and commands are concrete.

Type consistency:

- `ResourceWallet`, `ResourceStorageSystem`, `RaidForecast`, `RaidOutcome`, and `RaidLossCalculator` are introduced before any later task references them.
- UI classes consistently use `MobileUiFactory` and `MobileHudTheme`.
