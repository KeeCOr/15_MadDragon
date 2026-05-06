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

        private int   _gold  = 300;
        private int   _valor = 0;
        private float _elapsed;
        private float _dmgMult = 1f;
        private readonly int[]        _roster   = new int[6];
        private readonly HashSet<int> _unlocked = new HashSet<int>();

        // 전투 중 획득 통계
        private int _earnedGold, _earnedValor, _destroyedBuildings;

        // ═══════════════════════════════════════════════════════
        //  씬 오브젝트
        // ═══════════════════════════════════════════════════════
        private Building _enemyCastle;
        private readonly List<Building>         _enemyBarracks     = new List<Building>();
        private readonly List<Building>         _allEnemyBuildings = new List<Building>();
        private readonly List<TestSimpleUnitAI> _playerUnits       = new List<TestSimpleUnitAI>();
        private readonly List<TestSimpleUnitAI> _selectedUnits     = new List<TestSimpleUnitAI>();

        // ═══════════════════════════════════════════════════════
        //  UI
        // ═══════════════════════════════════════════════════════
        private Font       _font;
        private GameObject _canvas;
        private GameObject _prepPanel, _battleHud, _upgradePanel, _resultPanel;

        // 준비 화면
        private Text _prepGoldText, _rosterText;
        private readonly Button[] _buyBtns   = new Button[6];
        private readonly Text[]   _buyLabels = new Text[6];

        // 전투 HUD
        private Text       _timerText, _valorHudText, _enemyHpText, _infoText;
        private GameObject _unitTypeBar, _selectionBox;
        private Vector2    _dragStart;
        private bool       _isDragging;
        private readonly HashSet<GameObject> _revealedBuildings = new HashSet<GameObject>();

        // 기지 개발
        private readonly Button[] _upgBtns = new Button[4];

        // 결과
        private Text _resultText, _resultStatsText;

        // 전투 중 자원 현황 패널
        private Text _statGoldText, _statValorText, _statBldgText;

        // ═══════════════════════════════════════════════════════
        //  라이프사이클
        // ═══════════════════════════════════════════════════════
        private void Start()
        {
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
            if (_phase != Phase.Battle) return;

            _elapsed += Time.deltaTime;
            float remaining = BattleTimeLimit - _elapsed;
            int remSec = Mathf.CeilToInt(Mathf.Max(remaining, 0f));
            _timerText.text  = $"⏱ {remSec}s";
            _timerText.color = remaining < 30f
                ? (Mathf.FloorToInt(remaining * 2f) % 2 == 0 ? Color.red : Color.white)
                : Color.white;
            _enemyHpText.text = $"적성: {(_enemyCastle != null ? _enemyCastle.CurrentHp : 0)}";

            HandleInput();
            _playerUnits.RemoveAll(u => u == null || !u.GetComponent<Unit>().IsAlive);
            _selectedUnits.RemoveAll(u => u == null);

            // ── 종료 조건 체크 ──────────────────────────────
            if (remaining <= 0f)      { EndGame(false, "시간 초과"); return; }
            if (_playerUnits.Count == 0) { EndGame(false, "전군 전멸"); return; }
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
        //  페이즈 전환
        // ═══════════════════════════════════════════════════════
        private void EnterBattle()
        {
            int total = 0;
            foreach (var c in _roster) total += c;
            if (total == 0) return;

            _prepPanel.SetActive(false);
            _battleHud.SetActive(true);
            _upgradePanel.SetActive(true);
            _phase = Phase.Battle;

            DeployArmy();

            StartCoroutine(DeployInitialEnemies());
            foreach (var b in _enemyBarracks)
                StartCoroutine(BarracksSpawnRoutine(b));
            StartCoroutine(CastleReinforceRoutine());
            StartCoroutine(FogOfWarRoutine());
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

            var unit = go.GetComponent<Unit>() ?? go.AddComponent<Unit>();
            var data = ScriptableObject.CreateInstance<UnitData>();
            data.unitName = def.name; data.maxHp = def.hp;
            data.damage   = Mathf.RoundToInt(def.dmg * _dmgMult);
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
        //  적 비동기 스폰
        // ═══════════════════════════════════════════════════════
        private IEnumerator DeployInitialEnemies()
        {
            yield return new WaitForSeconds(3f);
            var pts = new[] {
                new Vector3(5,0,3), new Vector3(5,0,-3),
                new Vector3(6,0,0), new Vector3(7,0,5), new Vector3(7,0,-5),
            };
            foreach (var p in pts) { SpawnUnit(Random.Range(0,2), false, p); yield return new WaitForSeconds(0.3f); }
        }

        private IEnumerator BarracksSpawnRoutine(Building b)
        {
            yield return new WaitForSeconds(Random.Range(10f, 16f));
            while (_phase == Phase.Battle && b != null && b.IsAlive)
            {
                float interval = Mathf.Max(15f - _elapsed / 25f, 6f);
                yield return new WaitForSeconds(interval);
                if (b == null || !b.IsAlive) break;
                int maxIdx = _elapsed < 60f ? 1 : (_elapsed < 120f ? 3 : 5);
                int count  = Mathf.Min(1 + Mathf.FloorToInt(_elapsed / 60f), 3);
                for (int i = 0; i < count; i++)
                {
                    SpawnUnit(Random.Range(0, Mathf.Min(maxIdx + 1, Defs.Length)),
                        false, b.transform.position + new Vector3(-1, 0, Random.Range(-2f, 2f)));
                    yield return new WaitForSeconds(0.4f);
                }
            }
        }

        private IEnumerator CastleReinforceRoutine()
        {
            yield return new WaitForSeconds(55f);
            while (_phase == Phase.Battle && _enemyCastle != null && _enemyCastle.IsAlive)
            {
                int count = Mathf.Min(2 + Mathf.FloorToInt(_elapsed / 60f), 5);
                for (int i = 0; i < count; i++)
                {
                    SpawnUnit(Random.Range(3, Defs.Length), false,
                        _enemyCastle.transform.position + new Vector3(-2, 0, Random.Range(-3f, 3f)));
                    yield return new WaitForSeconds(0.4f);
                }
                yield return new WaitForSeconds(40f);
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
                    if (hitAI == null || hitUnit == null || !hitUnit.IsPlayerUnit)
                    {
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
                }

                // 선택 없음 또는 아군 클릭 → 선택 처리
                if (hitAI != null && hitUnit != null && hitUnit.IsPlayerUnit && hitAI.AwaitingOrders)
                {
                    if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
                    Select(hitAI);
                    SetInfo($"[{hitUnit.Data.unitName}] 선택됨 — 다시 탭/우클릭으로 목표 지정");
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

            // 유닛 구매 버튼 — 3열 2행
            Vector2 btnSize = new Vector2(200, 108);
            float colW = 215f, startX = -215f;
            float row1Y = -175f, row2Y = -295f;

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

            _rosterText = Lbl(_prepPanel, "Roster", a, new Vector2(0, -418),
                new Vector2(720, 36), "병력 없음", 20, new Color(0.9f, 0.9f, 0.8f));
            if (_rosterText != null) _rosterText.alignment = TextAnchor.UpperCenter;

            Btn(_prepPanel, "StartBtn", a, new Vector2(0, -470),
                new Vector2(250, 66), "출전! ▶", new Color(0.1f, 0.55f, 0.15f), EnterBattle);
        }

        private string BuildBuyLabel(int i)
        {
            var d = Defs[i];
            bool locked = d.valorToUnlock > 0 && !_unlocked.Contains(i);
            string sub  = locked ? $"🔒 Valor {d.valorToUnlock}" : $"{d.cost}G";
            return $"[{d.name}]  {d.desc}\n{sub}   보유: {_roster[i]}";
        }

        private Color BuyColor(int i)
        {
            bool locked = Defs[i].valorToUnlock > 0 && !_unlocked.Contains(i);
            return locked ? new Color(0.22f, 0.14f, 0.14f)
                 : _gold >= Defs[i].cost ? new Color(0.12f, 0.22f, 0.38f)
                 : new Color(0.26f, 0.26f, 0.28f);
        }

        private void BuyUnit(int idx)
        {
            var d = Defs[idx];
            bool locked = d.valorToUnlock > 0 && !_unlocked.Contains(idx);
            if (locked || _gold < d.cost) return;
            _gold -= d.cost; _roster[idx]++;
            _prepGoldText.text = $"골드: {_gold}";
            for (int i = 0; i < Defs.Length; i++)
            {
                if (_buyLabels[i] != null) _buyLabels[i].text = BuildBuyLabel(i);
                if (_buyBtns[i]   != null) _buyBtns[i].GetComponent<Image>().color = BuyColor(i);
            }
            UpdateRosterText();
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
    }
}
