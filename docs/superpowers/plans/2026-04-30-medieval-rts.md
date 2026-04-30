# Medieval RTS — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mad Rocket: Fog of War 클론을 중세 판타지 테마로 Unity 6에서 구현하며, Android + WebGL 싱글플레이어 캠페인 MVP를 완성한다.

**Architecture:** GameManager 싱글턴이 씬 전환과 상태를 관리하고, 타입 안전 EventBus로 시스템 간 디커플링을 유지한다. 모든 밸런스 데이터는 ScriptableObject로 외부화하고, 기지 레이아웃과 진행도는 Newtonsoft.Json으로 로컬 JSON 저장한다.

**Tech Stack:** Unity 6 LTS, URP, New Input System, C#, Unity Test Framework (NUnit), NavMesh (AI Navigation package), Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`)

---

## File Map

| 파일 | 역할 |
|------|------|
| `Assets/_Game/Scripts/Core/GameState.cs` | 게임 상태 enum + 이벤트 구조체 |
| `Assets/_Game/Scripts/Core/EventBus.cs` | 타입 안전 정적 이벤트 버스 |
| `Assets/_Game/Scripts/Core/GameManager.cs` | 씬 전환·상태 관리 싱글턴 |
| `Assets/_Game/Scripts/Data/UnitData.cs` | 유닛 밸런스 ScriptableObject |
| `Assets/_Game/Scripts/Data/BuildingData.cs` | 건물 밸런스 ScriptableObject |
| `Assets/_Game/Scripts/Data/StageData.cs` | AI 기지 레이아웃 + 웨이브 ScriptableObject |
| `Assets/_Game/Scripts/Grid/GridSystem.cs` | 그리드 배치 로직 (pure C#, 테스트 가능) |
| `Assets/_Game/Scripts/Grid/GridVisualizer.cs` | 그리드 시각화 MonoBehaviour |
| `Assets/_Game/Scripts/Base/BaseLayoutData.cs` | 기지 레이아웃 직렬화 모델 |
| `Assets/_Game/Scripts/Base/BaseBuilderManager.cs` | BaseBuilder 씬 컨트롤러 |
| `Assets/_Game/Scripts/Base/BuildingPlacer.cs` | 건물 배치 인풋 처리 |
| `Assets/_Game/Scripts/Base/BaseBuilderUI.cs` | BaseBuilder HUD |
| `Assets/_Game/Scripts/Units/Unit.cs` | 유닛 MonoBehaviour (체력·사망) |
| `Assets/_Game/Scripts/Units/UnitAI.cs` | NavMesh 이동 + 타겟팅·공격 |
| `Assets/_Game/Scripts/Units/UnitSpawner.cs` | 유닛 소환 + 골드 차감 |
| `Assets/_Game/Scripts/Units/FogRevealAgent.cs` | 유닛 시야 반경 Fog 제거 |
| `Assets/_Game/Scripts/Buildings/Building.cs` | 건물 MonoBehaviour (체력·사망) |
| `Assets/_Game/Scripts/Buildings/DefenseTower.cs` | 타워 자동 공격 AI |
| `Assets/_Game/Scripts/Battle/BattleManager.cs` | 전투 시작·종료·승패 판정 |
| `Assets/_Game/Scripts/Battle/ResourceManager.cs` | 골드 자동 충전 |
| `Assets/_Game/Scripts/Battle/SpawnZone.cs` | 유닛 배치 존 |
| `Assets/_Game/Scripts/Battle/AIWaveSpawner.cs` | AI 웨이브 유닛 투입 |
| `Assets/_Game/Scripts/Campaign/CampaignManager.cs` | 스테이지 선택·진행도 |
| `Assets/_Game/Scripts/Campaign/StarRatingSystem.cs` | 별점 계산 (pure C#, 테스트 가능) |
| `Assets/_Game/Scripts/Progression/SaveData.cs` | 저장 데이터 모델 |
| `Assets/_Game/Scripts/Progression/SaveSystem.cs` | JSON 저장·로드 (pure C#, 테스트 가능) |
| `Assets/_Game/Scripts/UI/BattleHUD.cs` | 전투 HUD (골드·타이머·유닛 버튼) |
| `Assets/_Game/Scripts/UI/ResultUI.cs` | 결과 화면 (별점·버튼) |
| `Assets/_Game/Tests/EditMode/GridSystemTests.cs` | GridSystem 단위 테스트 |
| `Assets/_Game/Tests/EditMode/SaveSystemTests.cs` | SaveSystem 단위 테스트 |
| `Assets/_Game/Tests/EditMode/StarRatingTests.cs` | 별점 계산 단위 테스트 |

---

## Task 1: Unity 프로젝트 생성 (수동 작업)

**Files:**
- Create: `Assets/_Game/` (Unity 에디터에서 생성)

> ⚠️ 이 태스크는 Unity Hub GUI에서 수행한다.

- [ ] **Step 1: Unity 6 설치**

  Unity Hub 열기 → Installs → Install Editor → Unity 6 (6000.x.x LTS) 선택
  Add modules에서 체크:
  - Android Build Support (+ Android SDK & NDK, OpenJDK)
  - WebGL Build Support

- [ ] **Step 2: 프로젝트 생성**

  Unity Hub → New project → 3D (URP) 템플릿 선택
  Location: `C:\Development`
  Project name: `15_MD`
  → Create project 클릭

- [ ] **Step 3: 불필요한 샘플 씬 삭제**

  Project 창에서 `Assets/Scenes/SampleScene.unity` 삭제

- [ ] **Step 4: 폴더 구조 생성**

  Project 창 → Assets 우클릭 → Create Folder로 아래 구조 생성:
  ```
  Assets/
  └── _Game/
      ├── Scenes/
      ├── Scripts/
      │   ├── Core/
      │   ├── Data/
      │   ├── Grid/
      │   ├── Base/
      │   ├── Units/
      │   ├── Buildings/
      │   ├── Battle/
      │   ├── Campaign/
      │   ├── Progression/
      │   └── UI/
      ├── ScriptableObjects/
      │   ├── Units/
      │   ├── Buildings/
      │   └── Stages/
      └── Tests/
          └── EditMode/
  ```

- [ ] **Step 5: Newtonsoft.Json 패키지 추가**

  Window → Package Manager → + → Add package by name:
  ```
  com.unity.nuget.newtonsoft-json
  ```

- [ ] **Step 6: AI Navigation 패키지 추가**

  Window → Package Manager → + → Add package by name:
  ```
  com.unity.ai.navigation
  ```

- [ ] **Step 7: 씬 4개 생성**

  File → New Scene → Basic (URP) → 저장:
  - `Assets/_Game/Scenes/MainMenu.unity`
  - `Assets/_Game/Scenes/BaseBuilder.unity`
  - `Assets/_Game/Scenes/Battle.unity`
  - `Assets/_Game/Scenes/Result.unity`

  File → Build Settings → 씬 4개 모두 추가 (순서: MainMenu=0, BaseBuilder=1, Battle=2, Result=3)

- [ ] **Step 8: git에 커밋**

  ```bash
  cd "C:/Development/15_MD"
  git add -A
  git commit -m "feat: initialize Unity 6 URP project with folder structure"
  ```

---

## Task 2: Assembly Definitions 설정

**Files:**
- Create: `Assets/_Game/Scripts/MedievalRTS.Runtime.asmdef`
- Create: `Assets/_Game/Tests/EditMode/MedievalRTS.EditModeTests.asmdef`

- [ ] **Step 1: Runtime Assembly Definition 생성**

  Project 창에서 `Assets/_Game/Scripts` 우클릭 → Create → Assembly Definition
  이름: `MedievalRTS.Runtime`

  Inspector에서 설정:
  - Allow 'unsafe' code: 체크 해제
  - Auto Referenced: 체크
  - Platforms: Any Platform

- [ ] **Step 2: EditMode Test Assembly Definition 생성**

  `Assets/_Game/Tests/EditMode` 우클릭 → Create → Assembly Definition
  이름: `MedievalRTS.EditModeTests`

  Inspector에서 설정:
  - Assembly References에 추가:
    - `MedievalRTS.Runtime`
    - `UnityEngine.TestRunner`
    - `UnityEditor.TestRunner`
  - Test Assemblies: `Edit Mode` 체크, `Play Mode` 체크 해제

- [ ] **Step 3: 커밋**

  ```bash
  git add Assets/_Game/Scripts/MedievalRTS.Runtime.asmdef* \
          Assets/_Game/Tests/EditMode/MedievalRTS.EditModeTests.asmdef*
  git commit -m "feat: add assembly definitions for runtime and test"
  ```

---

## Task 3: Core — GameState, Events, EventBus

**Files:**
- Create: `Assets/_Game/Scripts/Core/GameState.cs`
- Create: `Assets/_Game/Scripts/Core/EventBus.cs`

- [ ] **Step 1: `GameState.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Core/GameState.cs
  namespace MedievalRTS.Core
  {
      public enum GameState { MainMenu, BaseBuilder, Battle, Result }

      public readonly struct GameStateChangedEvent
      {
          public readonly GameState State;
          public GameStateChangedEvent(GameState state) => State = state;
      }

      public readonly struct GoldChangedEvent
      {
          public readonly int Amount;
          public GoldChangedEvent(int amount) => Amount = amount;
      }

      public readonly struct BattleEndedEvent
      {
          public readonly bool Victory;
          public readonly int Stars;
          public readonly BattleStats Stats;
          public BattleEndedEvent(bool victory, int stars, BattleStats stats)
          {
              Victory = victory;
              Stars = stars;
              Stats = stats;
          }
      }

      public struct BattleStats
      {
          public bool Victory;
          public int UnitsLost;
          public float TimeElapsed;
      }

      public readonly struct UnitDiedEvent
      {
          public readonly Units.Unit Unit;
          public UnitDiedEvent(Units.Unit unit) => Unit = unit;
      }

      public readonly struct BuildingDestroyedEvent
      {
          public readonly Buildings.Building Building;
          public BuildingDestroyedEvent(Buildings.Building building) => Building = building;
      }
  }
  ```

- [ ] **Step 2: `EventBus.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Core/EventBus.cs
  using System;
  using System.Collections.Generic;

  namespace MedievalRTS.Core
  {
      public static class EventBus
      {
          private static readonly Dictionary<Type, Delegate> _handlers = new();

          public static void Subscribe<T>(Action<T> handler)
          {
              var type = typeof(T);
              if (_handlers.ContainsKey(type))
                  _handlers[type] = Delegate.Combine(_handlers[type], handler);
              else
                  _handlers[type] = handler;
          }

          public static void Unsubscribe<T>(Action<T> handler)
          {
              var type = typeof(T);
              if (!_handlers.TryGetValue(type, out var existing)) return;
              var updated = Delegate.Remove(existing, handler);
              if (updated == null) _handlers.Remove(type);
              else _handlers[type] = updated;
          }

          public static void Publish<T>(T evt)
          {
              if (_handlers.TryGetValue(typeof(T), out var handler))
                  ((Action<T>)handler).Invoke(evt);
          }

          /// <summary>테스트 후 정리용</summary>
          public static void Clear() => _handlers.Clear();
      }
  }
  ```

- [ ] **Step 3: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Core/
  git commit -m "feat: add GameState events and EventBus"
  ```

---

## Task 4: Core — GameManager

**Files:**
- Create: `Assets/_Game/Scripts/Core/GameManager.cs`

- [ ] **Step 1: `GameManager.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Core/GameManager.cs
  using UnityEngine;
  using UnityEngine.SceneManagement;

  namespace MedievalRTS.Core
  {
      public class GameManager : MonoBehaviour
      {
          public static GameManager Instance { get; private set; }
          public GameState CurrentState { get; private set; }

          private void Awake()
          {
              if (Instance != null) { Destroy(gameObject); return; }
              Instance = this;
              DontDestroyOnLoad(gameObject);
          }

          public void ChangeState(GameState newState)
          {
              CurrentState = newState;
              EventBus.Publish(new GameStateChangedEvent(newState));
              switch (newState)
              {
                  case GameState.MainMenu:    SceneManager.LoadScene("MainMenu");    break;
                  case GameState.BaseBuilder: SceneManager.LoadScene("BaseBuilder"); break;
                  case GameState.Battle:      SceneManager.LoadScene("Battle");      break;
                  case GameState.Result:      SceneManager.LoadScene("Result");      break;
              }
          }
      }
  }
  ```

- [ ] **Step 2: MainMenu 씬에 GameManager 배치 (수동)**

  MainMenu.unity 씬 열기 → Hierarchy에서 빈 GameObject 생성 → 이름 `GameManager` → `GameManager` 컴포넌트 추가

- [ ] **Step 3: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Core/GameManager.cs
  git commit -m "feat: add GameManager singleton with scene management"
  ```

---

## Task 5: ScriptableObjects — UnitData, BuildingData, StageData

**Files:**
- Create: `Assets/_Game/Scripts/Data/UnitData.cs`
- Create: `Assets/_Game/Scripts/Data/BuildingData.cs`
- Create: `Assets/_Game/Scripts/Data/StageData.cs`

- [ ] **Step 1: `UnitData.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Data/UnitData.cs
  using UnityEngine;

  namespace MedievalRTS.Data
  {
      public enum UnitType { Knight, Archer, Catapult, Scout, Mage }

      [CreateAssetMenu(fileName = "UnitData", menuName = "Medieval RTS/Unit Data")]
      public class UnitData : ScriptableObject
      {
          public string unitName;
          public UnitType unitType;
          public int maxHp;
          public int damage;
          public float moveSpeed;
          public int goldCost;
          public float attackRange;
          public float attackCooldown;
          public float sightRange;
          public GameObject prefab;
          public Sprite icon;
      }
  }
  ```

- [ ] **Step 2: `BuildingData.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Data/BuildingData.cs
  using UnityEngine;

  namespace MedievalRTS.Data
  {
      public enum BuildingType { Castle, ArcherTower, CannonTower, Wall, Barracks, GoldMine }

      [CreateAssetMenu(fileName = "BuildingData", menuName = "Medieval RTS/Building Data")]
      public class BuildingData : ScriptableObject
      {
          public string buildingName;
          public BuildingType buildingType;
          public int maxHp;
          public Vector2Int gridSize;
          public int goldCost;
          public bool isDefenseTower;
          public float attackRange;
          public int attackDamage;
          public float attackCooldown;
          public int goldProductionPerSecond; // GoldMine 전용
          public GameObject prefab;
          public Sprite icon;
      }
  }
  ```

- [ ] **Step 3: `StageData.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Data/StageData.cs
  using System;
  using UnityEngine;

  namespace MedievalRTS.Data
  {
      [Serializable]
      public struct BuildingPlacement
      {
          public BuildingData data;
          public Vector2Int gridPosition;
      }

      [Serializable]
      public struct WaveData
      {
          public UnitData[] units;
          public float spawnInterval;
          public float waveStartTime;
      }

      [CreateAssetMenu(fileName = "StageData", menuName = "Medieval RTS/Stage Data")]
      public class StageData : ScriptableObject
      {
          public string stageName;
          public int stageIndex;
          public float battleDuration;           // 제한 시간(초)
          public int unlockRequirementStars;     // 언락에 필요한 누적 별
          public BuildingPlacement[] enemyBase;  // AI 기지 레이아웃
          public WaveData[] waves;               // AI 공격 웨이브
      }
  }
  ```

- [ ] **Step 4: ScriptableObject 에셋 생성 (수동)**

  Project 창 → `Assets/_Game/ScriptableObjects/Units` 우클릭 → Create → Medieval RTS → Unit Data로 아래 5개 생성:

  | Asset 이름 | unitName | unitType | maxHp | damage | moveSpeed | goldCost | attackRange | attackCooldown | sightRange |
  |-----------|----------|----------|-------|--------|-----------|----------|-------------|----------------|------------|
  | Knight    | Knight   | Knight   | 200   | 20     | 3.0       | 50       | 1.5         | 1.0            | 5.0        |
  | Archer    | Archer   | Archer   | 80    | 15     | 3.5       | 40       | 8.0         | 1.2            | 8.0        |
  | Catapult  | Catapult | Catapult | 120   | 50     | 1.5       | 80       | 6.0         | 3.0            | 5.0        |
  | Scout     | Scout    | Scout    | 60    | 5      | 6.0       | 30       | 2.0         | 1.0            | 15.0       |
  | Mage      | Mage     | Mage     | 100   | 40     | 2.5       | 100      | 5.0         | 2.0            | 6.0        |

  `Assets/_Game/ScriptableObjects/Buildings`에 아래 6개 생성:

  | Asset 이름  | buildingName | buildingType | maxHp | gridSize | isDefenseTower | attackRange | attackDamage | attackCooldown | goldProductionPerSecond |
  |------------|--------------|--------------|-------|----------|----------------|-------------|--------------|----------------|------------------------|
  | Castle     | Castle       | Castle       | 2000  | 2x2      | false          | 0           | 0            | 0              | 0                      |
  | ArcherTower| Archer Tower | ArcherTower  | 500   | 1x1      | true           | 8.0         | 12           | 1.0            | 0                      |
  | CannonTower| Cannon Tower | CannonTower  | 800   | 2x2      | true           | 6.0         | 40           | 3.0            | 0                      |
  | Wall       | Wall         | Wall         | 300   | 1x1      | false          | 0           | 0            | 0              | 0                      |
  | Barracks   | Barracks     | Barracks     | 400   | 2x2      | false          | 0           | 0            | 0              | 0                      |
  | GoldMine   | Gold Mine    | GoldMine     | 300   | 1x1      | false          | 0           | 0            | 0              | 2                      |

- [ ] **Step 5: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Data/ Assets/_Game/ScriptableObjects/
  git commit -m "feat: add UnitData, BuildingData, StageData ScriptableObjects with initial assets"
  ```

---

## Task 6: GridSystem (TDD)

**Files:**
- Create: `Assets/_Game/Scripts/Grid/GridSystem.cs`
- Create: `Assets/_Game/Tests/EditMode/GridSystemTests.cs`

- [ ] **Step 1: 실패하는 테스트 작성**

  ```csharp
  // Assets/_Game/Tests/EditMode/GridSystemTests.cs
  using NUnit.Framework;
  using MedievalRTS.Grid;
  using UnityEngine;

  public class GridSystemTests
  {
      private GridSystem _grid;

      [SetUp]
      public void SetUp()
      {
          _grid = new GridSystem(10, 10);
      }

      [Test]
      public void CanPlace_EmptyGrid_1x1_ReturnsTrue()
      {
          Assert.IsTrue(_grid.CanPlace(0, 0, new Vector2Int(1, 1)));
      }

      [Test]
      public void CanPlace_OutOfBounds_ReturnsFalse()
      {
          Assert.IsFalse(_grid.CanPlace(9, 9, new Vector2Int(2, 2)));
      }

      [Test]
      public void CanPlace_NegativeCoord_ReturnsFalse()
      {
          Assert.IsFalse(_grid.CanPlace(-1, 0, new Vector2Int(1, 1)));
      }

      [Test]
      public void Place_Then_CanPlace_SameCell_ReturnsFalse()
      {
          _grid.Place(0, 0, new Vector2Int(2, 2));
          Assert.IsFalse(_grid.CanPlace(0, 0, new Vector2Int(1, 1)));
          Assert.IsFalse(_grid.CanPlace(1, 1, new Vector2Int(1, 1)));
      }

      [Test]
      public void Remove_AfterPlace_CellBecomesAvailable()
      {
          _grid.Place(3, 3, new Vector2Int(2, 2));
          _grid.Remove(3, 3, new Vector2Int(2, 2));
          Assert.IsTrue(_grid.CanPlace(3, 3, new Vector2Int(2, 2)));
      }

      [Test]
      public void CanPlace_AdjacentToPlaced_ReturnsTrue()
      {
          _grid.Place(0, 0, new Vector2Int(2, 2));
          Assert.IsTrue(_grid.CanPlace(2, 0, new Vector2Int(1, 1)));
      }

      [Test]
      public void IsOccupied_AfterPlace_ReturnsTrue()
      {
          _grid.Place(5, 5, new Vector2Int(1, 1));
          Assert.IsTrue(_grid.IsOccupied(5, 5));
      }
  }
  ```

- [ ] **Step 2: 테스트 실행 — FAIL 확인**

  Unity 에디터 → Window → General → Test Runner → EditMode → Run All
  Expected: `GridSystem` 클래스를 찾을 수 없어 컴파일 에러

- [ ] **Step 3: `GridSystem.cs` 구현**

  ```csharp
  // Assets/_Game/Scripts/Grid/GridSystem.cs
  using UnityEngine;

  namespace MedievalRTS.Grid
  {
      public class GridSystem
      {
          private readonly int _width;
          private readonly int _height;
          private readonly bool[,] _occupied;

          public GridSystem(int width, int height)
          {
              _width = width;
              _height = height;
              _occupied = new bool[width, height];
          }

          public bool CanPlace(int x, int y, Vector2Int size)
          {
              for (int dx = 0; dx < size.x; dx++)
              for (int dy = 0; dy < size.y; dy++)
              {
                  int cx = x + dx, cy = y + dy;
                  if (cx < 0 || cx >= _width || cy < 0 || cy >= _height) return false;
                  if (_occupied[cx, cy]) return false;
              }
              return true;
          }

          public void Place(int x, int y, Vector2Int size)
          {
              for (int dx = 0; dx < size.x; dx++)
              for (int dy = 0; dy < size.y; dy++)
                  _occupied[x + dx, y + dy] = true;
          }

          public void Remove(int x, int y, Vector2Int size)
          {
              for (int dx = 0; dx < size.x; dx++)
              for (int dy = 0; dy < size.y; dy++)
                  _occupied[x + dx, y + dy] = false;
          }

          public bool IsOccupied(int x, int y) => _occupied[x, y];
      }
  }
  ```

- [ ] **Step 4: 테스트 실행 — PASS 확인**

  Test Runner → Run All
  Expected: 7개 테스트 모두 PASS (녹색)

- [ ] **Step 5: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Grid/GridSystem.cs \
          Assets/_Game/Tests/EditMode/GridSystemTests.cs
  git commit -m "feat: add GridSystem with full test coverage"
  ```

---

## Task 7: SaveSystem (TDD)

**Files:**
- Create: `Assets/_Game/Scripts/Progression/SaveData.cs`
- Create: `Assets/_Game/Scripts/Progression/SaveSystem.cs`
- Create: `Assets/_Game/Tests/EditMode/SaveSystemTests.cs`

- [ ] **Step 1: 실패하는 테스트 작성**

  ```csharp
  // Assets/_Game/Tests/EditMode/SaveSystemTests.cs
  using NUnit.Framework;
  using System.IO;
  using MedievalRTS.Progression;
  using MedievalRTS.Base;

  public class SaveSystemTests
  {
      private string _testPath;

      [SetUp]
      public void SetUp()
      {
          _testPath = Path.Combine(Path.GetTempPath(), "medieval_rts_test_save.json");
          if (File.Exists(_testPath)) File.Delete(_testPath);
      }

      [TearDown]
      public void TearDown()
      {
          if (File.Exists(_testPath)) File.Delete(_testPath);
      }

      [Test]
      public void Load_NoFile_ReturnsNewSaveData()
      {
          var data = SaveSystem.Load(_testPath);
          Assert.IsNotNull(data);
          Assert.IsNotNull(data.StageStars);
          Assert.IsNotNull(data.UnitLevels);
      }

      [Test]
      public void Save_Then_Load_PreservesStageStars()
      {
          var data = new SaveData();
          data.StageStars[0] = 3;
          data.StageStars[2] = 1;

          SaveSystem.Save(data, _testPath);
          var loaded = SaveSystem.Load(_testPath);

          Assert.AreEqual(3, loaded.StageStars[0]);
          Assert.AreEqual(1, loaded.StageStars[2]);
      }

      [Test]
      public void Save_Then_Load_PreservesUnitLevels()
      {
          var data = new SaveData();
          data.UnitLevels["Knight"] = 2;
          data.UnitLevels["Archer"] = 3;

          SaveSystem.Save(data, _testPath);
          var loaded = SaveSystem.Load(_testPath);

          Assert.AreEqual(2, loaded.UnitLevels["Knight"]);
          Assert.AreEqual(3, loaded.UnitLevels["Archer"]);
      }

      [Test]
      public void Save_Then_Load_PreservesPlayerBase()
      {
          var data = new SaveData();
          data.PlayerBase = new BaseLayoutData();
          data.PlayerBase.Buildings.Add(new PlacedBuilding
          {
              BuildingName = "Castle",
              GridX = 5,
              GridY = 5
          });

          SaveSystem.Save(data, _testPath);
          var loaded = SaveSystem.Load(_testPath);

          Assert.AreEqual(1, loaded.PlayerBase.Buildings.Count);
          Assert.AreEqual("Castle", loaded.PlayerBase.Buildings[0].BuildingName);
          Assert.AreEqual(5, loaded.PlayerBase.Buildings[0].GridX);
      }

      [Test]
      public void Delete_RemovesFile()
      {
          SaveSystem.Save(new SaveData(), _testPath);
          SaveSystem.Delete(_testPath);
          Assert.IsFalse(File.Exists(_testPath));
      }
  }
  ```

- [ ] **Step 2: 테스트 실행 — FAIL 확인 (컴파일 에러 예상)**

- [ ] **Step 3: `SaveData.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Progression/SaveData.cs
  using System.Collections.Generic;
  using MedievalRTS.Base;

  namespace MedievalRTS.Progression
  {
      public class SaveData
      {
          public Dictionary<int, int> StageStars { get; set; } = new();
          public Dictionary<string, int> UnitLevels { get; set; } = new();
          public BaseLayoutData PlayerBase { get; set; }
      }
  }
  ```

- [ ] **Step 4: `BaseLayoutData.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Base/BaseLayoutData.cs
  using System;
  using System.Collections.Generic;

  namespace MedievalRTS.Base
  {
      [Serializable]
      public class PlacedBuilding
      {
          public string BuildingName;
          public int GridX;
          public int GridY;
      }

      [Serializable]
      public class BaseLayoutData
      {
          public List<PlacedBuilding> Buildings { get; set; } = new();
      }
  }
  ```

- [ ] **Step 5: `SaveSystem.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Progression/SaveSystem.cs
  using System.IO;
  using Newtonsoft.Json;
  using UnityEngine;

  namespace MedievalRTS.Progression
  {
      public static class SaveSystem
      {
          private static string DefaultPath =>
              Path.Combine(Application.persistentDataPath, "save.json");

          public static void Save(SaveData data, string path = null)
          {
              path ??= DefaultPath;
              File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
          }

          public static SaveData Load(string path = null)
          {
              path ??= DefaultPath;
              if (!File.Exists(path)) return new SaveData();
              return JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path))
                     ?? new SaveData();
          }

          public static void Delete(string path = null)
          {
              path ??= DefaultPath;
              if (File.Exists(path)) File.Delete(path);
          }
      }
  }
  ```

- [ ] **Step 6: 테스트 실행 — PASS 확인**

  Test Runner → Run All → 5개 테스트 PASS

- [ ] **Step 7: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Progression/ \
          Assets/_Game/Scripts/Base/BaseLayoutData.cs \
          Assets/_Game/Tests/EditMode/SaveSystemTests.cs
  git commit -m "feat: add SaveSystem and BaseLayoutData with test coverage"
  ```

---

## Task 8: StarRatingSystem (TDD)

**Files:**
- Create: `Assets/_Game/Scripts/Campaign/StarRatingSystem.cs`
- Create: `Assets/_Game/Tests/EditMode/StarRatingTests.cs`

- [ ] **Step 1: 실패하는 테스트 작성**

  ```csharp
  // Assets/_Game/Tests/EditMode/StarRatingTests.cs
  using NUnit.Framework;
  using MedievalRTS.Campaign;
  using MedievalRTS.Core;
  using MedievalRTS.Data;
  using UnityEngine;

  public class StarRatingTests
  {
      private StageData _stage;

      [SetUp]
      public void SetUp()
      {
          _stage = ScriptableObject.CreateInstance<StageData>();
          _stage.battleDuration = 120f;
      }

      [TearDown]
      public void TearDown()
      {
          Object.DestroyImmediate(_stage);
      }

      [Test]
      public void Calculate_Defeat_Returns0()
      {
          var stats = new BattleStats { Victory = false };
          Assert.AreEqual(0, StarRatingSystem.Calculate(stats, _stage));
      }

      [Test]
      public void Calculate_Victory_WithLosses_SlowClear_Returns1()
      {
          var stats = new BattleStats { Victory = true, UnitsLost = 3, TimeElapsed = 110f };
          Assert.AreEqual(1, StarRatingSystem.Calculate(stats, _stage));
      }

      [Test]
      public void Calculate_Victory_NoLosses_SlowClear_Returns2()
      {
          var stats = new BattleStats { Victory = true, UnitsLost = 0, TimeElapsed = 110f };
          Assert.AreEqual(2, StarRatingSystem.Calculate(stats, _stage));
      }

      [Test]
      public void Calculate_Victory_NoLosses_FastClear_Returns3()
      {
          // 120 * 0.7 = 84초 이내
          var stats = new BattleStats { Victory = true, UnitsLost = 0, TimeElapsed = 80f };
          Assert.AreEqual(3, StarRatingSystem.Calculate(stats, _stage));
      }

      [Test]
      public void Calculate_Victory_WithLosses_FastClear_Returns2()
      {
          // 빠르지만 손실 있음 → 별 1(승리) + 별 1(빠름) = 2
          var stats = new BattleStats { Victory = true, UnitsLost = 2, TimeElapsed = 80f };
          Assert.AreEqual(2, StarRatingSystem.Calculate(stats, _stage));
      }
  }
  ```

- [ ] **Step 2: 테스트 실행 — FAIL 확인**

- [ ] **Step 3: `StarRatingSystem.cs` 구현**

  ```csharp
  // Assets/_Game/Scripts/Campaign/StarRatingSystem.cs
  using MedievalRTS.Core;
  using MedievalRTS.Data;
  using UnityEngine;

  namespace MedievalRTS.Campaign
  {
      public static class StarRatingSystem
      {
          private const float FastClearThreshold = 0.7f;

          public static int Calculate(BattleStats stats, StageData stage)
          {
              if (!stats.Victory) return 0;

              int stars = 1; // 승리 기본 1개

              if (stats.UnitsLost == 0) stars++;

              if (stats.TimeElapsed <= stage.battleDuration * FastClearThreshold) stars++;

              return Mathf.Clamp(stars, 0, 3);
          }
      }
  }
  ```

- [ ] **Step 4: 테스트 실행 — PASS 확인**

  Test Runner → Run All → 5개 테스트 PASS

- [ ] **Step 5: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Campaign/StarRatingSystem.cs \
          Assets/_Game/Tests/EditMode/StarRatingTests.cs
  git commit -m "feat: add StarRatingSystem with full test coverage"
  ```

---

## Task 9: Building MonoBehaviour

**Files:**
- Create: `Assets/_Game/Scripts/Buildings/Building.cs`
- Create: `Assets/_Game/Scripts/Buildings/DefenseTower.cs`

- [ ] **Step 1: `Building.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Buildings/Building.cs
  using System;
  using UnityEngine;
  using MedievalRTS.Core;
  using MedievalRTS.Data;

  namespace MedievalRTS.Buildings
  {
      public class Building : MonoBehaviour
      {
          public BuildingData Data { get; private set; }
          public int CurrentHp { get; private set; }
          public bool IsAlive => CurrentHp > 0;
          public bool IsPlayerBuilding { get; private set; }

          public event Action<Building> OnDestroyed;

          public void Initialize(BuildingData data, bool isPlayerBuilding)
          {
              Data = data;
              CurrentHp = data.maxHp;
              IsPlayerBuilding = isPlayerBuilding;
              gameObject.tag = isPlayerBuilding ? "PlayerBuilding" : "EnemyBuilding";
          }

          public void TakeDamage(int amount)
          {
              if (!IsAlive) return;
              CurrentHp = Mathf.Max(0, CurrentHp - amount);
              if (CurrentHp == 0) DestroyBuilding();
          }

          private void DestroyBuilding()
          {
              OnDestroyed?.Invoke(this);
              EventBus.Publish(new BuildingDestroyedEvent(this));
              Destroy(gameObject, 0.3f);
          }
      }
  }
  ```

- [ ] **Step 2: `DefenseTower.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Buildings/DefenseTower.cs
  using UnityEngine;
  using MedievalRTS.Units;

  namespace MedievalRTS.Buildings
  {
      [RequireComponent(typeof(Building))]
      public class DefenseTower : MonoBehaviour
      {
          private Building _building;
          private float _attackTimer;

          private void Awake() => _building = GetComponent<Building>();

          private void Update()
          {
              if (!_building.IsAlive || !_building.Data.isDefenseTower) return;

              _attackTimer -= Time.deltaTime;
              if (_attackTimer > 0f) return;

              var target = FindNearestEnemyUnit();
              if (target == null) return;

              target.TakeDamage(_building.Data.attackDamage);
              _attackTimer = _building.Data.attackCooldown;
          }

          private Unit FindNearestEnemyUnit()
          {
              string enemyTag = _building.IsPlayerBuilding ? "EnemyUnit" : "PlayerUnit";
              var candidates = GameObject.FindGameObjectsWithTag(enemyTag);
              float nearest = float.MaxValue;
              Unit result = null;

              foreach (var c in candidates)
              {
                  var unit = c.GetComponent<Unit>();
                  if (unit == null || !unit.IsAlive) continue;
                  float d = Vector3.Distance(transform.position, c.transform.position);
                  if (d <= _building.Data.attackRange && d < nearest)
                  {
                      nearest = d;
                      result = unit;
                  }
              }
              return result;
          }
      }
  }
  ```

- [ ] **Step 3: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Buildings/
  git commit -m "feat: add Building and DefenseTower MonoBehaviours"
  ```

---

## Task 10: Unit MonoBehaviour + UnitAI

**Files:**
- Create: `Assets/_Game/Scripts/Units/Unit.cs`
- Create: `Assets/_Game/Scripts/Units/UnitAI.cs`
- Create: `Assets/_Game/Scripts/Units/FogRevealAgent.cs`

- [ ] **Step 1: `Unit.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Units/Unit.cs
  using System;
  using UnityEngine;
  using MedievalRTS.Core;
  using MedievalRTS.Data;

  namespace MedievalRTS.Units
  {
      public class Unit : MonoBehaviour
      {
          public UnitData Data { get; private set; }
          public int CurrentHp { get; private set; }
          public bool IsAlive => CurrentHp > 0;
          public bool IsPlayerUnit { get; private set; }

          public event Action<Unit> OnDied;

          public void Initialize(UnitData data, bool isPlayerUnit)
          {
              Data = data;
              CurrentHp = data.maxHp;
              IsPlayerUnit = isPlayerUnit;
              gameObject.tag = isPlayerUnit ? "PlayerUnit" : "EnemyUnit";
          }

          public void TakeDamage(int amount)
          {
              if (!IsAlive) return;
              CurrentHp = Mathf.Max(0, CurrentHp - amount);
              if (CurrentHp == 0) Die();
          }

          private void Die()
          {
              OnDied?.Invoke(this);
              EventBus.Publish(new UnitDiedEvent(this));
              Destroy(gameObject, 0.3f);
          }
      }
  }
  ```

- [ ] **Step 2: `UnitAI.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Units/UnitAI.cs
  using UnityEngine;
  using UnityEngine.AI;
  using MedievalRTS.Buildings;

  namespace MedievalRTS.Units
  {
      [RequireComponent(typeof(NavMeshAgent), typeof(Unit))]
      public class UnitAI : MonoBehaviour
      {
          private NavMeshAgent _agent;
          private Unit _unit;
          private Transform _target;
          private float _attackTimer;

          private void Awake()
          {
              _agent = GetComponent<NavMeshAgent>();
              _unit = GetComponent<Unit>();
          }

          private void Start()
          {
              _agent.speed = _unit.Data.moveSpeed;
              _agent.stoppingDistance = _unit.Data.attackRange * 0.9f;
          }

          private void Update()
          {
              if (!_unit.IsAlive) return;

              if (_target == null || !_target.gameObject.activeInHierarchy)
                  FindTarget();

              if (_target == null) return;

              float dist = Vector3.Distance(transform.position, _target.position);
              if (dist > _unit.Data.attackRange)
              {
                  _agent.SetDestination(_target.position);
              }
              else
              {
                  _agent.ResetPath();
                  _attackTimer -= Time.deltaTime;
                  if (_attackTimer <= 0f)
                  {
                      Attack();
                      _attackTimer = _unit.Data.attackCooldown;
                  }
              }
          }

          private void FindTarget()
          {
              string enemyUnitTag     = _unit.IsPlayerUnit ? "EnemyUnit"     : "PlayerUnit";
              string enemyBuildingTag = _unit.IsPlayerUnit ? "EnemyBuilding" : "PlayerBuilding";

              float nearest = float.MaxValue;
              _target = null;

              foreach (var tag in new[] { enemyUnitTag, enemyBuildingTag })
              foreach (var go in GameObject.FindGameObjectsWithTag(tag))
              {
                  float d = Vector3.Distance(transform.position, go.transform.position);
                  if (d < nearest) { nearest = d; _target = go.transform; }
              }
          }

          private void Attack()
          {
              if (_target == null) return;

              var targetUnit = _target.GetComponent<Unit>();
              if (targetUnit != null) { targetUnit.TakeDamage(_unit.Data.damage); return; }

              var targetBuilding = _target.GetComponent<Building>();
              targetBuilding?.TakeDamage(_unit.Data.damage);
          }
      }
  }
  ```

- [ ] **Step 3: `FogRevealAgent.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Units/FogRevealAgent.cs
  // 실제 FOW 에셋 API에 맞게 Update() 내부를 교체한다.
  // 에셋 예시: "FOW - Fog of War" (Asset Store)
  using UnityEngine;

  namespace MedievalRTS.Units
  {
      [RequireComponent(typeof(Unit))]
      public class FogRevealAgent : MonoBehaviour
      {
          private Unit _unit;

          private void Awake() => _unit = GetComponent<Unit>();

          private void Update()
          {
              if (!_unit.IsAlive) return;
              // TODO: 구매한 FOW 에셋 API 호출로 교체
              // 예시 (에셋마다 API 다름):
              // FogOfWarManager.Instance.Reveal(transform.position, _unit.Data.sightRange);
          }
      }
  }
  ```

- [ ] **Step 4: 유닛 Prefab에 NavMesh Surface 설정 (수동)**

  Battle.unity 씬 열기 → Hierarchy에서 지형 오브젝트 선택
  → Inspector → Add Component → NavMesh Surface → Bake 클릭

  각 유닛 Prefab에:
  - `Unit` 컴포넌트 추가 (prefab 필드는 비워도 됨)
  - `UnitAI` 컴포넌트 추가 (NavMeshAgent 자동 추가됨)
  - `FogRevealAgent` 컴포넌트 추가

- [ ] **Step 5: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Units/
  git commit -m "feat: add Unit, UnitAI with NavMesh, FogRevealAgent"
  ```

---

## Task 11: BaseBuilder — GridVisualizer + BuildingPlacer + BaseBuilderManager

**Files:**
- Create: `Assets/_Game/Scripts/Grid/GridVisualizer.cs`
- Create: `Assets/_Game/Scripts/Base/BaseBuilderManager.cs`
- Create: `Assets/_Game/Scripts/Base/BuildingPlacer.cs`

- [ ] **Step 1: `GridVisualizer.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Grid/GridVisualizer.cs
  using UnityEngine;

  namespace MedievalRTS.Grid
  {
      public class GridVisualizer : MonoBehaviour
      {
          [SerializeField] private int width = 20;
          [SerializeField] private int height = 20;
          [SerializeField] private float cellSize = 1f;
          [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);

          private void OnDrawGizmos()
          {
              Gizmos.color = gridColor;
              for (int x = 0; x <= width; x++)
                  Gizmos.DrawLine(
                      transform.position + new Vector3(x * cellSize, 0, 0),
                      transform.position + new Vector3(x * cellSize, 0, height * cellSize));
              for (int y = 0; y <= height; y++)
                  Gizmos.DrawLine(
                      transform.position + new Vector3(0, 0, y * cellSize),
                      transform.position + new Vector3(width * cellSize, 0, y * cellSize));
          }
      }
  }
  ```

- [ ] **Step 2: `BaseBuilderManager.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Base/BaseBuilderManager.cs
  using System.Collections.Generic;
  using UnityEngine;
  using MedievalRTS.Grid;
  using MedievalRTS.Data;
  using MedievalRTS.Buildings;
  using MedievalRTS.Progression;

  namespace MedievalRTS.Base
  {
      public class BaseBuilderManager : MonoBehaviour
      {
          [SerializeField] private int gridWidth = 20;
          [SerializeField] private int gridHeight = 20;
          [SerializeField] private float cellSize = 1f;
          [SerializeField] private Vector3 gridOrigin;
          [SerializeField] private BuildingData[] buildingCatalog; // Inspector에서 할당

          public GridSystem GridSystem { get; private set; }

          private BaseLayoutData _layout;
          private SaveData _saveData;
          private Dictionary<string, BuildingData> _catalog;

          private void Awake()
          {
              GridSystem = new GridSystem(gridWidth, gridHeight);
              _layout = new BaseLayoutData();
              _saveData = SaveSystem.Load();

              _catalog = new Dictionary<string, BuildingData>();
              foreach (var b in buildingCatalog)
                  _catalog[b.name] = b;

              if (_saveData.PlayerBase != null)
                  RestoreLayout(_saveData.PlayerBase);
          }

          public Vector2Int WorldToGrid(Vector3 worldPos)
          {
              return new Vector2Int(
                  Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize),
                  Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize));
          }

          public Vector3 GridToWorld(int x, int y)
          {
              return gridOrigin + new Vector3(
                  x * cellSize + cellSize * 0.5f, 0f,
                  y * cellSize + cellSize * 0.5f);
          }

          public bool TryPlaceBuilding(BuildingData data, int x, int y)
          {
              if (!GridSystem.CanPlace(x, y, data.gridSize)) return false;

              GridSystem.Place(x, y, data.gridSize);
              var go = Instantiate(data.prefab, GridToWorld(x, y), Quaternion.identity);
              go.GetComponent<Building>().Initialize(data, isPlayerBuilding: true);

              _layout.Buildings.Add(new PlacedBuilding
              {
                  BuildingName = data.name,
                  GridX = x,
                  GridY = y
              });
              return true;
          }

          public void SaveLayout()
          {
              _saveData.PlayerBase = _layout;
              SaveSystem.Save(_saveData);
          }

          private void RestoreLayout(BaseLayoutData saved)
          {
              foreach (var p in saved.Buildings)
              {
                  if (_catalog.TryGetValue(p.BuildingName, out var data))
                      TryPlaceBuilding(data, p.GridX, p.GridY);
              }
          }
      }
  }
  ```

- [ ] **Step 3: `BuildingPlacer.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Base/BuildingPlacer.cs
  using UnityEngine;
  using MedievalRTS.Data;

  namespace MedievalRTS.Base
  {
      public class BuildingPlacer : MonoBehaviour
      {
          [SerializeField] private BaseBuilderManager _manager;
          [SerializeField] private Camera _cam;
          [SerializeField] private LayerMask _groundLayer;

          private BuildingData _selected;
          private GameObject _ghost;

          public void SelectBuilding(BuildingData data)
          {
              CancelPlacement();
              _selected = data;
              _ghost = Instantiate(data.prefab);
              SetGhostAlpha(0.5f);
          }

          private void Update()
          {
              if (_selected == null) return;

              var ray = _cam.ScreenPointToRay(Input.mousePosition);
              if (!Physics.Raycast(ray, out var hit, 100f, _groundLayer)) return;

              var gridPos = _manager.WorldToGrid(hit.point);
              _ghost.transform.position = _manager.GridToWorld(gridPos.x, gridPos.y);

              bool canPlace = _manager.GridSystem.CanPlace(gridPos.x, gridPos.y, _selected.gridSize);
              SetGhostColor(canPlace ? Color.green : Color.red, 0.5f);

              if (Input.GetMouseButtonDown(0) && canPlace)
              {
                  _manager.TryPlaceBuilding(_selected, gridPos.x, gridPos.y);
                  CancelPlacement();
              }

              if (Input.GetMouseButtonDown(1)) CancelPlacement();
          }

          private void CancelPlacement()
          {
              if (_ghost != null) Destroy(_ghost);
              _ghost = null;
              _selected = null;
          }

          private void SetGhostAlpha(float alpha)
          {
              foreach (var r in _ghost.GetComponentsInChildren<Renderer>())
              {
                  var c = r.material.color;
                  r.material.color = new Color(c.r, c.g, c.b, alpha);
              }
          }

          private void SetGhostColor(Color color, float alpha)
          {
              foreach (var r in _ghost.GetComponentsInChildren<Renderer>())
                  r.material.color = new Color(color.r, color.g, color.b, alpha);
          }
      }
  }
  ```

- [ ] **Step 4: BaseBuilder 씬 설정 (수동)**

  BaseBuilder.unity 씬 열기:
  1. Hierarchy → 빈 GameObject `BaseBuilderManager` 생성 → `BaseBuilderManager` 컴포넌트 추가
     - Grid Width: 20, Grid Height: 20, Cell Size: 1
     - Building Catalog: 6개 BuildingData 에셋 할당
  2. 같은 오브젝트에 `GridVisualizer` 추가 (같은 Width/Height/Cell Size)
  3. 빈 GameObject `BuildingPlacer` 생성 → `BuildingPlacer` 컴포넌트 추가
     - Manager 필드: BaseBuilderManager 연결
     - Cam 필드: Main Camera 연결
     - Ground Layer: 지형 레이어 선택

- [ ] **Step 5: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Grid/GridVisualizer.cs \
          Assets/_Game/Scripts/Base/
  git commit -m "feat: add GridVisualizer, BaseBuilderManager, BuildingPlacer"
  ```

---

## Task 12: Battle — BattleManager, ResourceManager, UnitSpawner, SpawnZone, AIWaveSpawner

**Files:**
- Create: `Assets/_Game/Scripts/Battle/BattleManager.cs`
- Create: `Assets/_Game/Scripts/Battle/ResourceManager.cs`
- Create: `Assets/_Game/Scripts/Battle/UnitSpawner.cs`
- Create: `Assets/_Game/Scripts/Battle/SpawnZone.cs`
- Create: `Assets/_Game/Scripts/Battle/AIWaveSpawner.cs`

- [ ] **Step 1: `ResourceManager.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Battle/ResourceManager.cs
  using UnityEngine;
  using MedievalRTS.Core;

  namespace MedievalRTS.Battle
  {
      public class ResourceManager : MonoBehaviour
      {
          [SerializeField] private int startingGold = 100;
          [SerializeField] private float goldPerSecond = 5f;

          public int Gold { get; private set; }

          private float _accumulator;

          private void Start()
          {
              Gold = startingGold;
              EventBus.Publish(new GoldChangedEvent(Gold));
          }

          private void Update()
          {
              _accumulator += goldPerSecond * Time.deltaTime;
              if (_accumulator < 1f) return;
              int gained = Mathf.FloorToInt(_accumulator);
              _accumulator -= gained;
              AddGold(gained);
          }

          public bool TrySpend(int amount)
          {
              if (Gold < amount) return false;
              Gold -= amount;
              EventBus.Publish(new GoldChangedEvent(Gold));
              return true;
          }

          public void AddGold(int amount)
          {
              Gold += amount;
              EventBus.Publish(new GoldChangedEvent(Gold));
          }
      }
  }
  ```

- [ ] **Step 2: `SpawnZone.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Battle/SpawnZone.cs
  using UnityEngine;

  namespace MedievalRTS.Battle
  {
      public class SpawnZone : MonoBehaviour
      {
          [SerializeField] private BoxCollider _area;

          public Vector3 GetRandomPosition()
          {
              var b = _area.bounds;
              return new Vector3(
                  Random.Range(b.min.x, b.max.x),
                  b.center.y,
                  Random.Range(b.min.z, b.max.z));
          }
      }
  }
  ```

- [ ] **Step 3: `UnitSpawner.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Battle/UnitSpawner.cs
  using UnityEngine;
  using MedievalRTS.Data;
  using MedievalRTS.Units;

  namespace MedievalRTS.Battle
  {
      public class UnitSpawner : MonoBehaviour
      {
          [SerializeField] private SpawnZone _playerSpawnZone;

          private ResourceManager _resources;

          private void Start() => _resources = FindObjectOfType<ResourceManager>();

          public bool TrySpawnPlayerUnit(UnitData data)
          {
              if (!_resources.TrySpend(data.goldCost)) return false;
              SpawnUnit(data, _playerSpawnZone.GetRandomPosition(), isPlayer: true);
              return true;
          }

          public void SpawnEnemyUnit(UnitData data, Vector3 position)
          {
              SpawnUnit(data, position, isPlayer: false);
          }

          private void SpawnUnit(UnitData data, Vector3 position, bool isPlayer)
          {
              var go = Instantiate(data.prefab, position, Quaternion.identity);
              go.GetComponent<Unit>().Initialize(data, isPlayer);
          }
      }
  }
  ```

- [ ] **Step 4: `AIWaveSpawner.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Battle/AIWaveSpawner.cs
  using System.Collections;
  using UnityEngine;
  using MedievalRTS.Data;

  namespace MedievalRTS.Battle
  {
      public class AIWaveSpawner : MonoBehaviour
      {
          [SerializeField] private SpawnZone _enemySpawnZone;

          private UnitSpawner _spawner;

          private void Start() => _spawner = FindObjectOfType<UnitSpawner>();

          public void BeginWaves(StageData stage)
          {
              foreach (var wave in stage.waves)
                  StartCoroutine(SpawnWave(wave));
          }

          private IEnumerator SpawnWave(WaveData wave)
          {
              yield return new WaitForSeconds(wave.waveStartTime);
              foreach (var unitData in wave.units)
              {
                  _spawner.SpawnEnemyUnit(unitData, _enemySpawnZone.GetRandomPosition());
                  yield return new WaitForSeconds(wave.spawnInterval);
              }
          }
      }
  }
  ```

- [ ] **Step 5: `BattleManager.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Battle/BattleManager.cs
  using UnityEngine;
  using MedievalRTS.Core;
  using MedievalRTS.Data;
  using MedievalRTS.Campaign;

  namespace MedievalRTS.Battle
  {
      public class BattleManager : MonoBehaviour
      {
          private StageData _stage;
          private float _elapsed;
          private int _playerUnitsLost;
          private bool _active;
          private AIWaveSpawner _waveSpawner;

          private void Start()
          {
              _waveSpawner = FindObjectOfType<AIWaveSpawner>();
              EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
              EventBus.Subscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);

              _stage = CampaignManager.Instance.SelectedStage;
              if (_stage != null) StartBattle(_stage);
          }

          private void OnDestroy()
          {
              EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
              EventBus.Unsubscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);
          }

          private void StartBattle(StageData stage)
          {
              _elapsed = 0f;
              _playerUnitsLost = 0;
              _active = true;
              _waveSpawner.BeginWaves(stage);
              SpawnEnemyBase(stage);
          }

          private void SpawnEnemyBase(StageData stage)
          {
              foreach (var placement in stage.enemyBase)
              {
                  // BaseBuilderManager의 GridToWorld 재활용 대신 간단 오프셋 배치
                  var pos = new Vector3(placement.gridPosition.x, 0, placement.gridPosition.y);
                  var go = Instantiate(placement.data.prefab, pos, Quaternion.identity);
                  go.GetComponent<Buildings.Building>().Initialize(placement.data, isPlayerBuilding: false);
              }
          }

          private void Update()
          {
              if (!_active) return;
              _elapsed += Time.deltaTime;
              if (_elapsed >= _stage.battleDuration) EndBattle(false);
          }

          private void OnUnitDied(UnitDiedEvent evt)
          {
              if (evt.Unit.IsPlayerUnit) _playerUnitsLost++;
          }

          private void OnBuildingDestroyed(BuildingDestroyedEvent evt)
          {
              if (!evt.Building.IsPlayerBuilding &&
                  evt.Building.Data.buildingType == BuildingType.Castle)
                  EndBattle(true);

              if (evt.Building.IsPlayerBuilding &&
                  evt.Building.Data.buildingType == BuildingType.Castle)
                  EndBattle(false);
          }

          private void EndBattle(bool victory)
          {
              if (!_active) return;
              _active = false;
              var stats = new BattleStats
              {
                  Victory = victory,
                  UnitsLost = _playerUnitsLost,
                  TimeElapsed = _elapsed
              };
              int stars = StarRatingSystem.Calculate(stats, _stage);
              EventBus.Publish(new BattleEndedEvent(victory, stars, stats));
              GameManager.Instance.ChangeState(GameState.Result);
          }
      }
  }
  ```

- [ ] **Step 6: Battle 씬 설정 (수동)**

  Battle.unity 씬 열기:
  1. 빈 GameObject `BattleManager` → `BattleManager` 컴포넌트 추가
  2. 빈 GameObject `ResourceManager` → `ResourceManager` 컴포넌트 추가 (Starting Gold: 100, Gold Per Second: 5)
  3. 빈 GameObject `UnitSpawner` → `UnitSpawner` 컴포넌트 추가
  4. 빈 GameObject `PlayerSpawnZone` → `SpawnZone` + `BoxCollider` 추가 → Is Trigger: true → SpawnZone.Area에 할당
  5. 빈 GameObject `EnemySpawnZone` → 동일하게 생성
  6. 빈 GameObject `AIWaveSpawner` → `AIWaveSpawner` 컴포넌트 추가 → EnemySpawnZone 연결
  7. UnitSpawner의 PlayerSpawnZone 연결

- [ ] **Step 7: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Battle/
  git commit -m "feat: add BattleManager, ResourceManager, UnitSpawner, SpawnZone, AIWaveSpawner"
  ```

---

## Task 13: CampaignManager

**Files:**
- Create: `Assets/_Game/Scripts/Campaign/CampaignManager.cs`

- [ ] **Step 1: `CampaignManager.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Campaign/CampaignManager.cs
  using System.Linq;
  using UnityEngine;
  using MedievalRTS.Core;
  using MedievalRTS.Data;
  using MedievalRTS.Progression;

  namespace MedievalRTS.Campaign
  {
      public class CampaignManager : MonoBehaviour
      {
          public static CampaignManager Instance { get; private set; }

          [SerializeField] private StageData[] stages;

          public StageData SelectedStage { get; private set; }
          public StageData[] Stages => stages;

          private SaveData _saveData;

          private void Awake()
          {
              if (Instance != null) { Destroy(gameObject); return; }
              Instance = this;
              DontDestroyOnLoad(gameObject);

              _saveData = SaveSystem.Load();
              EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
          }

          private void OnDestroy()
          {
              EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
          }

          public void SelectAndStartStage(StageData stage)
          {
              SelectedStage = stage;
              GameManager.Instance.ChangeState(GameState.Battle);
          }

          public int GetStageStars(int stageIndex) =>
              _saveData.StageStars.TryGetValue(stageIndex, out int s) ? s : 0;

          public bool IsStageUnlocked(StageData stage)
          {
              if (stage.stageIndex == 0) return true;
              int totalStars = _saveData.StageStars.Values.Sum();
              return totalStars >= stage.unlockRequirementStars;
          }

          private void OnBattleEnded(BattleEndedEvent evt)
          {
              if (SelectedStage == null || !evt.Victory) return;

              int current = GetStageStars(SelectedStage.stageIndex);
              if (evt.Stars > current)
              {
                  _saveData.StageStars[SelectedStage.stageIndex] = evt.Stars;
                  SaveSystem.Save(_saveData);
              }
          }
      }
  }
  ```

- [ ] **Step 2: MainMenu 씬에 CampaignManager 배치 (수동)**

  MainMenu.unity → GameManager 오브젝트에 `CampaignManager` 컴포넌트 추가
  → Stages 배열에 생성한 StageData 에셋 순서대로 할당 (10~15개)

- [ ] **Step 3: Stage 1~3 에셋 생성 (수동)**

  `Assets/_Game/ScriptableObjects/Stages`에서 Create → Medieval RTS → Stage Data

  Stage 1 예시:
  - Stage Name: "Tutorial Village"
  - Stage Index: 0
  - Battle Duration: 180
  - Unlock Requirement Stars: 0
  - Enemy Base: Castle(5,5), ArcherTower(3,3), ArcherTower(7,3)
  - Waves: 1개 웨이브, waveStartTime=10, units=[Knight, Knight], spawnInterval=2

- [ ] **Step 4: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Campaign/CampaignManager.cs \
          Assets/_Game/ScriptableObjects/Stages/
  git commit -m "feat: add CampaignManager with stage progression and save integration"
  ```

---

## Task 14: UI — BattleHUD, BaseBuilderUI, ResultUI

**Files:**
- Create: `Assets/_Game/Scripts/UI/BattleHUD.cs`
- Create: `Assets/_Game/Scripts/Base/BaseBuilderUI.cs`
- Create: `Assets/_Game/Scripts/UI/ResultUI.cs`

- [ ] **Step 1: `BattleHUD.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/UI/BattleHUD.cs
  using UnityEngine;
  using UnityEngine.UI;
  using TMPro;
  using MedievalRTS.Core;
  using MedievalRTS.Data;
  using MedievalRTS.Battle;
  using MedievalRTS.Campaign;

  namespace MedievalRTS.UI
  {
      public class BattleHUD : MonoBehaviour
      {
          [SerializeField] private TMP_Text goldText;
          [SerializeField] private TMP_Text timerText;
          [SerializeField] private Transform unitButtonContainer;
          [SerializeField] private GameObject unitButtonPrefab; // Image + TMP_Text + Button
          [SerializeField] private UnitData[] availableUnits;

          private UnitSpawner _spawner;
          private float _elapsed;
          private float _duration;

          private void Start()
          {
              _spawner = FindObjectOfType<UnitSpawner>();
              _duration = CampaignManager.Instance?.SelectedStage?.battleDuration ?? 180f;

              EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);

              foreach (var unit in availableUnits)
                  CreateUnitButton(unit);
          }

          private void OnDestroy()
          {
              EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
          }

          private void Update()
          {
              _elapsed += Time.deltaTime;
              float remaining = Mathf.Max(0f, _duration - _elapsed);
              timerText.text = $"{Mathf.CeilToInt(remaining)}s";
          }

          private void OnGoldChanged(GoldChangedEvent evt)
          {
              goldText.text = evt.Amount.ToString();
          }

          private void CreateUnitButton(UnitData data)
          {
              var go = Instantiate(unitButtonPrefab, unitButtonContainer);
              go.GetComponentInChildren<TMP_Text>().text = $"{data.unitName}\n{data.goldCost}G";
              go.GetComponent<Image>().sprite = data.icon;
              go.GetComponent<Button>().onClick.AddListener(() => _spawner.TrySpawnPlayerUnit(data));
          }
      }
  }
  ```

- [ ] **Step 2: `BaseBuilderUI.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Base/BaseBuilderUI.cs
  using UnityEngine;
  using UnityEngine.UI;
  using TMPro;
  using MedievalRTS.Core;
  using MedievalRTS.Data;

  namespace MedievalRTS.Base
  {
      public class BaseBuilderUI : MonoBehaviour
      {
          [SerializeField] private BuildingPlacer placer;
          [SerializeField] private BaseBuilderManager manager;
          [SerializeField] private Button saveButton;
          [SerializeField] private Transform buttonContainer;
          [SerializeField] private GameObject buildingButtonPrefab;
          [SerializeField] private BuildingData[] catalog;

          private void Start()
          {
              saveButton.onClick.AddListener(OnSave);
              foreach (var b in catalog)
                  CreateBuildingButton(b);
          }

          private void CreateBuildingButton(BuildingData data)
          {
              var go = Instantiate(buildingButtonPrefab, buttonContainer);
              go.GetComponentInChildren<TMP_Text>().text = data.buildingName;
              go.GetComponent<Image>().sprite = data.icon;
              go.GetComponent<Button>().onClick.AddListener(() => placer.SelectBuilding(data));
          }

          private void OnSave()
          {
              manager.SaveLayout();
              GameManager.Instance.ChangeState(GameState.MainMenu);
          }
      }
  }
  ```

- [ ] **Step 3: `ResultUI.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/UI/ResultUI.cs
  using UnityEngine;
  using UnityEngine.UI;
  using TMPro;
  using MedievalRTS.Core;

  namespace MedievalRTS.UI
  {
      public class ResultUI : MonoBehaviour
      {
          [SerializeField] private TMP_Text titleText;
          [SerializeField] private GameObject[] starObjects; // 별 3개 GameObject
          [SerializeField] private Button continueButton;
          [SerializeField] private Button retryButton;

          private void Start()
          {
              EventBus.Subscribe<BattleEndedEvent>(ShowResult);
              continueButton.onClick.AddListener(() =>
                  GameManager.Instance.ChangeState(GameState.MainMenu));
              retryButton.onClick.AddListener(() =>
              {
                  var stage = Campaign.CampaignManager.Instance?.SelectedStage;
                  if (stage != null)
                      Campaign.CampaignManager.Instance.SelectAndStartStage(stage);
              });
          }

          private void OnDestroy()
          {
              EventBus.Unsubscribe<BattleEndedEvent>(ShowResult);
          }

          private void ShowResult(BattleEndedEvent evt)
          {
              titleText.text = evt.Victory ? "Victory!" : "Defeat";
              for (int i = 0; i < starObjects.Length; i++)
                  starObjects[i].SetActive(i < evt.Stars);
              retryButton.gameObject.SetActive(!evt.Victory);
          }
      }
  }
  ```

- [ ] **Step 4: UI Canvas 설정 (수동)**

  **Battle.unity:**
  - Hierarchy → UI → Canvas → 자식으로 빈 GameObject `BattleHUD` 추가
  - TMP_Text (골드), TMP_Text (타이머), 하단 Panel (유닛 버튼 컨테이너) 배치
  - `BattleHUD` 컴포넌트 추가 → 필드 연결

  **BaseBuilder.unity:**
  - Canvas → `BaseBuilderUI` 컴포넌트 추가 → 필드 연결
  - 좌측 패널에 건물 버튼들, 우하단에 Save 버튼

  **Result.unity:**
  - Canvas → 중앙에 결과 텍스트, 별 3개 Image, Continue/Retry 버튼
  - `ResultUI` 컴포넌트 추가 → 필드 연결

- [ ] **Step 5: 커밋**

  ```bash
  git add Assets/_Game/Scripts/UI/ Assets/_Game/Scripts/Base/BaseBuilderUI.cs
  git commit -m "feat: add BattleHUD, BaseBuilderUI, ResultUI"
  ```

---

## Task 15: 유닛 업그레이드 시스템

**Files:**
- Create: `Assets/_Game/Scripts/Progression/UnitUpgradeSystem.cs`

유닛 레벨(1~3)에 따라 maxHp·damage·moveSpeed를 배율로 스케일한다. UnitData 자체는 수정하지 않고 런타임에 스케일 값을 계산해 반환한다.

- [ ] **Step 1: `UnitUpgradeSystem.cs` 작성**

  ```csharp
  // Assets/_Game/Scripts/Progression/UnitUpgradeSystem.cs
  using MedievalRTS.Data;
  using MedievalRTS.Progression;

  namespace MedievalRTS.Progression
  {
      public static class UnitUpgradeSystem
      {
          private const float LevelMultiplier = 0.2f; // 레벨당 +20%

          /// <summary>현재 레벨 기반 최종 maxHp 반환</summary>
          public static int GetMaxHp(UnitData data, SaveData save)
          {
              int level = GetLevel(data, save);
              return Mathf.RoundToInt(data.maxHp * (1f + (level - 1) * LevelMultiplier));
          }

          /// <summary>현재 레벨 기반 최종 damage 반환</summary>
          public static int GetDamage(UnitData data, SaveData save)
          {
              int level = GetLevel(data, save);
              return Mathf.RoundToInt(data.damage * (1f + (level - 1) * LevelMultiplier));
          }

          public static int GetLevel(UnitData data, SaveData save)
          {
              return save.UnitLevels.TryGetValue(data.unitName, out int lvl)
                  ? UnityEngine.Mathf.Clamp(lvl, 1, 3)
                  : 1;
          }

          /// <summary>별점을 업그레이드 포인트로 변환해 유닛 레벨업 시도</summary>
          public static bool TryLevelUp(UnitData data, SaveData save)
          {
              int current = GetLevel(data, save);
              if (current >= 3) return false;
              save.UnitLevels[data.unitName] = current + 1;
              return true;
          }
      }
  }
  ```

  > 참고: `Mathf`는 `UnityEngine` 네임스페이스에 있으므로 위 코드처럼 `UnityEngine.Mathf`로 직접 참조하거나 파일 상단에 `using UnityEngine;` 추가.

- [ ] **Step 2: Unit.Initialize에서 업그레이드 수치 적용**

  `Assets/_Game/Scripts/Units/Unit.cs`의 `Initialize` 메서드를 수정:

  ```csharp
  public void Initialize(UnitData data, bool isPlayerUnit, SaveData save = null)
  {
      Data = data;
      // 플레이어 유닛이면 업그레이드 적용, AI 유닛이면 기본값
      if (isPlayerUnit && save != null)
      {
          CurrentHp = UnitUpgradeSystem.GetMaxHp(data, save);
          _effectiveDamage = UnitUpgradeSystem.GetDamage(data, save);
      }
      else
      {
          CurrentHp = data.maxHp;
          _effectiveDamage = data.damage;
      }
      IsPlayerUnit = isPlayerUnit;
      gameObject.tag = isPlayerUnit ? "PlayerUnit" : "EnemyUnit";
  }
  ```

  `Unit.cs`에 `_effectiveDamage` 필드 추가 및 `UnitAI.Attack()`에서 `Data.damage` 대신 `_unit.EffectiveDamage` 사용:

  ```csharp
  // Unit.cs에 추가
  private int _effectiveDamage;
  public int EffectiveDamage => _effectiveDamage;
  ```

  `UnitAI.cs`의 `Attack()` 내부 수정:
  ```csharp
  private void Attack()
  {
      if (_target == null) return;
      var targetUnit = _target.GetComponent<Unit>();
      if (targetUnit != null) { targetUnit.TakeDamage(_unit.EffectiveDamage); return; }
      var targetBuilding = _target.GetComponent<Buildings.Building>();
      targetBuilding?.TakeDamage(_unit.EffectiveDamage);
  }
  ```

- [ ] **Step 3: UnitSpawner에서 SaveData 전달**

  `UnitSpawner.cs`의 `TrySpawnPlayerUnit` 수정:

  ```csharp
  public bool TrySpawnPlayerUnit(UnitData data)
  {
      if (!_resources.TrySpend(data.goldCost)) return false;
      var go = Instantiate(data.prefab, _playerSpawnZone.GetRandomPosition(), Quaternion.identity);
      var save = SaveSystem.Load();
      go.GetComponent<Unit>().Initialize(data, isPlayer: true, save: save);
      return true;
  }
  ```

- [ ] **Step 4: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Progression/UnitUpgradeSystem.cs \
          Assets/_Game/Scripts/Units/Unit.cs \
          Assets/_Game/Scripts/Units/UnitAI.cs \
          Assets/_Game/Scripts/Battle/UnitSpawner.cs
  git commit -m "feat: add unit upgrade system with level-based stat scaling"
  ```

---

## Task 16: Android + WebGL 빌드 설정

- [ ] **Step 1: Player Settings 설정 (수동)**

  Edit → Project Settings → Player:
  - Company Name: 원하는 이름
  - Product Name: `Medieval RTS`
  - Bundle Identifier (Android): `com.yourstudio.medievalrts`

  Android 탭:
  - Minimum API Level: Android 7.0 (API 24)
  - Target API Level: Automatic (highest installed)
  - Scripting Backend: IL2CPP
  - Target Architectures: ARM64 체크

  WebGL 탭:
  - Compression Format: Gzip
  - Exception Support: None (성능 최적화)

- [ ] **Step 2: Android 빌드 테스트**

  File → Build Settings → Android → Switch Platform → Build
  빌드 경로: `C:\Development\15_MD\Builds\Android\`
  Expected: APK 파일 생성

- [ ] **Step 3: WebGL 빌드 테스트**

  File → Build Settings → WebGL → Switch Platform → Build
  빌드 경로: `C:\Development\15_MD\Builds\WebGL\`
  Expected: `index.html` + `Build/` 폴더 생성

- [ ] **Step 4: .gitignore에 빌드 폴더 제외**

  ```bash
  cat >> "C:/Development/15_MD/.gitignore" << 'EOF'
  Builds/
  [Ll]ibrary/
  [Tt]emp/
  [Oo]bj/
  [Bb]uild/
  [Ll]ogs/
  [Mm]emoryCaptures/
  *.pidb.meta
  *.pdb.meta
  *.mdb.meta
  EOF
  git add .gitignore
  git commit -m "chore: add .gitignore for Unity build artifacts"
  ```

---

## Task 16: Fog of War 에셋 연동

> ⚠️ 이 태스크는 Asset Store에서 Fog of War 에셋 구매/다운로드 후 수행한다.
> 권장 에셋: "FOW - Fog of War" 또는 "Top Down Fog of War"

- [ ] **Step 1: 에셋 임포트**

  Asset Store에서 FOW 에셋 구매 → Unity Package Manager → My Assets → 해당 에셋 Import

- [ ] **Step 2: `FogRevealAgent.cs` 업데이트**

  에셋의 API 문서를 확인한 후 `FogRevealAgent.Update()` 내부를 교체:

  ```csharp
  // 에셋 API에 맞게 교체 — 아래는 예시
  private void Update()
  {
      if (!_unit.IsAlive) return;
      // 예시: FogOfWarManager.Reveal(transform.position, _unit.Data.sightRange);
      // 실제 API는 에셋 문서 참고
  }
  ```

- [ ] **Step 3: Battle 씬에 FOW Manager 배치 (수동)**

  에셋 문서에 따라 FOW Manager 컴포넌트를 Battle 씬에 추가하고 카메라·레이어 설정

- [ ] **Step 4: 적 기지 초기 안개 설정**

  배틀 시작 시 적 기지 구역을 안개로 가리는 설정 — 에셋 문서 참고

- [ ] **Step 5: 커밋**

  ```bash
  git add Assets/_Game/Scripts/Units/FogRevealAgent.cs
  git commit -m "feat: integrate Fog of War asset with unit sight system"
  ```

---

## 완료 기준

- [ ] EditMode 테스트 전체 PASS (GridSystem 7개, SaveSystem 5개, StarRating 5개)
- [ ] BaseBuilder에서 건물 배치 후 저장 → 재시작해도 레이아웃 유지
- [ ] Battle에서 Castle 파괴 시 Result 씬으로 전환
- [ ] 별점이 저장되고 스테이지 언락 작동
- [ ] Android APK 빌드 성공
- [ ] WebGL 빌드 성공
