// Assets/_Game/Scripts/Testing/TestBootstrap.cs
// ─ 준비 화면: 골드로 병력 구성
// ─ 출전 시: 병력 전체 도열 (황색 링 = 명령 대기)
// ─ 좌클릭 선택 → 우클릭 목적지/적 → 명령 잠금 (재지정 불가)
// ─ 방향 버튼: 대기 중인 병력 일괄 파견
// ─ 시야 시스템: 아군 시야 밖 적 유닛 은폐
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MedievalRTS.Core;
using MedievalRTS.Data;
using MedievalRTS.Units;
using MedievalRTS.Buildings;
using MedievalRTS.Battle;

namespace MedievalRTS.Testing
{
    public class TestBootstrap : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════
        //  유닛 정의
        // ═══════════════════════════════════════════════════════
        private struct UnitDef
        {
            public string name, assetName, desc;
            public int    hp, dmg, cost;
            public float  speed, atkRange, cooldown, threat, bldgMult;
            public Color  color;
            public int    valorToUnlock;
        }

        private static readonly UnitDef[] Defs =
        {
            new UnitDef{name="기사",  assetName="Knight",  desc="탱커 근접",  hp=150,dmg=22,cost=50, speed=2.5f,atkRange=1.8f,cooldown=1.0f,threat=6f, bldgMult=1.0f,color=new Color(0.2f,0.4f,1f),   valorToUnlock=0},
            new UnitDef{name="궁수",  assetName="Archer",  desc="원거리 딜",  hp=70, dmg=30,cost=35, speed=3.0f,atkRange=7.0f,cooldown=1.2f,threat=9f, bldgMult=1.0f,color=new Color(0.2f,0.8f,0.8f), valorToUnlock=0},
            new UnitDef{name="마법사",assetName="Mage",    desc="건물 마법",  hp=90, dmg=55,cost=75, speed=2.0f,atkRange=5.5f,cooldown=1.5f,threat=7f, bldgMult=1.8f,color=new Color(0.7f,0.1f,0.9f), valorToUnlock=0},
            new UnitDef{name="정찰병",assetName="Scout",   desc="저렴·고속",  hp=50, dmg=18,cost=25, speed=5.5f,atkRange=1.5f,cooldown=0.8f,threat=5f, bldgMult=1.0f,color=new Color(0.3f,0.8f,0.3f), valorToUnlock=0},
            new UnitDef{name="기병",  assetName="Cavalry", desc="돌격대",     hp=130,dmg=32,cost=70, speed=6.0f,atkRange=1.8f,cooldown=0.9f,threat=7f, bldgMult=1.0f,color=new Color(1f,0.85f,0.1f),  valorToUnlock=1},
            new UnitDef{name="공성기",assetName="Catapult",desc="건물 특화",  hp=50, dmg=85,cost=110,speed=1.2f,atkRange=10f, cooldown=2.5f,threat=6f, bldgMult=3.5f,color=new Color(1f,0.45f,0.1f),  valorToUnlock=2},
        };

        // ═══════════════════════════════════════════════════════
        //  게임 상태
        // ═══════════════════════════════════════════════════════
        private enum Phase { Prep, Battle, GameOver }
        private Phase _phase = Phase.Prep;

        private const float BattleTimeLimit = 180f;

        private int   _gold  = 999999;
        private int   _valor = 0;
        private float _elapsed;
        private float _dmgMult = 1f;
        private readonly int[]        _roster   = new int[6];
        private readonly HashSet<int> _unlocked = new HashSet<int>();

        // 전투 중 획득 통계
        private int _earnedGold, _earnedValor, _destroyedBuildings;

        // 모드 선택
        private bool _defenseMode = false;

        // ═══════════════════════════════════════════════════════
        //  씬 오브젝트
        // ═══════════════════════════════════════════════════════
        private Building _enemyCastle;
        private Building _playerCastle;
        private readonly List<Building>         _enemyBarracks     = new List<Building>();
        private readonly List<Building>         _allEnemyBuildings = new List<Building>();
        private readonly List<Building>         _allPlayerBuildings = new List<Building>();
        private readonly List<TestSimpleUnitAI> _playerUnits       = new List<TestSimpleUnitAI>();
        private readonly List<TestSimpleUnitAI> _selectedUnits     = new List<TestSimpleUnitAI>();
        private readonly List<GameObject>       _wallSegments      = new List<GameObject>();
        private int _gateIndex = 2; // 동쪽 성벽 중 어느 칸이 문인지

        // 수비 진형 구성
        private bool _defenseSetupActive;
        private int  _selectedPlaceBldg = -1; // 0=방어탑, 1=성벽
        private GameObject _dsHud;
        private Text _dsGoldText, _dsStatusText;
        private readonly Button[] _dsPalBtns  = new Button[2];
        private readonly Text[]   _dsPalLbls  = new Text[2];
        private readonly Button[] _dsUnitBtns = new Button[6];
        private readonly Text[]   _dsUnitLbls = new Text[6];
        private readonly Button[] _dsSpecBtns = new Button[6];
        private readonly Text[]   _dsSpecLbls = new Text[6];
        private Button _startBattleBtn, _enterSetupBtn;
        private int _stagingCol, _stagingRow;

        private static readonly (string label, int cost)[] _placeDefs =
        {
            ("방어탑", 80),
            ("성벽",   20),
        };

        // ═══════════════════════════════════════════════════════
        //  UI
        // ═══════════════════════════════════════════════════════
        private Font       _font;
        private GameObject _canvas;
        private GameObject _prepPanel, _battleHud, _upgradePanel, _resultPanel;
        private GameObject _rightPanel;
        private GameObject _spellSectionRoot; // 마법 구매 섹션 전체 (레이블 + 버튼)

        // 준비 화면
        private Text   _prepGoldText, _rosterText;
        private Button _modeToggleBtn;
        private Text   _modeToggleLbl;
        private readonly Button[] _buyBtns   = new Button[6];
        private readonly Text[]   _buyLabels = new Text[6];

        // 전투 HUD
        private Text       _timerText, _valorHudText, _enemyHpText, _infoText;
        private GameObject _unitTypeBar, _selectionBox;
        private Vector2    _dragStart;
        private bool       _isDragging;
        private readonly HashSet<GameObject>  _revealedBuildings = new HashSet<GameObject>();
        private readonly HashSet<Vector2Int>  _revealedCells     = new HashSet<Vector2Int>();
        private const float FowCellSize = 2f;
        private LineRenderer _spellRangeCircle;

        // 기지 개발
        private readonly Button[] _upgBtns = new Button[4];

        // 결과
        private Text _resultText, _resultStatsText;

        // 전투 중 자원 현황 패널
        private Text _statGoldText, _statValorText, _statBldgText;

        // 특수 건물 UI (준비 화면 우측)
        private readonly Button[] _specialBldgBtns = new Button[6];
        private readonly Text[]   _specialBldgLbls = new Text[6];

        // 마법 구매 UI (준비 화면 우측)
        private readonly Button[] _spellBuyBtns = new Button[5];
        private readonly Text[]   _spellBuyLbls = new Text[5];

        // 마법 전투 버튼
        private int _pendingSpell = -1;
        private readonly Button[] _spellBattleBtns      = new Button[5];
        private readonly Text[]   _spellBattleChargeLbls = new Text[5];

        // ═══════════════════════════════════════════════════════
        //  라이프사이클
        // ═══════════════════════════════════════════════════════
        private void Start()
        {
            BuildingEffectSystem.Reset();
            SpellSystem.Reset();
            BuildWorld();
            BuildUI();
            EventBus.Subscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);
        }

        private void Update()
        {
            // 수비 진형 구성 중: 유닛 선택·명령 + 건물 배치
            if (_phase == Phase.Prep && _defenseSetupActive)
            {
                _playerUnits.RemoveAll(u => u == null || !u.GetComponent<Unit>().IsAlive);
                _selectedUnits.RemoveAll(u => u == null);
                HandleInput();
                return;
            }

            if (_phase != Phase.Battle) return;

            _elapsed += Time.deltaTime;
            float remaining = BattleTimeLimit - _elapsed;
            int remSec = Mathf.CeilToInt(Mathf.Max(remaining, 0f));
            _timerText.text  = $"⏱ {remSec}s";
            _timerText.color = remaining < 30f
                ? (Mathf.FloorToInt(remaining * 2f) % 2 == 0 ? Color.red : Color.white)
                : Color.white;
            if (_defenseMode)
                _enemyHpText.text = $"아군 성: {(_playerCastle != null ? _playerCastle.CurrentHp : 0)}";
            else
                _enemyHpText.text = $"적성: {(_enemyCastle != null ? _enemyCastle.CurrentHp : 0)}";

            HandleInput();
            _playerUnits.RemoveAll(u => u == null || !u.GetComponent<Unit>().IsAlive);
            _selectedUnits.RemoveAll(u => u == null);

            // ── 종료 조건 체크 ──────────────────────────────
            if (remaining <= 0f) { EndGame(!_defenseMode, "시간 초과"); return; }
            if (!_defenseMode && _playerUnits.Count == 0) { EndGame(false, "전군 전멸"); return; }
        }

        // ═══════════════════════════════════════════════════════
        //  월드 구성
        // ═══════════════════════════════════════════════════════
        private void BuildWorld()
        {
            SetupCamera();
            SetupLight();
            SetupGround();

            // 1선: 성벽 (x=2) — 고체력 장벽
            MakeWall("Wall_L",  new Vector3(2, 1f,  6));
            MakeWall("Wall_C",  new Vector3(2, 1f,  0));
            MakeWall("Wall_R",  new Vector3(2, 1f, -6));

            // 2선: 망루 (x=5) — 전방 공격 타워
            MakeTower("Tower_F1", new Vector3(5, 1f,  5));
            MakeTower("Tower_F2", new Vector3(5, 1f, -5));

            // 3선: 병영 + 봉화대 (x=9)
            _enemyBarracks.Add(MakeBarracks("Barracks_L", new Vector3(9, 0.6f,  5)));
            _enemyBarracks.Add(MakeBarracks("Barracks_R", new Vector3(9, 0.6f, -5)));
            MakeBuffBuilding("Shrine", new Vector3(9, 0.75f, 0));

            // 4선: 마법사 탑 (x=13)
            MakeMageTower("MageTower_L", new Vector3(13, 1.4f,  4));
            MakeMageTower("MageTower_R", new Vector3(13, 1.4f, -4));

            // 5선: 후방 망루 (x=16)
            MakeTower("Tower_B1", new Vector3(16, 1f,  6));
            MakeTower("Tower_BC", new Vector3(16, 1f,  0));
            MakeTower("Tower_B2", new Vector3(16, 1f, -6));

            // 적 성 (x=21)
            _enemyCastle = MakeBuilding("EnemyCastle", new Vector3(21, 1.5f, 0), 900, false,
                new Color(0.75f, 0.1f, 0.1f), new Vector3(4, 3, 4));
        }

        private void MakeWall(string n, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = new Vector3(0.8f, 2f, 3.5f);
            Paint(go, new Color(0.35f, 0.35f, 0.38f));
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = 600;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: false);
            _allEnemyBuildings.Add(b);
        }

        private void MakeMageTower(string n, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = new Vector3(1.2f, 1.4f, 1.2f);
            Paint(go, new Color(0.45f, 0.1f, 0.75f));
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = 180;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: false);
            _allEnemyBuildings.Add(b);
            go.AddComponent<TestMageTowerAI>().Setup(false, 14f, 35, 2.5f, 2);
        }

        private void MakeBuffBuilding(string n, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = new Vector3(1.8f, 1.5f, 1.8f);
            Paint(go, new Color(0.85f, 0.72f, 0.1f));
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = 150;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: false);
            _allEnemyBuildings.Add(b);
            go.AddComponent<TestBuffBuildingAI>().Setup(false);
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var g = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = g.AddComponent<Camera>(); g.AddComponent<AudioListener>();
            }
            cam.transform.SetPositionAndRotation(new Vector3(4, 28, -22), Quaternion.Euler(50, 0, 0));
            cam.backgroundColor = new Color(0.36f, 0.55f, 0.85f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private void SetupLight()
        {
            var l = FindObjectOfType<Light>();
            if (l == null) { var g = new GameObject("Sun"); l = g.AddComponent<Light>(); l.type = LightType.Directional; }
            l.transform.rotation = Quaternion.Euler(50, -30, 0);
            l.intensity = 1.2f;
        }

        private void SetupGround()
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
            g.name = "Ground"; g.transform.localScale = new Vector3(6, 1, 3);
            Paint(g, new Color(0.28f, 0.48f, 0.18f));
        }

        private Building MakeBuilding(string n, Vector3 pos, int hp,
            bool isPlayer, Color col, Vector3? scale = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = scale ?? Vector3.one;
            Paint(go, col);
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = hp;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayer);
            if (!isPlayer) _allEnemyBuildings.Add(b);
            return b;
        }

        private Building MakeBarracks(string n, Vector3 pos)
        {
            return MakeBuilding(n, pos, 300, false,
                new Color(0.45f, 0.08f, 0.08f), new Vector3(2.5f, 1.2f, 2.5f));
        }

        private void MakeTower(string n, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            Paint(go, new Color(0.55f, 0.08f, 0.08f));
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = 220;
            var tb = go.AddComponent<Building>();
            tb.Initialize(data, isPlayerBuilding: false);
            _allEnemyBuildings.Add(tb);
            go.AddComponent<TestTowerAI>().Setup(false, 9f, 18, 1.2f);
        }

        // ═══════════════════════════════════════════════════════
        //  모드 전환
        // ═══════════════════════════════════════════════════════
        private void ToggleMode()
        {
            _defenseMode = !_defenseMode;
            if (_modeToggleLbl != null)
                _modeToggleLbl.text = _defenseMode ? "수비 모드 🛡" : "공격 모드 ▶";
            _modeToggleBtn.GetComponent<Image>().color = _defenseMode
                ? new Color(0.4f, 0.15f, 0.15f)
                : new Color(0.15f, 0.4f, 0.15f);
            ApplyModePrepVisibility();
        }

        /// <summary>공격/수비 모드에 따라 준비 화면 섹션 가시성을 조정합니다.</summary>
        private void ApplyModePrepVisibility()
        {
            // 공격 모드: 마법 섹션·출전 버튼 / 수비 모드: 진형 구성 시작 버튼
            if (_spellSectionRoot != null)  _spellSectionRoot.SetActive(!_defenseMode);
            if (_rightPanel != null)        _rightPanel.SetActive(false); // 수비 특수건물은 진형 구성 HUD에 표시
            if (_startBattleBtn != null)    _startBattleBtn.gameObject.SetActive(!_defenseMode);
            if (_enterSetupBtn != null)     _enterSetupBtn.gameObject.SetActive(_defenseMode);
        }

        // ═══════════════════════════════════════════════════════
        //  페이즈 전환
        // ═══════════════════════════════════════════════════════
        private void EnterBattle()
        {
            int total = 0;
            foreach (var c in _roster) total += c;
            // 공격 모드는 유닛 필요, 수비 모드는 타워만으로도 가능
            if (total == 0 && !_defenseMode) return;

            _prepPanel.SetActive(false);
            if (_dsHud != null) _dsHud.SetActive(false);
            _battleHud.SetActive(true);
            _upgradePanel.SetActive(true);
            _phase = Phase.Battle;
            BuildingEffectSystem.TreasuryAlive = BuildingEffectSystem.GetLevel(SpecialBuildingType.Treasury) > 0;
            RefreshSpellBattleBtns();

            if (_defenseMode)
            {
                if (_defenseSetupActive)
                {
                    // 진형 구성에서 전환 — 건물·유닛 이미 배치됨
                    BuildUnitTypeButtons();
                    SetInfo("전투 시작! 적이 오른쪽에서 공격합니다.");
                }
                else
                {
                    // 준비 패널에서 바로 시작 (폴백)
                    BuildDefenseBase();
                    DeployDefenseArmy();
                }
                StartCoroutine(DefenseEnemyWaveRoutine());
            }
            else
            {
                DeployArmy();
                StartCoroutine(SpawnInitialEnemyForce());
                StartCoroutine(FogOfWarRoutine());
            }
        }

        // ═══════════════════════════════════════════════════════
        //  병력 도열
        // ═══════════════════════════════════════════════════════
        private void DeployArmy()
        {
            // 4열 종대로 좌측 배치
            int col = 0, row = 0;
            for (int i = 0; i < Defs.Length; i++)
            {
                for (int k = 0; k < _roster[i]; k++)
                {
                    // x: -14부터 뒤로, z: 중앙 기준 좌우
                    float x = -14f - col * 2.2f;
                    float z = (row - 1) * 2.4f;
                    var ai = SpawnUnit(i, true, new Vector3(x, 0, z));
                    ai.SetAwaitingOrders();
                    _playerUnits.Add(ai);
                    if (++row >= 3) { row = 0; col++; }
                }
            }
            BuildUnitTypeButtons();
            SetInfo("병력이 도열했습니다 — 드래그·클릭으로 선택 후 우클릭 또는 방향 버튼");
        }

        // ═══════════════════════════════════════════════════════
        //  유닛 스폰
        // ═══════════════════════════════════════════════════════
        private TestSimpleUnitAI SpawnUnit(int idx, bool isPlayer, Vector3 pos)
        {
            var def = Defs[idx];
            var prefab = FindUnitPrefab(def.assetName);
            GameObject go;
            if (prefab != null)
            {
                // NavMeshAgent OnEnable 방지: 비활성 상태로 복사 → 컴포넌트 제거 → 활성화
                bool wasActive = prefab.activeSelf;
                prefab.SetActive(false);
                go = Instantiate(prefab);
                prefab.SetActive(wasActive);
                go.transform.position = pos + Vector3.up * 0.5f;
                var mainAI = go.GetComponent<Units.UnitAI>();
                if (mainAI != null) DestroyImmediate(mainAI);
                var nav = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (nav != null) DestroyImmediate(nav);
                go.SetActive(true);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.position = pos + new Vector3(
                    Random.Range(-0.2f, 0.2f), 0.5f, Random.Range(-0.2f, 0.2f));
                go.transform.localScale = Vector3.one * 0.8f;
                Paint(go, def.color);
            }
            go.name = $"{(isPlayer ? "P" : "E")}_{def.name}";

            // 공격 모드 적 유닛: FOW 적용 전까지 숨겨서 깜박임 방지
            // 수비 모드: FOW 없음 → 적 유닛 즉시 표시
            if (!isPlayer && !_defenseMode)
            {
                foreach (var rnd in go.GetComponentsInChildren<Renderer>()) rnd.enabled = false;
                foreach (var col in go.GetComponentsInChildren<Collider>())  col.enabled = false;
            }

            var unit = go.GetComponent<Unit>() ?? go.AddComponent<Unit>();
            var data = ScriptableObject.CreateInstance<UnitData>();
            data.unitName = def.name;
            if (isPlayer)
            {
                float sm = BuildingEffectSystem.GetUnitStatMultiplier();
                data.maxHp  = Mathf.RoundToInt(def.hp  * sm);
                data.damage = Mathf.RoundToInt(def.dmg  * _dmgMult * sm);
            }
            else
            {
                data.maxHp  = def.hp;
                data.damage = def.dmg;
            }
            unit.Initialize(data, isPlayerUnit: isPlayer);

            var ai = go.GetComponent<TestSimpleUnitAI>() ?? go.AddComponent<TestSimpleUnitAI>();
            ai.Setup(def.speed, def.atkRange, def.cooldown, def.threat, def.bldgMult);
            return ai;
        }

        private static GameObject FindUnitPrefab(string assetName)
        {
#if UNITY_EDITOR
            var ud = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitData>(
                $"Assets/_Game/ScriptableObjects/Units/{assetName}.asset");
            if (ud != null && ud.prefab != null) return ud.prefab;
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:Prefab {assetName}", new[] { "Assets" }))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(path),
                    assetName, System.StringComparison.OrdinalIgnoreCase))
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
#endif
            return null;
        }

        // ═══════════════════════════════════════════════════════
        //  공격 모드 — 초기 적 배치 (수비측은 재생산 없음)
        // ═══════════════════════════════════════════════════════
        private IEnumerator SpawnInitialEnemyForce()
        {
            yield return new WaitForSeconds(2f);
            // 1선 수비 유닛 (기사 + 궁수)
            var line1 = new[] {
                new Vector3(4,0,4), new Vector3(4,0,0), new Vector3(4,0,-4),
                new Vector3(5,0,7), new Vector3(5,0,-7),
            };
            foreach (var p in line1)
            {
                SpawnUnit(Random.Range(0, 2), false, p);
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(1f);
            // 2선: 마법사 + 정찰병
            var line2 = new[] {
                new Vector3(8,0,5), new Vector3(8,0,0), new Vector3(8,0,-5),
            };
            foreach (var p in line2)
            {
                SpawnUnit(Random.Range(1, 4), false, p);
                yield return new WaitForSeconds(0.25f);
            }
            // 3선 (성 근처): 강한 유닛
            yield return new WaitForSeconds(2f);
            var line3 = new[] {
                new Vector3(17,0,4), new Vector3(17,0,-4),
                new Vector3(19,0,0),
            };
            foreach (var p in line3)
            {
                SpawnUnit(Random.Range(3, Defs.Length), false, p);
                yield return new WaitForSeconds(0.25f);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  수비 모드 — 기지 구축 + 성벽 생성
        // ═══════════════════════════════════════════════════════
        private void BuildDefenseBase()
        {
            // 플레이어 성 (왼쪽 끝)
            _playerCastle = MakePlayerBuilding("PlayerCastle", new Vector3(-21, 1.5f, 0), 900,
                new Color(0.2f, 0.35f, 0.75f), new Vector3(4, 3, 4));

            // 플레이어 타워 (성 앞)
            MakePlayerTower("PTower_L",  new Vector3(-16, 1f,  6));
            MakePlayerTower("PTower_C",  new Vector3(-16, 1f,  0));
            MakePlayerTower("PTower_R",  new Vector3(-16, 1f, -6));
            MakePlayerTower("PTower_F1", new Vector3(-13, 1f,  4));
            MakePlayerTower("PTower_F2", new Vector3(-13, 1f, -4));

            // 자동 성벽 생성 (x=-10 라인, z=-8~8)
            GenerateAutoWall(-10f, -8f, 8f);

            // 적 방향 (오른쪽) 표시용 표지
            SetInfo("수비 준비 완료 — 적이 오른쪽에서 공격합니다! 방향 버튼으로 병력 배치");
        }

        private Building MakePlayerBuilding(string n, Vector3 pos, int hp, Color col, Vector3? scale = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = scale ?? Vector3.one;
            Paint(go, col);
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = hp;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: true);
            _allPlayerBuildings.Add(b);
            return b;
        }

        private void MakePlayerTower(string n, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            Paint(go, new Color(0.2f, 0.35f, 0.75f));
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = 220;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: true);
            _allPlayerBuildings.Add(b);
            // 타워 AI (아군이므로 isPlayerSide=true → 적 유닛 공격)
            go.AddComponent<TestTowerAI>().Setup(true, 9f, 18, 1.2f);
        }

        private void GenerateAutoWall(float wallX, float zMin, float zMax)
        {
            _wallSegments.Clear();
            float segH = 2f; // 각 성벽 세그먼트 높이
            float segW = 0.8f;
            int count = Mathf.RoundToInt((zMax - zMin) / 2.5f) + 1;
            for (int i = 0; i < count; i++)
            {
                float z = Mathf.Lerp(zMin, zMax, (float)i / (count - 1));
                bool isGate = (i == _gateIndex);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Wall_{i}";
                go.transform.position = new Vector3(wallX, segH * 0.5f, z);
                go.transform.localScale = new Vector3(segW, segH, 2.3f);
                Paint(go, isGate ? new Color(0.6f, 0.5f, 0.1f) : new Color(0.35f, 0.35f, 0.38f));
                // 문(gate)은 통과 가능 (콜라이더 없앰)
                if (isGate) { Destroy(go.GetComponent<Collider>()); }
                _wallSegments.Add(go);

                // 문 클릭 이벤트용 태그 (BuildingData 없이 단순 오브젝트)
                if (isGate) go.name = "Gate";
            }
        }

        private void MoveGate(int newIdx)
        {
            if (newIdx < 0 || newIdx >= _wallSegments.Count) return;
            // 기존 문 → 일반 성벽으로 복원
            var old = _wallSegments[_gateIndex];
            if (old != null)
            {
                Paint(old, new Color(0.35f, 0.35f, 0.38f));
                if (old.GetComponent<Collider>() == null) old.AddComponent<BoxCollider>();
                old.name = $"Wall_{_gateIndex}";
            }
            _gateIndex = newIdx;
            var gateGo = _wallSegments[_gateIndex];
            if (gateGo != null)
            {
                Paint(gateGo, new Color(0.6f, 0.5f, 0.1f));
                Destroy(gateGo.GetComponent<Collider>());
                gateGo.name = "Gate";
            }
        }

        private void DeployDefenseArmy()
        {
            // 성벽 안쪽(서쪽)에 4열 배치
            int col = 0, row = 0;
            for (int i = 0; i < Defs.Length; i++)
            {
                for (int k = 0; k < _roster[i]; k++)
                {
                    float x = -12f - col * 2.2f;
                    float z = (row - 1) * 2.4f;
                    var ai = SpawnUnit(i, true, new Vector3(x, 0, z));
                    ai.SetAwaitingOrders();
                    _playerUnits.Add(ai);
                    if (++row >= 3) { row = 0; col++; }
                }
            }
            BuildUnitTypeButtons();
        }

        private IEnumerator DefenseEnemyWaveRoutine()
        {
            int wave = 0;
            var waveSpawnX = new[] { 20f, 18f, 16f };
            while (_phase == Phase.Battle)
            {
                yield return new WaitForSeconds(wave == 0 ? 5f : 25f);
                if (_phase != Phase.Battle) yield break;
                wave++;
                int unitCount = 3 + wave * 2;
                int maxUnitIdx = Mathf.Min(wave, Defs.Length - 1);
                ShowResourcePopup(new Vector3(0, 3, 0), $"제{wave}파 공격!");
                SetInfo($"제{wave}파 적이 공격합니다!");
                for (int i = 0; i < unitCount; i++)
                {
                    float z = Random.Range(-7f, 7f);
                    float x = waveSpawnX[Random.Range(0, waveSpawnX.Length)];
                    SpawnUnit(Random.Range(0, maxUnitIdx + 1), false, new Vector3(x, 0, z));
                    yield return new WaitForSeconds(0.4f);
                }
                if (wave >= 5)
                {
                    // 마지막 파 처리 후 일정 시간 대기 → 승리
                    yield return new WaitForSeconds(22f);
                    if (_phase == Phase.Battle) EndGame(true, "전 파 격퇴!");
                    yield break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  시야 시스템 (FOW)
        // ═══════════════════════════════════════════════════════
        private IEnumerator FogOfWarRoutine()
        {
            var wait = new WaitForSeconds(0.12f);
            while (_phase == Phase.Battle)
            {
                yield return wait;
                var sources = new List<(Vector3 p, float r)>();
                foreach (var ai in _playerUnits)
                {
                    if (ai == null) continue;
                    var u = ai.GetComponent<Unit>();
                    if (u != null && u.IsAlive) sources.Add((ai.transform.position, 10f));
                }
                // 시야 도달 셀 기록 — 마법 사용 가능 구역 누적
                foreach (var (p, r) in sources) MarkRevealed(p, r);
                ApplyFog("EnemyUnit", sources, 10f);
                ApplyFogBuildings(sources, 14f);
            }
        }

        // 건물: 한 번 본 것은 영구 표시
        private void ApplyFogBuildings(List<(Vector3 p, float r)> sources, float sightRadius)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag("EnemyBuilding"))
            {
                if (go == null) continue;
                if (_revealedBuildings.Contains(go))
                {
                    // 이미 발견된 건물 — 항상 표시
                    foreach (var rnd in go.GetComponentsInChildren<Renderer>()) rnd.enabled = true;
                    foreach (var col in go.GetComponentsInChildren<Collider>())  col.enabled = true;
                    continue;
                }
                bool vis = false;
                foreach (var (p, r) in sources)
                    if (Vector3.Distance(go.transform.position, p) <= sightRadius) { vis = true; break; }
                if (vis) _revealedBuildings.Add(go); // 첫 발견 시 등록
                foreach (var rnd in go.GetComponentsInChildren<Renderer>()) rnd.enabled = vis;
                foreach (var col in go.GetComponentsInChildren<Collider>())  col.enabled = vis;
            }
        }

        private static void ApplyFog(string tag, List<(Vector3 p, float r)> sources, float sightRadius)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag(tag))
            {
                if (go == null) continue;
                bool vis = false;
                foreach (var (p, r) in sources)
                    if (Vector3.Distance(go.transform.position, p) <= sightRadius) { vis = true; break; }
                foreach (var rnd in go.GetComponentsInChildren<Renderer>())
                    rnd.enabled = vis;
                // 콜라이더도 비활성화해서 시야 밖 건물 클릭 방지
                foreach (var col in go.GetComponentsInChildren<Collider>())
                    col.enabled = vis;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  전투 입력 처리
        // ═══════════════════════════════════════════════════════
        private void HandleInput()
        {
            bool lmbDown = Input.GetMouseButtonDown(0);
            bool lmbHeld = Input.GetMouseButton(0);
            bool lmbUp   = Input.GetMouseButtonUp(0);
            bool rmb     = Input.GetMouseButtonDown(1);

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            // ── 수비 진형 구성: 건물 배치 선택 취소 (우클릭) ──────────
            if (_defenseSetupActive && _selectedPlaceBldg >= 0 && rmb)
            {
                _selectedPlaceBldg = -1;
                for (int i = 0; i < _dsPalBtns.Length; i++)
                    if (_dsPalBtns[i] != null)
                        _dsPalBtns[i].GetComponent<Image>().color = new Color(0.15f, 0.25f, 0.4f);
                SetDsStatus("선택 해제 — 건물을 선택하거나 유닛을 생산하세요");
                return;
            }

            // ── 수비 진형 구성: 지면 클릭 → 건물 배치 ────────────────
            if (_defenseSetupActive && _selectedPlaceBldg >= 0 && lmbDown && !overUI)
            {
                var pr = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(pr, out RaycastHit ph, 200f))
                {
                    // 성벽 칸 클릭 시 문 이동으로 처리
                    int wi = _wallSegments.IndexOf(ph.collider.gameObject);
                    if (wi >= 0) { MoveGate(wi); SetDsStatus($"문 위치 변경 → 칸 {wi}"); return; }

                    Vector3 pos = ph.point;
                    if (pos.x > -10.5f)
                        SetDsStatus("성벽 안쪽에만 배치 가능합니다");
                    else if (pos.x < -24f)
                        SetDsStatus("배치 가능 범위를 벗어났습니다");
                    else
                    {
                        int cost = _placeDefs[_selectedPlaceBldg].cost;
                        if (_gold < cost)
                            SetDsStatus($"골드 부족 (필요: {cost}G)");
                        else
                        {
                            _gold -= cost;
                            if (_selectedPlaceBldg == 0) PlacePlayerTower(pos);
                            else                          PlacePlayerWall(pos);
                            RefreshDsGold();
                            RefreshDsUnitBtns();
                            RefreshDsSpecBtns();
                        }
                    }
                }
                return;
            }

            // ── 수비 모드: 성벽 클릭 → 문 위치 변경 ─────────────────
            if (_defenseMode && lmbDown && !overUI && _wallSegments.Count > 0)
            {
                var wr = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(wr, out RaycastHit wh, 200f))
                {
                    int wi = _wallSegments.IndexOf(wh.collider.gameObject);
                    if (wi >= 0) { MoveGate(wi); SetInfo($"문 위치 변경 → 칸 {wi}"); return; }
                }
            }

            // ── 마법 시전 대기 ─────────────────────────────────
            if (_pendingSpell >= 0)
            {
                // 매 프레임: 마우스 위치에 범위 표시기 갱신
                if (!overUI && Camera.main != null)
                {
                    var rangeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(rangeRay, out RaycastHit rh, 200f))
                    {
                        float rad  = GetSpellIndicatorRadius(_pendingSpell);
                        bool global = rad < 0.1f;
                        bool vis   = _defenseMode || global || IsAreaRevealed(rh.point);
                        Color col  = SpellSystem.Defs[_pendingSpell].uiColor;
                        if (global) HideSpellRangeCircle();
                        else        UpdateSpellRangeCircle(rh.point, rad,
                                        vis ? col : new Color(0.55f, 0.1f, 0.1f));
                        string hint = vis ? $"[{SpellSystem.Defs[_pendingSpell].name}] — 위치를 드래그·클릭 후 놓으세요  (우클릭: 취소)"
                                          : "시야가 닿지 않은 지역 — 마법 사용 불가";
                        SetInfo(hint);
                    }
                    else HideSpellRangeCircle();
                }

                // 우클릭: 취소
                if (rmb)
                {
                    HideSpellRangeCircle();
                    _pendingSpell = -1;
                    SetInfo("마법 시전 취소");
                    return;
                }

                // 마우스 버튼 릴리즈(드래그 끝 or 단순 클릭): 시전
                if (lmbUp && !overUI && Camera.main != null)
                {
                    HideSpellRangeCircle();
                    var castRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(castRay, out RaycastHit ch, 200f))
                    {
                        bool global  = GetSpellIndicatorRadius(_pendingSpell) < 0.1f;
                        bool revealed = _defenseMode || global || IsAreaRevealed(ch.point);
                        if (revealed) CastSpell(_pendingSpell, ch.point);
                        else          SetInfo("시야가 닿지 않은 지역에는 마법을 사용할 수 없습니다");
                    }
                    _pendingSpell = -1;
                    return;
                }
                return;
            }

            // ── 드래그 선택 ─────────────────────────────────
            if (lmbDown && !overUI)
                _dragStart = Input.mousePosition;

            if (lmbHeld && !_isDragging && !overUI &&
                Vector2.Distance(Input.mousePosition, _dragStart) > 12f)
                _isDragging = true;

            if (_isDragging)
            {
                if (lmbUp)
                {
                    _isDragging = false;
                    if (_selectionBox != null) _selectionBox.SetActive(false);
                    FinalizeDrag(_dragStart, Input.mousePosition);
                }
                else UpdateDragBox();
                return;
            }

            if (!lmbDown && !rmb) return;
            if (overUI) return;

            var ray  = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray, 200f);
            if (hits.Length == 0) return;

            // 거리순 정렬 후 Unit/Building이 있는 히트를 우선 선택
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            RaycastHit hit = hits[0];
            foreach (var h in hits)
            {
                if (h.collider.GetComponentInParent<Unit>() != null ||
                    h.collider.GetComponentInParent<Building>() != null)
                {
                    hit = h;
                    break;
                }
            }

            var hitUnit = hit.collider.GetComponentInParent<Unit>();
            var hitAI   = hit.collider.GetComponentInParent<TestSimpleUnitAI>();
            var hitBldg = hit.collider.GetComponentInParent<Building>();

            if (lmbDown)
            {
                // 아군 유닛 클릭 → 선택 (다른 유닛이 선택된 상태에서도 전환 가능)
                if (hitAI != null && hitUnit != null && hitUnit.IsPlayerUnit)
                {
                    if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
                    Select(hitAI);
                    string hint = hitAI.AwaitingOrders ? "탭/우클릭으로 목표 지정" : "명령 수행 중";
                    SetInfo($"[{hitUnit.Data.unitName}] 선택됨 — {hint}");
                    return;
                }

                // 유닛이 선택된 상태 → 탭/클릭이 명령으로 동작 (터치 우클릭 대체)
                if (_selectedUnits.Count > 0)
                {
                    if ((hitUnit != null && !hitUnit.IsPlayerUnit) ||
                        (hitBldg != null && !hitBldg.IsPlayerBuilding))
                    {
                        foreach (var u in _selectedUnits) u.CommandAttack(hit.collider.transform);
                        SetInfo("공격 명령 발령 — 명령은 변경할 수 없습니다");
                        DeselectAll();
                        return;
                    }
                    // 빈 지면 → 이동 명령
                    Vector3 dest = hit.point;
                    int i = 0;
                    foreach (var u in _selectedUnits)
                    {
                        float off = (i % 3 - 1) * 2f;
                        u.CommandMove(dest + new Vector3(0, 0, off));
                        i++;
                    }
                    SetInfo("이동 명령 발령 — 명령은 변경할 수 없습니다");
                    DeselectAll();
                    return;
                }

                DeselectAll();
                return;
            }

            // 우클릭 — 명시적 명령 (마우스 전용, 기존 동작 유지)
            if (rmb && _selectedUnits.Count > 0)
            {
                if ((hitUnit != null && !hitUnit.IsPlayerUnit) ||
                    (hitBldg != null && !hitBldg.IsPlayerBuilding))
                {
                    foreach (var u in _selectedUnits) u.CommandAttack(hit.collider.transform);
                    SetInfo("공격 명령 발령 — 명령은 변경할 수 없습니다");
                }
                else
                {
                    Vector3 dest = hit.point;
                    int i = 0;
                    foreach (var u in _selectedUnits)
                    {
                        float off = (i % 3 - 1) * 2f;
                        u.CommandMove(dest + new Vector3(0, 0, off));
                        i++;
                    }
                    SetInfo("이동 명령 발령 — 명령은 변경할 수 없습니다");
                }
                DeselectAll();
            }
        }

        private void UpdateDragBox()
        {
            if (_selectionBox == null) return;
            Vector2 cur = Input.mousePosition;
            float scale = _canvas.GetComponent<Canvas>().scaleFactor;
            Vector2 min = Vector2.Min(_dragStart, cur) / scale;
            Vector2 max = Vector2.Max(_dragStart, cur) / scale;
            var rt = _selectionBox.GetComponent<RectTransform>();
            rt.anchoredPosition = min;
            rt.sizeDelta = max - min;
            _selectionBox.SetActive(true);
        }

        private void FinalizeDrag(Vector2 screenA, Vector2 screenB)
        {
            Vector2 min = Vector2.Min(screenA, screenB);
            Vector2 max = Vector2.Max(screenA, screenB);
            Rect rect = new Rect(min, max - min);
            DeselectAll();
            foreach (var u in _playerUnits)
            {
                if (u == null || !u.AwaitingOrders) continue;
                Vector3 sp = Camera.main.WorldToScreenPoint(u.transform.position);
                if (rect.Contains(sp)) Select(u);
            }
            int count = _selectedUnits.Count;
            SetInfo(count > 0
                ? $"{count}기 선택됨 — 우클릭으로 목표 지정 또는 방향 버튼"
                : "영역에 선택 가능한 유닛 없음");
        }

        // ── 유닛 종류 버튼 (전투 시작 후 동적 생성) ──────────────
        private void BuildUnitTypeButtons()
        {
            // 기존 버튼 제거
            foreach (Transform c in _unitTypeBar.transform) Destroy(c.gameObject);

            var types = new List<int>();
            for (int i = 0; i < Defs.Length; i++)
                if (_roster[i] > 0) types.Add(i);
            if (types.Count == 0) return;

            Lbl(_unitTypeBar, "TypeLbl", new Vector2(0f, 0.5f), new Vector2(52, 0),
                new Vector2(90, 40), "종류 선택:", 13, new Color(0.8f,0.8f,0.8f));

            float btnW = 130f, gap = 8f;
            float totalW = btnW * types.Count + gap * (types.Count - 1);
            float startX = 105f - totalW / 2f + btnW / 2f; // 라벨 오른쪽부터

            for (int ti = 0; ti < types.Count; ti++)
            {
                int idx = types[ti];
                float x = startX + ti * (btnW + gap);
                Color c = Defs[idx].color * 0.55f; c.a = 1f;
                Btn(_unitTypeBar, $"Type{idx}", new Vector2(0f, 0.5f),
                    new Vector2(x, 0), new Vector2(btnW, 48),
                    $"{Defs[idx].name}  ×{_roster[idx]}", c,
                    () => SelectUnitsByType(idx));
            }
        }

        private void SelectUnitsByType(int defIdx)
        {
            DeselectAll();
            string typeName = Defs[defIdx].name;
            foreach (var u in _playerUnits)
            {
                if (u == null || !u.AwaitingOrders) continue;
                var unit = u.GetComponent<Unit>();
                if (unit != null && unit.Data.unitName == typeName) Select(u);
            }
            int count = _selectedUnits.Count;
            SetInfo(count > 0
                ? $"{Defs[defIdx].name} {count}기 선택됨 — 우클릭 목표 지정 또는 방향 버튼"
                : $"대기 중인 {Defs[defIdx].name} 없음");
        }

        private void BuildSelectionBox()
        {
            _selectionBox = new GameObject("SelectionBox");
            _selectionBox.transform.SetParent(_canvas.transform, false);
            var img = _selectionBox.AddComponent<Image>();
            img.color = new Color(0.3f, 0.7f, 1f, 0.18f);
            var rt = _selectionBox.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            _selectionBox.SetActive(false);
        }

        private void Select(TestSimpleUnitAI ai)
        {
            if (_selectedUnits.Contains(ai)) return;
            _selectedUnits.Add(ai); ai.IsSelected = true;
        }

        private void DeselectAll()
        {
            foreach (var u in _selectedUnits) if (u != null) u.IsSelected = false;
            _selectedUnits.Clear();
        }

        // ═══════════════════════════════════════════════════════
        //  방향 버튼 (대기 중인 전체 병력 일괄 파견)
        // ═══════════════════════════════════════════════════════
        private void OrderAwaitingUnits(System.Action<TestSimpleUnitAI> cmd)
        {
            foreach (var u in _playerUnits)
                if (u != null && u.AwaitingOrders) cmd(u);
            SetInfo("명령 발령 완료");
        }

        // ═══════════════════════════════════════════════════════
        //  이벤트 처리
        // ═══════════════════════════════════════════════════════
        private void OnBuildingDestroyed(BuildingDestroyedEvent evt)
        {
            if (_phase == Phase.GameOver) return;
            var b = evt.Building;

            // 수비 모드: 플레이어 건물 파괴 처리
            if (_defenseMode && b.IsPlayerBuilding)
            {
                _destroyedBuildings++;
                RefreshStatPanel();
                ShowResourcePopup(b.transform.position, "건물 파괴!");
                if (b == _playerCastle) { EndGame(false, "아군 성 함락!"); return; }
                return;
            }

            if (b.IsPlayerBuilding) return;

            int gold  = b.Data.maxHp / 5;
            int valor = b == _enemyCastle ? 3 : 1;
            _gold         += gold;
            _valor        += valor;
            _earnedGold   += gold;
            _earnedValor  += valor;
            _destroyedBuildings++;

            RefreshUpgradeBtns();
            RefreshStatPanel();
            ShowResourcePopup(b.transform.position, $"+{gold}G  +{valor}무공");
            SetInfo($"건물 파괴! +{gold}G  +{valor} 무공");

            if (b == _enemyCastle) { EndGame(true, "적 성 점령!"); return; }

            // 남은 적 건물이 없으면 승리
            bool allGone = true;
            foreach (var eb in _allEnemyBuildings)
                if (eb != null && eb.IsAlive) { allGone = false; break; }
            if (allGone) EndGame(true, "완전 정복!");
        }

        private void EndGame(bool victory, string reason = "")
        {
            if (_phase == Phase.GameOver) return;
            _phase = Phase.GameOver;
            Time.timeScale = 0.25f;
            _resultPanel.SetActive(true);
            _resultText.text  = victory ? $"승리!\n{reason}" : $"패배\n{reason}";
            _resultText.color = victory ? Color.yellow : Color.red;
            if (_resultStatsText != null)
                _resultStatsText.text =
                    $"파괴 건물: {_destroyedBuildings}개\n" +
                    $"획득 골드: +{_earnedGold}G\n" +
                    $"획득 무공: +{_earnedValor}";
        }

        // ═══════════════════════════════════════════════════════
        //  기지 개발
        // ═══════════════════════════════════════════════════════
        private struct UpgDef { public string label; public int cost; public System.Action action; }

        private UpgDef[] GetUpgrades() => new[]
        {
            new UpgDef{label="🏇 기병 해금\nValor 1",  cost=1, action=()=>{ _unlocked.Add(4); }},
            new UpgDef{label="⚙ 공성기 해금\nValor 2", cost=2, action=()=>{ _unlocked.Add(5); }},
            new UpgDef{label="⚔ 전술 훈련\nValor 1",   cost=1, action=()=>{ _dmgMult += 0.3f; SetInfo("공격력 +30%!"); }},
            new UpgDef{label="🛡 방어 강화\nValor 2",   cost=2, action=()=>{ SetInfo("방어 강화 완료!"); }},
        };

        private void TryUpgrade(int idx)
        {
            var d = GetUpgrades();
            if (idx >= d.Length || _valor < d[idx].cost) return;
            _valor -= d[idx].cost;
            d[idx].action();
            RefreshUpgradeBtns();
        }

        // ═══════════════════════════════════════════════════════
        //  UI 구성
        // ═══════════════════════════════════════════════════════
        private void BuildUI()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
            }
            _canvas = new GameObject("Canvas");
            var cv = _canvas.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            _canvas.AddComponent<GraphicRaycaster>();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            BuildPrepPanel();
            BuildBattleHud();
            BuildUpgradePanel();
            BuildStatPanel();
            BuildResultPanel();
            BuildSelectionBox();
            BuildDefenseSetupHud();
            CreateSpellRangeCircle();

            _prepPanel.SetActive(true);
            _battleHud.SetActive(false);
            _upgradePanel.SetActive(false);
            _resultPanel.SetActive(false);
        }

        // ── 준비 화면 ─────────────────────────────────────────
        // 모든 요소: anchor=(0.5,1), pivot=(0.5,1) — 패널 상단 중앙 기준 Y 누적
        private void BuildPrepPanel()
        {
            _prepPanel = NewFillPanel(_canvas, "PrepPanel", new Color(0.04f, 0.04f, 0.10f, 0.93f));

            var a = new Vector2(0.5f, 1f); // 상단 중앙 앵커

            Lbl(_prepPanel, "Title", a, new Vector2(0, -38),
                new Vector2(480, 52), "⚔  전투 준비", 36, Color.white);

            _prepGoldText = Lbl(_prepPanel, "Gold", a, new Vector2(0, -100),
                new Vector2(260, 38), $"골드: {_gold}", 24, Color.yellow);

            // 공격/수비 모드 토글
            _modeToggleBtn = Btn(_prepPanel, "ModeToggle", a, new Vector2(0, -138),
                new Vector2(260, 36), "공격 모드 ▶", new Color(0.15f, 0.4f, 0.15f), ToggleMode);
            _modeToggleLbl = _modeToggleBtn.GetComponentInChildren<Text>();

            // 유닛 구매 버튼 — 3열 2행
            Vector2 btnSize = new Vector2(200, 90);
            float colW = 215f, startX = -215f;
            float row1Y = -210f, row2Y = -310f;

            for (int i = 0; i < Defs.Length; i++)
            {
                int idx = i;
                int col = i % 3, row = i / 3;
                float x = startX + col * colW;
                float y = row == 0 ? row1Y : row2Y;
                var b = Btn(_prepPanel, $"Buy{i}", a, new Vector2(x, y), btnSize,
                    BuildBuyLabel(i), BuyColor(i), () => BuyUnit(idx));
                _buyBtns[i]   = b;
                _buyLabels[i] = b.GetComponentInChildren<Text>();
            }

            // 마법 구매 섹션 — 공격 모드 전용 (컨테이너로 묶어서 한번에 토글)
            _spellSectionRoot = new GameObject("SpellSection");
            _spellSectionRoot.transform.SetParent(_prepPanel.transform, false);
            var ssRt = _spellSectionRoot.AddComponent<RectTransform>();
            ssRt.anchorMin = ssRt.anchorMax = ssRt.pivot = a;
            ssRt.anchoredPosition = Vector2.zero; ssRt.sizeDelta = Vector2.zero;

            Lbl(_spellSectionRoot, "SpellSec", a, new Vector2(0, -390),
                new Vector2(400, 30), "── 마법 구매 ──", 17, new Color(0.55f, 0.78f, 1f));

            float spellRow1Y = -458f, spellRow2Y = -560f;
            for (int i = 0; i < 5; i++)
            {
                int si = i;
                float sx = i < 3 ? startX + i * colW : startX + (i - 3) * colW + colW * 0.5f;
                float sy = i < 3 ? spellRow1Y : spellRow2Y;
                var sb = Btn(_spellSectionRoot, $"SpellBuy{i}", a, new Vector2(sx, sy), btnSize,
                    BuildSpellBuyLabel(i), SpellBuyColor(i), () => TryBuySpell(si));
                _spellBuyBtns[i] = sb;
                _spellBuyLbls[i] = sb.GetComponentInChildren<Text>();
            }

            _rosterText = Lbl(_prepPanel, "Roster", a, new Vector2(0, -632),
                new Vector2(720, 36), "병력 없음", 20, new Color(0.9f, 0.9f, 0.8f));
            if (_rosterText != null) _rosterText.alignment = TextAnchor.UpperCenter;

            _startBattleBtn = Btn(_prepPanel, "StartBtn", a, new Vector2(0, -681),
                new Vector2(250, 66), "출전! ▶", new Color(0.1f, 0.55f, 0.15f), EnterBattle);
            _enterSetupBtn = Btn(_prepPanel, "SetupBtn", a, new Vector2(0, -681),
                new Vector2(250, 66), "진형 구성 시작 ▶", new Color(0.55f, 0.3f, 0.05f), EnterDefenseSetup);

            // 우측 패널 — 특수 건물 (수비 모드 전용)
            _rightPanel = NewAnchoredPanel(_prepPanel, "PrepRightPanel",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-390, 50), new Vector2(-5, -5),
                new Color(0.06f, 0.06f, 0.18f, 0.92f));
            BuildSpecialBldgPanel(_rightPanel);

            // 초기 가시성: 공격 모드 기본
            ApplyModePrepVisibility();
        }

        private string BuildBuyLabel(int i)
        {
            var d = Defs[i];
            bool locked = i >= 4 && !BuildingEffectSystem.IsUnitUnlocked(i) && !_unlocked.Contains(i);
            int  effCost = Mathf.RoundToInt(d.cost * BuildingEffectSystem.GetCostMultiplier());
            string lockInfo = i == 4 ? "여인숙 Lv1+" : "여인숙 Lv2+";
            string sub = locked ? $"🔒 {lockInfo}" : $"{effCost}G";
            return $"[{d.name}]  {d.desc}\n{sub}   보유: {_roster[i]}";
        }

        private Color BuyColor(int i)
        {
            bool locked = i >= 4 && !BuildingEffectSystem.IsUnitUnlocked(i) && !_unlocked.Contains(i);
            if (locked) return new Color(0.22f, 0.14f, 0.14f);
            int effCost = Mathf.RoundToInt(Defs[i].cost * BuildingEffectSystem.GetCostMultiplier());
            return _gold >= effCost ? new Color(0.12f, 0.22f, 0.38f) : new Color(0.26f, 0.26f, 0.28f);
        }

        private void BuyUnit(int idx)
        {
            bool locked = idx >= 4 && !BuildingEffectSystem.IsUnitUnlocked(idx) && !_unlocked.Contains(idx);
            int  effCost = Mathf.RoundToInt(Defs[idx].cost * BuildingEffectSystem.GetCostMultiplier());
            if (locked || _gold < effCost) return;
            _gold -= effCost;
            _roster[idx]++;
            RefreshPrepGold();
            RefreshBuyBtns();
            RefreshSpecialBldgUI();
            RefreshSpellBuyUI();
            UpdateRosterText();
        }

        private void RefreshPrepGold()
        {
            if (_prepGoldText != null) _prepGoldText.text = $"골드: {_gold}";
        }

        private void RefreshBuyBtns()
        {
            for (int i = 0; i < Defs.Length; i++)
            {
                if (_buyLabels[i] != null) _buyLabels[i].text = BuildBuyLabel(i);
                if (_buyBtns[i]   != null) _buyBtns[i].GetComponent<Image>().color = BuyColor(i);
            }
        }

        private void UpdateRosterText()
        {
            var sb = new StringBuilder("보유 병력:  ");
            bool any = false;
            for (int i = 0; i < Defs.Length; i++)
            {
                if (_roster[i] <= 0) continue;
                if (any) sb.Append("   ");
                sb.Append($"{Defs[i].name} ×{_roster[i]}");
                any = true;
            }
            if (_rosterText != null) _rosterText.text = any ? sb.ToString() : "병력 없음";
        }

        // ── 특수 건물 패널 ─────────────────────────────────────
        private void BuildSpecialBldgPanel(GameObject parent)
        {
            var a = new Vector2(0.5f, 1f);
            Lbl(parent, "BldgTitle", a, new Vector2(0, -12), new Vector2(370, 28),
                "─── 특수 건물 업그레이드 ───", 15, new Color(0.95f, 0.85f, 0.5f));

            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                float y = -46f - i * 54f;
                var b = Btn(parent, $"SBldg{i}", a, new Vector2(0, y), new Vector2(370, 50),
                    BuildSpecialBldgLabel((SpecialBuildingType)i),
                    SpecialBldgColor((SpecialBuildingType)i),
                    () => TryUpgradeSpecialBuilding(idx));
                _specialBldgBtns[i] = b;
                _specialBldgLbls[i] = b.GetComponentInChildren<Text>();
            }
        }

        private string BuildSpecialBldgLabel(SpecialBuildingType t)
        {
            int lv = BuildingEffectSystem.GetLevel(t);
            bool maxed = lv >= BuildingEffectSystem.MaxLevel;
            bool unlocked = BuildingEffectSystem.IsBuildingUnlocked(t);
            string levelStr = maxed ? "Lv MAX" : (unlocked ? $"Lv {lv} → {lv + 1}" : "🔒 공방 필요");
            string costStr  = (maxed || !unlocked) ? "" : $"  |  {BuildingEffectSystem.GetUpgradeCost(t)}G";
            return $"[{BuildingEffectSystem.Names[(int)t]}] {BuildingEffectSystem.EffectDescs[(int)t]}\n{levelStr}{costStr}";
        }

        private Color SpecialBldgColor(SpecialBuildingType t)
        {
            if (!BuildingEffectSystem.IsBuildingUnlocked(t)) return new Color(0.14f, 0.10f, 0.10f);
            int lv = BuildingEffectSystem.GetLevel(t);
            if (lv >= BuildingEffectSystem.MaxLevel) return new Color(0.12f, 0.24f, 0.12f);
            return _gold >= BuildingEffectSystem.GetUpgradeCost(t)
                ? new Color(0.18f, 0.22f, 0.42f)
                : new Color(0.18f, 0.18f, 0.26f);
        }

        private void TryUpgradeSpecialBuilding(int idx)
        {
            var type = (SpecialBuildingType)idx;
            if (!BuildingEffectSystem.IsBuildingUnlocked(type)) return;
            if (!BuildingEffectSystem.CanUpgrade(type)) return;
            int cost = BuildingEffectSystem.GetUpgradeCost(type);
            if (_gold < cost) return;
            _gold -= cost;
            BuildingEffectSystem.Upgrade(type);
            RefreshPrepGold();
            RefreshSpecialBldgUI();
            RefreshBuyBtns();     // Blacksmith 변경 → 유닛 비용 표시 갱신
            RefreshSpellBuyUI();  // 골드 변경
        }

        private void RefreshSpecialBldgUI()
        {
            for (int i = 0; i < 6; i++)
            {
                var t = (SpecialBuildingType)i;
                if (_specialBldgLbls[i] != null) _specialBldgLbls[i].text  = BuildSpecialBldgLabel(t);
                if (_specialBldgBtns[i] != null) _specialBldgBtns[i].GetComponent<Image>().color = SpecialBldgColor(t);
            }
        }

        private string BuildSpellBuyLabel(int i)
        {
            var def = SpellSystem.Defs[i];
            int charges = SpellSystem.GetCharges((SpellType)i);
            bool maxed  = charges >= def.maxCharges;
            string chargeStr = maxed ? "MAX" : $"{charges}/{def.maxCharges}회";
            return $"[{def.name}]  {def.desc}\n보유: {chargeStr}";
        }

        private Color SpellBuyColor(int i)
        {
            var def = SpellSystem.Defs[i];
            if (SpellSystem.GetCharges((SpellType)i) >= def.maxCharges) return new Color(0.12f, 0.24f, 0.12f);
            Color c = def.uiColor * 0.5f; c.a = 1f;
            return c;
        }

        private void TryBuySpell(int si)
        {
            var type = (SpellType)si;
            if (!SpellSystem.CanBuyMore(type) || _gold < SpellSystem.BuyCost(type)) return;
            SpellSystem.TryBuy(type, ref _gold);
            RefreshPrepGold();
            RefreshSpellBuyUI();
            RefreshSpecialBldgUI();
            RefreshBuyBtns();
        }

        private void RefreshSpellBuyUI()
        {
            for (int i = 0; i < 5; i++)
            {
                if (_spellBuyLbls[i] != null) _spellBuyLbls[i].text = BuildSpellBuyLabel(i);
                if (_spellBuyBtns[i] != null) _spellBuyBtns[i].GetComponent<Image>().color = SpellBuyColor(i);
            }
        }

        // ── 마법 전투 ──────────────────────────────────────────
        private string BuildSpellBattleLabel(int i)
        {
            int charges = SpellSystem.GetCharges((SpellType)i);
            return $"{SpellSystem.Defs[i].name}\n({charges}회)";
        }

        private void ActivateSpell(int si)
        {
            if (!SpellSystem.HasCharge((SpellType)si)) return;
            _pendingSpell = si;
            SetInfo($"[{SpellSystem.Defs[si].name}] — 시전할 위치를 클릭하세요  (우클릭: 취소)");
        }

        private void RefreshSpellBattleBtns()
        {
            for (int i = 0; i < 5; i++)
            {
                bool hasCharge = SpellSystem.HasCharge((SpellType)i);
                if (_spellBattleChargeLbls[i] != null)
                    _spellBattleChargeLbls[i].text = BuildSpellBattleLabel(i);
                if (_spellBattleBtns[i] != null)
                {
                    Color c = SpellSystem.Defs[i].uiColor * (hasCharge ? 0.55f : 0.2f); c.a = 1f;
                    _spellBattleBtns[i].GetComponent<Image>().color = c;
                }
            }
        }

        private void CastSpell(int si, Vector3 worldPos)
        {
            if (!SpellSystem.UseCharge((SpellType)si)) return;
            switch ((SpellType)si)
            {
                case SpellType.Fireball:
                    foreach (var go in GameObject.FindGameObjectsWithTag("EnemyUnit"))
                        if (Vector3.Distance(go.transform.position, worldPos) <= 3f)
                            go.GetComponent<Unit>()?.TakeDamage(120);
                    foreach (var go in GameObject.FindGameObjectsWithTag("EnemyBuilding"))
                        if (Vector3.Distance(go.transform.position, worldPos) <= 3f)
                            go.GetComponent<Building>()?.TakeDamage(120);
                    ShowResourcePopup(worldPos, "화염구!");
                    break;

                case SpellType.Lightning:
                    Transform ltTarget = null; float ltDist = float.MaxValue;
                    foreach (var go in GameObject.FindGameObjectsWithTag("EnemyUnit"))
                    {
                        float d = Vector3.Distance(go.transform.position, worldPos);
                        if (d < ltDist) { ltDist = d; ltTarget = go.transform; }
                    }
                    foreach (var go in GameObject.FindGameObjectsWithTag("EnemyBuilding"))
                    {
                        float d = Vector3.Distance(go.transform.position, worldPos);
                        if (d < ltDist) { ltDist = d; ltTarget = go.transform; }
                    }
                    if (ltTarget != null)
                    {
                        ltTarget.GetComponent<Unit>()?.TakeDamage(200);
                        ltTarget.GetComponent<Building>()?.TakeDamage(200);
                        ShowResourcePopup(ltTarget.position, "번개!");
                    }
                    break;

                case SpellType.Heal:
                    TestSimpleUnitAI healTarget = null; float healDist = float.MaxValue;
                    foreach (var ai in _playerUnits)
                    {
                        if (ai == null) continue;
                        float d = Vector3.Distance(ai.transform.position, worldPos);
                        if (d < healDist) { healDist = d; healTarget = ai; }
                    }
                    if (healTarget != null)
                    {
                        healTarget.GetComponent<Unit>()?.Heal(300);
                        ShowResourcePopup(healTarget.transform.position, "치유!");
                    }
                    break;

                case SpellType.Freeze:
                    StartCoroutine(FreezeEnemies(5f));
                    ShowResourcePopup(worldPos, "빙결!");
                    break;

                case SpellType.Rage:
                    StartCoroutine(RagePlayerUnits(8f));
                    ShowResourcePopup(worldPos, "분노!");
                    break;
            }
            RefreshSpellBattleBtns();
            SetInfo($"[{SpellSystem.Defs[si].name}] 시전 완료");
        }

        private IEnumerator FreezeEnemies(float duration)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag("EnemyUnit"))
            {
                var ai = go.GetComponent<TestSimpleUnitAI>();
                if (ai != null) ai.SpeedMultiplier = 0.5f;
            }
            yield return new WaitForSeconds(duration);
            foreach (var go in GameObject.FindGameObjectsWithTag("EnemyUnit"))
            {
                var ai = go.GetComponent<TestSimpleUnitAI>();
                if (ai != null) ai.SpeedMultiplier = 1f;
            }
        }

        private IEnumerator RagePlayerUnits(float duration)
        {
            foreach (var ai in _playerUnits) if (ai != null) ai.DamageMultiplier += 0.5f;
            yield return new WaitForSeconds(duration);
            foreach (var ai in _playerUnits) if (ai != null) ai.DamageMultiplier = Mathf.Max(1f, ai.DamageMultiplier - 0.5f);
        }

        // ── 전투 HUD ──────────────────────────────────────────
        private void BuildBattleHud()
        {
            _battleHud = new GameObject("BattleHud");
            _battleHud.transform.SetParent(_canvas.transform, false);
            var rt = _battleHud.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            // 상단 바 배경
            var topBar = NewAnchoredPanel(_battleHud, "TopBar",
                new Vector2(0,1), new Vector2(1,1), new Vector2(0,-48), Vector2.zero,
                new Color(0,0,0,0.55f));

            var tl = new Vector2(0f, 1f);
            var tc = new Vector2(0.5f, 1f);
            var tr = new Vector2(1f, 1f);

            _timerText    = Lbl(topBar, "Timer",   tc, new Vector2(-60,-7), new Vector2(130,34), "⏱ 0s",    20, Color.white);
            _valorHudText = Lbl(topBar, "Valor",   tl, new Vector2(10, -7), new Vector2(160,34), "무공: 0",  20, new Color(1f,0.8f,0.2f));
            _enemyHpText  = Lbl(topBar, "EnemyHp", tr, new Vector2(-10,-7), new Vector2(220,34), "적성: 900",20, new Color(1f,0.4f,0.4f));
            if (_enemyHpText != null) _enemyHpText.alignment = TextAnchor.UpperRight;

            // 유닛 종류 선택 바 (동적 버튼은 DeployArmy에서 생성)
            _unitTypeBar = NewAnchoredPanel(_battleHud, "UnitTypeBar",
                new Vector2(0,0), new Vector2(1,0),
                new Vector2(0,118), new Vector2(0,178),
                new Color(0,0,0,0.4f));

            // 마법 빠른 시전 바
            var spellBar = NewAnchoredPanel(_battleHud, "SpellBar",
                new Vector2(0,0), new Vector2(1,0),
                new Vector2(0,178), new Vector2(0,232),
                new Color(0.02f,0.04f,0.18f,0.88f));

            Lbl(spellBar, "SpellLbl", new Vector2(0f,0.5f), new Vector2(46,0),
                new Vector2(82,44), "마법:", 15, new Color(0.7f,0.8f,1f));

            float spellBtnW = 150f, spellGap = 8f;
            float spellTotalW = spellBtnW * 5 + spellGap * 4;
            for (int i = 0; i < 5; i++)
            {
                int si = i;
                float x = -spellTotalW / 2f + spellBtnW / 2f + i * (spellBtnW + spellGap) + 50f;
                Color sc = SpellSystem.Defs[i].uiColor * 0.35f; sc.a = 1f;
                var sb = Btn(spellBar, $"SpellB{i}", new Vector2(0.5f,0.5f),
                    new Vector2(x, 0), new Vector2(spellBtnW, 44),
                    BuildSpellBattleLabel(i), sc, () => ActivateSpell(si));
                _spellBattleBtns[i]      = sb;
                _spellBattleChargeLbls[i] = sb.GetComponentInChildren<Text>();
            }

            // 하단 안내 + 방향 버튼 배경
            var botBar = NewAnchoredPanel(_battleHud, "BotBar",
                new Vector2(0,0), new Vector2(1,0), Vector2.zero, new Vector2(0,118),
                new Color(0,0,0,0.55f));

            _infoText = Lbl(botBar, "Info", new Vector2(0.5f,1), new Vector2(0,-6),
                new Vector2(800,28), "", 16, new Color(0.9f,0.9f,0.7f));
            if (_infoText != null) _infoText.alignment = TextAnchor.UpperCenter;

            // 방향 버튼 7개 (대기 유닛 일괄 파견)
            var tactics = new (string lbl, Color col, System.Action act)[]
            {
                ("전군 선택",   new Color(0.2f,0.35f,0.55f), ()=>{ DeselectAll(); foreach(var u in _playerUnits) if(u!=null&&u.AwaitingOrders) Select(u); }),
                ("전군 돌격",   new Color(0.65f,0.1f,0.1f),  ()=> OrderAwaitingUnits(u=>u.CommandAttack(_enemyCastle?.transform))),
                ("좌측 공격",   new Color(0.2f,0.4f,0.22f),  ()=> OrderAwaitingUnits(u=>u.CommandMove(new Vector3(6,0, 7)))),
                ("중앙 돌파",   new Color(0.2f,0.4f,0.22f),  ()=> OrderAwaitingUnits(u=>u.CommandMove(new Vector3(6,0, 0)))),
                ("우측 공격",   new Color(0.2f,0.4f,0.22f),  ()=> OrderAwaitingUnits(u=>u.CommandMove(new Vector3(6,0,-7)))),
                ("선택→목표",  new Color(0.35f,0.25f,0.5f),  ()=> SetInfo("우클릭으로 목표를 지정하세요")),
                ("전군 자동",   new Color(0.3f,0.3f,0.3f),   ()=> OrderAwaitingUnits(u=>u.CommandAttack(null))),
            };

            float btnW = 130f, totalW = btnW * tactics.Length + 10f * (tactics.Length - 1);
            for (int i = 0; i < tactics.Length; i++)
            {
                var (lbl, col, act) = tactics[i];
                float x = -totalW / 2f + btnW / 2f + i * (btnW + 10f);
                Btn(botBar, $"Tac{i}", new Vector2(0.5f,0), new Vector2(x, 10),
                    new Vector2(btnW, 72), lbl, col, act);
            }
        }

        // ── 기지 개발 패널 (우측) ─────────────────────────────
        private void BuildUpgradePanel()
        {
            _upgradePanel = NewAnchoredPanel(_canvas, "UpgradePanel",
                new Vector2(1,0), new Vector2(1,1),
                new Vector2(-185,118), Vector2.zero,
                new Color(0.05f,0.05f,0.12f,0.88f));

            Lbl(_upgradePanel, "Title", new Vector2(0.5f,1), new Vector2(0,-14),
                new Vector2(175,32), "기지 개발", 18, Color.white);

            var defs = GetUpgrades();
            for (int i = 0; i < defs.Length; i++)
            {
                int idx = i;
                _upgBtns[i] = Btn(_upgradePanel, $"Upg{i}", new Vector2(0.5f,1),
                    new Vector2(0, -54 - i * 88), new Vector2(168,80),
                    defs[i].label, new Color(0.18f,0.18f,0.28f), ()=>TryUpgrade(idx));
            }
            RefreshUpgradeBtns();
        }

        private void RefreshUpgradeBtns()
        {
            var d = GetUpgrades();
            for (int i = 0; i < _upgBtns.Length; i++)
            {
                if (_upgBtns[i] == null) continue;
                _upgBtns[i].GetComponent<Image>().color =
                    _valor >= d[i].cost ? new Color(0.18f,0.38f,0.55f) : new Color(0.18f,0.18f,0.28f);
            }
            if (_valorHudText != null) _valorHudText.text = $"무공: {_valor}";
        }

        // ── 자원 현황 패널 (배틀 중 우측 하단) ───────────────────
        private void BuildStatPanel()
        {
            var panel = NewAnchoredPanel(_battleHud, "StatPanel",
                new Vector2(1,0), new Vector2(1,0),
                new Vector2(-185, 178), new Vector2(0, 310),
                new Color(0.04f, 0.06f, 0.14f, 0.85f));

            Lbl(panel, "Title", new Vector2(0.5f,1), new Vector2(0,-10),
                new Vector2(175,26), "전투 획득", 15, Color.white);

            _statGoldText  = Lbl(panel, "Gold",  new Vector2(0.5f,1), new Vector2(0,-42),
                new Vector2(175,28), "골드   +0G",        16, new Color(1f,0.95f,0.4f));
            _statValorText = Lbl(panel, "Valor", new Vector2(0.5f,1), new Vector2(0,-74),
                new Vector2(175,28), "무공   +0",         16, new Color(0.6f,0.9f,1f));
            _statBldgText  = Lbl(panel, "Bldg",  new Vector2(0.5f,1), new Vector2(0,-106),
                new Vector2(175,28), "파괴   0개",        16, new Color(1f,0.6f,0.4f));
        }

        private void RefreshStatPanel()
        {
            if (_statGoldText  != null) _statGoldText.text  = $"골드   +{_earnedGold}G";
            if (_statValorText != null) _statValorText.text = $"무공   +{_earnedValor}";
            if (_statBldgText  != null) _statBldgText.text  = $"파괴   {_destroyedBuildings}개";
        }

        // ── 자원 획득 팝업 (플로팅 텍스트) ───────────────────────
        private void ShowResourcePopup(Vector3 worldPos, string text)
        {
            var go = new GameObject("Popup"); go.transform.SetParent(_canvas.transform, false);
            var t = go.AddComponent<Text>();
            t.font = _font; t.fontSize = 22; t.color = Color.yellow;
            t.text = text; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(220, 40);
            float scale = _canvas.GetComponent<Canvas>().scaleFactor;
            rt.anchoredPosition = (Vector2)Camera.main.WorldToScreenPoint(worldPos) / scale;
            StartCoroutine(FloatAndFade(go, t));
        }

        private IEnumerator FloatAndFade(GameObject go, Text t)
        {
            float dur = 1.6f, elapsed = 0f;
            var rt = t.rectTransform;
            Vector2 startPos = rt.anchoredPosition;
            while (elapsed < dur)
            {
                if (go == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                rt.anchoredPosition = startPos + Vector2.up * (80f * p);
                t.color = new Color(1f, 0.9f - 0.5f * p, 0.2f, 1f - p);
                yield return null;
            }
            Destroy(go);
        }

        // ── 결과 패널 ─────────────────────────────────────────
        private void BuildResultPanel()
        {
            _resultPanel = new GameObject("ResultPanel");
            _resultPanel.transform.SetParent(_canvas.transform, false);
            _resultPanel.AddComponent<Image>().color = new Color(0,0,0,0.9f);
            var rt = _resultPanel.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f,0.5f);
            rt.sizeDelta = new Vector2(480,250); rt.anchoredPosition = Vector2.zero;

            _resultText = Lbl(_resultPanel, "Msg", new Vector2(0.5f,0.5f),
                new Vector2(0,60), new Vector2(460,90), "", 44, Color.white);
            if (_resultText != null) _resultText.alignment = TextAnchor.MiddleCenter;

            _resultStatsText = Lbl(_resultPanel, "Stats", new Vector2(0.5f,0.5f),
                new Vector2(0,-10), new Vector2(380,80), "", 18, new Color(0.9f,0.9f,0.7f));
            if (_resultStatsText != null) _resultStatsText.alignment = TextAnchor.MiddleCenter;

            Btn(_resultPanel, "Retry", new Vector2(0.5f,0.5f), new Vector2(-80,-80),
                new Vector2(140,52), "다시 시작", new Color(0.1f,0.5f,0.1f), ()=>{
                    Time.timeScale = 1f;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                });
            Btn(_resultPanel, "Quit", new Vector2(0.5f,0.5f), new Vector2(80,-80),
                new Vector2(140,52), "종료", new Color(0.5f,0.1f,0.1f), Application.Quit);
        }

        // ═══════════════════════════════════════════════════════
        //  UI 헬퍼
        // ═══════════════════════════════════════════════════════
        private void SetInfo(string msg) { if (_infoText != null) _infoText.text = msg; }

        private static void Paint(GameObject go, Color c)
        {
            var s = Shader.Find("Universal Render Pipeline/Lit")
                 ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                 ?? Shader.Find("Standard") ?? Shader.Find("Legacy Shaders/Diffuse");
            go.GetComponent<Renderer>().material = new Material(s) { color = c };
        }

        private GameObject NewFillPanel(GameObject parent, string name, Color bg)
        {
            var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>().color = bg;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        // stretch 방식 패널 (offsetMin/offsetMax)
        private GameObject NewAnchoredPanel(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color bg)
        {
            var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>().color = bg;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            return go;
        }

        private Text Lbl(GameObject parent, string name, Vector2 anchor,
            Vector2 pos, Vector2 size, string text, int fs, Color col)
        {
            var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
            var t = go.AddComponent<Text>();
            t.font = _font; t.fontSize = fs; t.color = col; t.text = text; t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            return t;
        }

        private Button Btn(GameObject parent, string name, Vector2 anchor,
            Vector2 pos, Vector2 size, string label, Color bg, System.Action onClick)
        {
            var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>().color = bg;
            var btn = go.AddComponent<Button>(); btn.onClick.AddListener(()=>onClick());
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos; rt.sizeDelta = size;

            var tgo = new GameObject("L"); tgo.transform.SetParent(go.transform, false);
            var t = tgo.AddComponent<Text>();
            t.font = _font; t.text = label; t.fontSize = 14;
            t.color = Color.white; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false;
            var trt = t.rectTransform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.sizeDelta = Vector2.zero;
            return btn;
        }

        // ═══════════════════════════════════════════════════════
        //  수비 진형 구성
        // ═══════════════════════════════════════════════════════
        private void EnterDefenseSetup()
        {
            if (_defenseSetupActive) return;
            _defenseSetupActive  = true;
            _selectedPlaceBldg   = -1;
            _stagingCol = _stagingRow = 0;

            // 기본 기지: 성 + 자동 성벽
            _playerCastle = MakePlayerBuilding("PlayerCastle",
                new Vector3(-21f, 1.5f, 0f), 900,
                new Color(0.2f, 0.35f, 0.75f), new Vector3(4f, 3f, 4f));
            GenerateAutoWall(-10f, -8f, 8f);

            // 카메라를 플레이어 구역 중심으로 이동
            var cam = Camera.main;
            if (cam != null)
                cam.transform.SetPositionAndRotation(
                    new Vector3(-10f, 28f, -22f), Quaternion.Euler(50f, 0f, 0f));

            _prepPanel.SetActive(false);
            if (_dsHud != null) _dsHud.SetActive(true);

            RefreshDsGold();
            RefreshDsUnitBtns();
            RefreshDsSpecBtns();
            SetDsStatus("방어탑/성벽을 선택한 뒤 성벽 안쪽을 클릭해 배치  |  유닛 버튼으로 즉시 생산  |  우클릭: 배치 취소");
        }

        private void BuildDefenseSetupHud()
        {
            _dsHud = new GameObject("DefenseSetupHud");
            _dsHud.transform.SetParent(_canvas.transform, false);
            var rt = _dsHud.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            var a = new Vector2(0.5f, 1f);

            // ── 왼쪽 패널: 건물 배치 + 유닛 생산 ──────────────────────
            var left = NewAnchoredPanel(_dsHud, "DS_Left",
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(5f, 5f), new Vector2(222f, -5f),
                new Color(0.04f, 0.06f, 0.14f, 0.93f));

            Lbl(left, "Title", a, new Vector2(0f, -16f), new Vector2(212f, 28f),
                "수비 진형 구성", 16, Color.white);
            _dsGoldText = Lbl(left, "Gold", a, new Vector2(0f, -44f), new Vector2(212f, 22f),
                $"골드: {_gold}", 14, Color.yellow);

            // 건물 배치 팔레트
            Lbl(left, "BldgHdr", a, new Vector2(0f, -72f), new Vector2(212f, 18f),
                "── 건물 배치 ──", 11, new Color(0.7f, 0.85f, 1f));

            for (int i = 0; i < _placeDefs.Length; i++)
            {
                int idx = i;
                float py = -94f - i * 46f;
                var pb = Btn(left, $"Pal{i}", a, new Vector2(0f, py), new Vector2(208f, 42f),
                    $"{_placeDefs[i].label}  {_placeDefs[i].cost}G",
                    new Color(0.15f, 0.25f, 0.4f), () => SelectPlaceBldg(idx));
                _dsPalBtns[i] = pb;
                _dsPalLbls[i] = pb.GetComponentInChildren<Text>();
            }

            // 유닛 생산
            float uHdrY = -94f - _placeDefs.Length * 46f - 10f;
            Lbl(left, "UnitHdr", a, new Vector2(0f, uHdrY), new Vector2(212f, 18f),
                "── 유닛 생산 ──", 11, new Color(0.7f, 1f, 0.7f));

            float uStartY = uHdrY - 24f;
            for (int i = 0; i < Defs.Length; i++)
            {
                int idx = i;
                int col = i % 2, row = i / 2;
                float ux = col == 0 ? -52f : 52f;
                float uy = uStartY - row * 50f;
                var ub = Btn(left, $"DSUnit{i}", a, new Vector2(ux, uy), new Vector2(99f, 46f),
                    DsUnitLabel(i), new Color(0.12f, 0.22f, 0.38f), () => BuyUnitDefenseSetup(idx));
                _dsUnitBtns[i] = ub;
                _dsUnitLbls[i] = ub.GetComponentInChildren<Text>();
            }

            // 전투 시작 (하단 고정)
            Btn(left, "BattleStart", new Vector2(0.5f, 0f), new Vector2(0f, 8f),
                new Vector2(208f, 52f), "전투 시작 ▶",
                new Color(0.6f, 0.12f, 0.12f), EnterBattle);

            // ── 오른쪽 패널: 특수 건물 업그레이드 ─────────────────────
            var right = NewAnchoredPanel(_dsHud, "DS_Right",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-222f, 5f), new Vector2(-5f, -5f),
                new Color(0.06f, 0.06f, 0.18f, 0.93f));

            Lbl(right, "SpecHdr", a, new Vector2(0f, -12f), new Vector2(208f, 24f),
                "── 특수 건물 업그레이드 ──", 13, new Color(0.95f, 0.85f, 0.5f));

            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                float sy = -42f - i * 54f;
                var sb = Btn(right, $"DSSpec{i}", a, new Vector2(0f, sy), new Vector2(208f, 50f),
                    BuildSpecialBldgLabel((SpecialBuildingType)i),
                    SpecialBldgColor((SpecialBuildingType)i),
                    () => TryUpgradeSpecialBuildingDs(idx));
                _dsSpecBtns[i] = sb;
                _dsSpecLbls[i] = sb.GetComponentInChildren<Text>();
            }

            // ── 하단 상태 바 ───────────────────────────────────────────
            var bot = NewAnchoredPanel(_dsHud, "DS_Bot",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(228f, 0f), new Vector2(-228f, 34f),
                new Color(0f, 0f, 0f, 0.6f));
            _dsStatusText = Lbl(bot, "Status", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(800f, 30f), "", 13, new Color(0.9f, 0.9f, 0.7f));
            if (_dsStatusText != null) _dsStatusText.alignment = TextAnchor.MiddleCenter;

            _dsHud.SetActive(false);
        }

        private void SelectPlaceBldg(int idx)
        {
            _selectedPlaceBldg = (_selectedPlaceBldg == idx) ? -1 : idx;
            for (int i = 0; i < _dsPalBtns.Length; i++)
            {
                if (_dsPalBtns[i] == null) continue;
                bool sel = (i == _selectedPlaceBldg);
                _dsPalBtns[i].GetComponent<Image>().color = sel
                    ? new Color(0.4f, 0.55f, 1f)
                    : (_gold >= _placeDefs[i].cost ? new Color(0.15f, 0.25f, 0.4f) : new Color(0.22f, 0.22f, 0.26f));
            }
            SetDsStatus(_selectedPlaceBldg >= 0
                ? $"[{_placeDefs[_selectedPlaceBldg].label}] 선택됨 — 성벽 안쪽 클릭으로 배치  |  우클릭: 취소"
                : "선택 해제");
        }

        private void BuyUnitDefenseSetup(int idx)
        {
            bool locked = idx >= 4 && !BuildingEffectSystem.IsUnitUnlocked(idx) && !_unlocked.Contains(idx);
            int effCost = Mathf.RoundToInt(Defs[idx].cost * BuildingEffectSystem.GetCostMultiplier());
            if (locked || _gold < effCost) { SetDsStatus("골드 부족 또는 잠금 상태"); return; }
            _gold -= effCost;
            _roster[idx]++;

            // 성 뒤쪽 스테이징 구역에 즉시 배치
            float x = -19f - _stagingCol * 2.2f;
            float z = (_stagingRow - 2) * 2.4f;
            var ai = SpawnUnit(idx, true, new Vector3(x, 0f, z));
            ai.SetAwaitingOrders();
            _playerUnits.Add(ai);
            if (++_stagingRow >= 5) { _stagingRow = 0; _stagingCol++; }

            RefreshDsGold();
            RefreshDsUnitBtns();
            SetDsStatus($"{Defs[idx].name} 생산 완료 — 황색 링 유닛을 클릭 후 우클릭으로 이동/공격 명령 가능");
        }

        private void PlacePlayerTower(Vector3 hitPos)
        {
            hitPos.y = 1f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PTower_Custom";
            go.transform.position = hitPos;
            go.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            Paint(go, new Color(0.2f, 0.35f, 0.75f));
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = "방어탑"; data.maxHp = 220;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: true);
            _allPlayerBuildings.Add(b);
            go.AddComponent<TestTowerAI>().Setup(isPlayer: true, range: 9f, dmg: 18, cooldown: 1.2f);
            SetDsStatus($"방어탑 배치 완료  |  남은 골드: {_gold}G");
        }

        private void PlacePlayerWall(Vector3 hitPos)
        {
            hitPos.y = 1f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PCustomWall";
            go.transform.position = hitPos;
            go.transform.localScale = new Vector3(0.8f, 2f, 2.3f);
            Paint(go, new Color(0.32f, 0.32f, 0.36f));
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = "성벽"; data.maxHp = 400;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: true);
            _allPlayerBuildings.Add(b);
            SetDsStatus($"성벽 배치 완료  |  남은 골드: {_gold}G");
        }

        private void TryUpgradeSpecialBuildingDs(int idx)
        {
            TryUpgradeSpecialBuilding(idx);
            RefreshDsGold();
            RefreshDsSpecBtns();
        }

        private void RefreshDsGold()
        {
            if (_dsGoldText != null) _dsGoldText.text = $"골드: {_gold}";
        }

        private void RefreshDsUnitBtns()
        {
            for (int i = 0; i < Defs.Length; i++)
            {
                if (_dsUnitLbls[i] != null) _dsUnitLbls[i].text = DsUnitLabel(i);
                if (_dsUnitBtns[i] == null) continue;
                bool locked = i >= 4 && !BuildingEffectSystem.IsUnitUnlocked(i) && !_unlocked.Contains(i);
                int effCost = Mathf.RoundToInt(Defs[i].cost * BuildingEffectSystem.GetCostMultiplier());
                _dsUnitBtns[i].GetComponent<Image>().color = locked
                    ? new Color(0.2f, 0.12f, 0.12f)
                    : (_gold >= effCost ? new Color(0.12f, 0.22f, 0.38f) : new Color(0.22f, 0.22f, 0.26f));
            }
        }

        private void RefreshDsSpecBtns()
        {
            for (int i = 0; i < 6; i++)
            {
                var t = (SpecialBuildingType)i;
                if (_dsSpecLbls[i] != null) _dsSpecLbls[i].text  = BuildSpecialBldgLabel(t);
                if (_dsSpecBtns[i] != null) _dsSpecBtns[i].GetComponent<Image>().color = SpecialBldgColor(t);
            }
        }

        private void SetDsStatus(string msg)
        {
            if (_dsStatusText != null) _dsStatusText.text = msg;
        }

        private string DsUnitLabel(int i)
        {
            var d = Defs[i];
            bool locked = i >= 4 && !BuildingEffectSystem.IsUnitUnlocked(i) && !_unlocked.Contains(i);
            int effCost = Mathf.RoundToInt(d.cost * BuildingEffectSystem.GetCostMultiplier());
            string costStr = locked ? "잠금" : $"{effCost}G";
            return $"[{d.name}]\n{costStr} 보유:{_roster[i]}";
        }

        // ═══════════════════════════════════════════════════════
        //  마법 범위 표시기 & FOW 셀 추적
        // ═══════════════════════════════════════════════════════
        private void CreateSpellRangeCircle()
        {
            var go = new GameObject("SpellRangeCircle");
            _spellRangeCircle = go.AddComponent<LineRenderer>();
            _spellRangeCircle.useWorldSpace = true;
            _spellRangeCircle.loop         = true;
            _spellRangeCircle.positionCount = 48;
            _spellRangeCircle.startWidth   = 0.18f;
            _spellRangeCircle.endWidth     = 0.18f;
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Unlit/Color")
                      ?? Shader.Find("Standard");
            _spellRangeCircle.material = new Material(shader);
            go.SetActive(false);
        }

        private void UpdateSpellRangeCircle(Vector3 center, float radius, Color col)
        {
            if (_spellRangeCircle == null) return;
            _spellRangeCircle.gameObject.SetActive(true);
            center.y = 0.12f;
            int pts = _spellRangeCircle.positionCount;
            for (int i = 0; i < pts; i++)
            {
                float ang = i * 2f * Mathf.PI / pts;
                _spellRangeCircle.SetPosition(i,
                    center + new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius));
            }
            _spellRangeCircle.startColor = _spellRangeCircle.endColor = col;
            _spellRangeCircle.material.color = col;
        }

        private void HideSpellRangeCircle()
        {
            if (_spellRangeCircle != null) _spellRangeCircle.gameObject.SetActive(false);
        }

        /// <summary>마법별 범위 표시 반경. 0 = 전역 마법(원 미표시).</summary>
        private static float GetSpellIndicatorRadius(int si) => (SpellType)si switch
        {
            SpellType.Fireball  => 3f,
            SpellType.Lightning => 1.5f,
            SpellType.Heal      => 1.5f,
            SpellType.Freeze    => 0f,   // 전 적 — 전역
            SpellType.Rage      => 0f,   // 전 아군 — 전역
            _                   => 1f,
        };

        /// <summary>FOW: 해당 중심에서 반경 내 셀을 영구 공개 목록에 추가.</summary>
        private void MarkRevealed(Vector3 center, float radius)
        {
            int cr = Mathf.CeilToInt(radius / FowCellSize) + 1;
            int cx = Mathf.RoundToInt(center.x / FowCellSize);
            int cz = Mathf.RoundToInt(center.z / FowCellSize);
            float r2 = radius * radius;
            for (int dx = -cr; dx <= cr; dx++)
            for (int dz = -cr; dz <= cr; dz++)
            {
                float wx = (cx + dx) * FowCellSize - center.x;
                float wz = (cz + dz) * FowCellSize - center.z;
                if (wx * wx + wz * wz <= r2)
                    _revealedCells.Add(new Vector2Int(cx + dx, cz + dz));
            }
        }

        /// <summary>해당 위치가 과거에 한 번이라도 시야에 들어왔는지 확인.</summary>
        private bool IsAreaRevealed(Vector3 pos)
        {
            int cx = Mathf.RoundToInt(pos.x / FowCellSize);
            int cz = Mathf.RoundToInt(pos.z / FowCellSize);
            return _revealedCells.Contains(new Vector2Int(cx, cz));
        }
    }
}
