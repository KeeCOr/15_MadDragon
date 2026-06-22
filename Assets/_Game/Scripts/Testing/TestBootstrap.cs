// Assets/_Game/Scripts/Testing/TestBootstrap.cs
// ? 以鍮??붾㈃: 怨⑤뱶濡?蹂묐젰 援ъ꽦
// ? 異쒖쟾 ?? 蹂묐젰 ?꾩껜 ?꾩뿴 (?⑹깋 留?= 紐낅졊 ?湲?
// ? 醫뚰겢由??좏깮 ???고겢由?紐⑹쟻吏/????紐낅졊 ?좉툑 (?ъ???遺덇?)
// ? 諛⑺뼢 踰꾪듉: ?湲?以묒씤 蹂묐젰 ?쇨큵 ?뚭껄
// ? ?쒖빞 ?쒖뒪?? ?꾧뎔 ?쒖빞 諛????좊떅 ???
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
using MedievalRTS.Economy;
using MedievalRTS.UI;
using MedievalRTS.Visuals;

namespace MedievalRTS.Testing
{
    public class TestBootstrap : MonoBehaviour
    {
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?좊떅 ?뺤쓽
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
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
            new UnitDef{name="湲곗궗",  assetName="Knight",  desc="?깆빱 洹쇱젒",  hp=150,dmg=22,cost=50, speed=2.5f,atkRange=1.8f,cooldown=1.0f,threat=6f, bldgMult=1.0f,color=new Color(0.2f,0.4f,1f),   valorToUnlock=0},
            new UnitDef{name="沅곸닔",  assetName="Archer",  desc="?먭굅由???,  hp=70, dmg=30,cost=35, speed=3.0f,atkRange=7.0f,cooldown=1.2f,threat=9f, bldgMult=1.0f,color=new Color(0.2f,0.8f,0.8f), valorToUnlock=0},
            new UnitDef{name="留덈쾿??,assetName="Mage",    desc="嫄대Ъ 留덈쾿",  hp=90, dmg=55,cost=75, speed=2.0f,atkRange=5.5f,cooldown=1.5f,threat=7f, bldgMult=1.8f,color=new Color(0.7f,0.1f,0.9f), valorToUnlock=0},
            new UnitDef{name="?뺤같蹂?,assetName="Scout",   desc="??는룰퀬??,  hp=50, dmg=18,cost=25, speed=5.5f,atkRange=1.5f,cooldown=0.8f,threat=5f, bldgMult=1.0f,color=new Color(0.3f,0.8f,0.3f), valorToUnlock=0},
            new UnitDef{name="湲곕퀝",  assetName="Cavalry", desc="?뚭꺽?",     hp=130,dmg=32,cost=70, speed=6.0f,atkRange=1.8f,cooldown=0.9f,threat=7f, bldgMult=1.0f,color=new Color(1f,0.85f,0.1f),  valorToUnlock=1},
            new UnitDef{name="怨듭꽦湲?,assetName="Catapult",desc="嫄대Ъ ?뱁솕",  hp=50, dmg=85,cost=110,speed=1.2f,atkRange=10f, cooldown=2.5f,threat=6f, bldgMult=3.5f,color=new Color(1f,0.45f,0.1f),  valorToUnlock=2},
        };

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  寃뚯엫 ?곹깭
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private enum Phase { Prep, Battle, GameOver }
        private Phase _phase = Phase.Prep;

        private const float BattleTimeLimit = 180f;

        private int   _gold  = 999999;
        private int   _valor = 0;
        private float _elapsed;
        private float _dmgMult = 1f;
        private readonly int[]        _roster   = new int[6];
        private readonly HashSet<int> _unlocked = new HashSet<int>();

        // ?꾪닾 以??띾뱷 ?듦퀎
        private int _earnedGold, _earnedValor, _destroyedBuildings;

        // 紐⑤뱶 ?좏깮
        private bool _defenseMode = false;

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ???ㅻ툕?앺듃
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private Building _enemyCastle;
        private Building _playerCastle;
        private readonly List<Building>         _enemyBarracks     = new List<Building>();
        private readonly List<Building>         _allEnemyBuildings = new List<Building>();
        private readonly List<Building>         _allPlayerBuildings = new List<Building>();
        private readonly List<TestSimpleUnitAI> _playerUnits       = new List<TestSimpleUnitAI>();
        private readonly List<TestSimpleUnitAI> _selectedUnits     = new List<TestSimpleUnitAI>();
        private readonly List<GameObject>       _wallSegments      = new List<GameObject>();
        private int _gateIndex = 2; // ?숈そ ?깅꼍 以??대뒓 移몄씠 臾몄씤吏

        // ?섎퉬 吏꾪삎 援ъ꽦
        private bool _defenseSetupActive;
        private int  _selectedPlaceBldg = -1; // 0=諛⑹뼱?? 1=?깅꼍
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
            ("諛⑹뼱??, 80),
            ("?깅꼍",   20),
        };

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  UI
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private Font       _font;
        private GameObject _canvas;
        private GameObject _prepPanel, _battleHud, _upgradePanel, _resultPanel;
        private GameObject _rightPanel;
        private GameObject _spellSectionRoot; // 留덈쾿 援щℓ ?뱀뀡 ?꾩껜 (?덉씠釉?+ 踰꾪듉)

        // 以鍮??붾㈃
        private Text   _prepGoldText, _rosterText;
        private Button _modeToggleBtn;
        private Text   _modeToggleLbl;
        private readonly Button[] _buyBtns   = new Button[6];
        private readonly Text[]   _buyLabels = new Text[6];

        // ?꾪닾 HUD
        private Text       _timerText, _valorHudText, _enemyHpText, _infoText;
        private GameObject _unitTypeBar, _selectionBox;
        private Vector2    _dragStart;
        private bool       _isDragging;
        private readonly HashSet<GameObject>  _revealedBuildings = new HashSet<GameObject>();
        private readonly HashSet<Vector2Int>  _revealedCells     = new HashSet<Vector2Int>();
        private readonly List<FowVisualCell>  _fowVisualCells    = new List<FowVisualCell>();
        private const float FowCellSize = 2f;
        private const float FowMinX = -30f;
        private const float FowMaxX = 30f;
        private const float FowMinZ = -15f;
        private const float FowMaxZ = 15f;
        private LineRenderer _spellRangeCircle;
        private GameObject _fowVisualRoot;
        private Material _fowVisualMaterial;

        private struct FowVisualCell
        {
            public Vector2Int cell;
            public Vector3 worldCenter;
            public Renderer renderer;
        }

        // 湲곗? 媛쒕컻
        private readonly Button[] _upgBtns = new Button[4];

        // 寃곌낵
        private Text _resultText, _resultStatsText;

        // ?꾪닾 以??먯썝 ?꾪솴 ?⑤꼸
        private Text _statGoldText, _statValorText, _statBldgText;

        // ?뱀닔 嫄대Ъ UI (以鍮??붾㈃ ?곗륫)
        private readonly Button[] _specialBldgBtns = new Button[6];
        private readonly Text[]   _specialBldgLbls = new Text[6];

        // 留덈쾿 援щℓ UI (以鍮??붾㈃ ?곗륫)
        private readonly Button[] _spellBuyBtns = new Button[5];
        private readonly Text[]   _spellBuyLbls = new Text[5];

        // 留덈쾿 ?꾪닾 踰꾪듉
        private int _pendingSpell = -1;
        private readonly Button[] _spellBattleBtns      = new Button[5];
        private readonly Text[]   _spellBattleChargeLbls = new Text[5];
        private ResourceWallet _ownedResources;
        private ResourceStorageSystem _resourceStorage;
        private CampaignHubScreen _campaignHubScreen;
        private BaseManagementScreen _baseManagementScreen;
        private AttackPrepScreen _attackPrepScreen;
        private MobileBattleHud _mobileBattleHud;

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?쇱씠?꾩궗?댄겢
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void Start()
        {
            BuildingEffectSystem.Reset();
            SpellSystem.Reset();
            _ownedResources = new ResourceWallet();
            _ownedResources.Add(ResourceType.Gold, _gold);
            _resourceStorage = new ResourceStorageSystem(1);
            _resourceStorage.AddProducer(new ResourceProductionBuilding("GoldMine", ResourceType.Gold, 8f, 1200));
            BuildWorld();
            BuildUI();
            BuildMobileLoopScreens();
            ShowCampaignHub();
            RefreshMobileLoopScreens();
            EventBus.Subscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<BuildingDestroyedEvent>(OnBuildingDestroyed);
        }

        private void Update()
        {
            if (_resourceStorage != null)
            {
                _resourceStorage.TickProduction(Time.deltaTime);
                RefreshMobileLoopScreens();
            }

            // ?섎퉬 吏꾪삎 援ъ꽦 以? ?좊떅 ?좏깮쨌紐낅졊 + 嫄대Ъ 諛곗튂
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
            _timerText.text  = $"??{remSec}s";
            _timerText.color = remaining < 30f
                ? (Mathf.FloorToInt(remaining * 2f) % 2 == 0 ? Color.red : Color.white)
                : Color.white;
            if (_defenseMode)
                _enemyHpText.text = $"?꾧뎔 ?? {(_playerCastle != null ? _playerCastle.CurrentHp : 0)}";
            else
                _enemyHpText.text = $"?곸꽦: {(_enemyCastle != null ? _enemyCastle.CurrentHp : 0)}";
            _mobileBattleHud?.Refresh(
                remSec,
                _defenseMode ? (_playerCastle != null ? _playerCastle.CurrentHp : 0) : (_enemyCastle != null ? _enemyCastle.CurrentHp : 0),
                _earnedGold,
                _earnedValor);

            HandleInput();
            _playerUnits.RemoveAll(u => u == null || !u.GetComponent<Unit>().IsAlive);
            _selectedUnits.RemoveAll(u => u == null);

            // ?? 醫낅즺 議곌굔 泥댄겕 ??????????????????????????????
            if (remaining <= 0f) { EndGame(!_defenseMode, "?쒓컙 珥덇낵"); return; }
            if (!_defenseMode && _playerUnits.Count == 0) { EndGame(false, "?꾧뎔 ?꾨㈇"); return; }
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?붾뱶 援ъ꽦
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void BuildWorld()
        {
            SetupCamera();
            SetupLight();
            SetupGround();
            BuildBrightArenaDetails();

            // 1?? ?깅꼍 (x=2) ??怨좎껜???λ꼍
            MakeWall("Wall_L",  new Vector3(2, 1f,  6));
            MakeWall("Wall_C",  new Vector3(2, 1f,  0));
            MakeWall("Wall_R",  new Vector3(2, 1f, -6));

            // 2?? 留앸（ (x=5) ???꾨갑 怨듦꺽 ???
            MakeTower("Tower_F1", new Vector3(5, 1f,  5));
            MakeTower("Tower_F2", new Vector3(5, 1f, -5));

            // 3?? 蹂묒쁺 + 遊됲솕? (x=9)
            _enemyBarracks.Add(MakeBarracks("Barracks_L", new Vector3(9, 0.6f,  5)));
            _enemyBarracks.Add(MakeBarracks("Barracks_R", new Vector3(9, 0.6f, -5)));
            MakeBuffBuilding("Shrine", new Vector3(9, 0.75f, 0));

            // 4?? 留덈쾿????(x=13)
            MakeMageTower("MageTower_L", new Vector3(13, 1.4f,  4));
            MakeMageTower("MageTower_R", new Vector3(13, 1.4f, -4));

            // 5?? ?꾨갑 留앸（ (x=16)
            MakeTower("Tower_B1", new Vector3(16, 1f,  6));
            MakeTower("Tower_BC", new Vector3(16, 1f,  0));
            MakeTower("Tower_B2", new Vector3(16, 1f, -6));

            // ????(x=21)
            _enemyCastle = MakeBuilding("EnemyCastle", new Vector3(21, 1.5f, 0), 900, false,
                MobileVisualStyle.EnemyRed, new Vector3(4, 3, 4));
            AddToonyDecoration("red_banner", new Vector3(18.5f, 0f, 2.9f), Vector3.one * 0.9f, 180f);
            AddToonyDecoration("red_banner", new Vector3(18.5f, 0f, -2.9f), Vector3.one * 0.9f, 180f);
        }

        private void BuildBrightArenaDetails()
        {
            MakeFlatDetail("MainPath", new Vector3(0f, 0.03f, 0f), new Vector3(3.6f, 0.04f, 18f), MobileVisualStyle.PathStone);
            MakeFlatDetail("CrossPath", new Vector3(9f, 0.04f, 0f), new Vector3(18f, 0.04f, 3.2f), MobileVisualStyle.PathStone);
            MakeFlatDetail("ForestBed_N", new Vector3(0f, 0.02f, 12.4f), new Vector3(56f, 0.03f, 4.6f), MobileVisualStyle.ForestDeep);
            MakeFlatDetail("ForestBed_S", new Vector3(0f, 0.02f, -12.4f), new Vector3(56f, 0.03f, 4.6f), MobileVisualStyle.ForestDeep);
            MakeFlatDetail("OuterGrass_N", new Vector3(0f, 0.025f, 9.7f), new Vector3(58f, 0.03f, 1.4f), MobileVisualStyle.GrassPatch);
            MakeFlatDetail("OuterGrass_S", new Vector3(0f, 0.025f, -9.7f), new Vector3(58f, 0.03f, 1.4f), MobileVisualStyle.GrassPatch);
            MakeFlatDetail("Creek", new Vector3(-12f, 0.035f, -8.8f), new Vector3(13f, 0.035f, 0.75f), MobileVisualStyle.WaterBlue);
            MakeFlatDetail("CreekBank_A", new Vector3(-12f, 0.04f, -8.25f), new Vector3(13.5f, 0.035f, 0.16f), MobileVisualStyle.DirtWarm);
            MakeFlatDetail("CreekBank_B", new Vector3(-12f, 0.04f, -9.35f), new Vector3(13.5f, 0.035f, 0.16f), MobileVisualStyle.DirtWarm);

            for (int i = 0; i < 9; i++)
            {
                float x = -24f + i * 6f;
                MakeTree($"Pine_N_{i}", new Vector3(x, 0f, 10.8f));
                MakeTree($"Pine_S_{i}", new Vector3(x + 2f, 0f, -10.8f));
            }

            for (int i = 0; i < 8; i++)
            {
                float x = -24f + i * 7f;
                MakeForestCluster($"Forest_N_{i}", new Vector3(x, 0f, 12.1f), i);
                MakeForestCluster($"Forest_S_{i}", new Vector3(x + 3.5f, 0f, -12.0f), i + 8);
            }

            for (int i = 0; i < 10; i++)
            {
                float x = -25f + i * 5.5f;
                MakeGrassTuft($"Grass_N_{i}", new Vector3(x, 0f, 8.8f + (i % 3) * 0.35f), 0.9f + (i % 2) * 0.2f);
                MakeGrassTuft($"Grass_S_{i}", new Vector3(x + 1.8f, 0f, -8.8f - (i % 3) * 0.35f), 0.85f + (i % 2) * 0.18f);
            }

            MakeFlowerPatch("Flowers_Left", new Vector3(-22f, 0f, -7.3f), 7);
            MakeFlowerPatch("Flowers_Right", new Vector3(18f, 0f, 7.4f), 9);
            MakeBackgroundRidge("Ridge_Back_N", new Vector3(0f, 0f, 14.6f));
            MakeBackgroundRidge("Ridge_Back_S", new Vector3(0f, 0f, -14.6f));

            MakeRockCluster("Rock_L", new Vector3(-18f, 0f, 8.4f));
            MakeRockCluster("Rock_C", new Vector3(0f, 0f, -9.2f));
            MakeRockCluster("Rock_R", new Vector3(18f, 0f, 8.4f));
        }

        private void MakeWall(string n, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = new Vector3(0.8f, 2f, 3.5f);
            Paint(go, MobileVisualStyle.StoneWarm);
            AddWallCap(go);
            ApplyBuildingVisual(go, "wall", "Buildings/wall", new Vector3(0f, 1.2f, -0.15f), new Vector2(3.6f, 3.6f), -0.95f, Vector3.one * 1.25f);
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
            Paint(go, MobileVisualStyle.MageViolet);
            AddMageTowerDecor(go);
            ApplyBuildingVisual(go, "mage_tower", "Buildings/mage_tower", new Vector3(0f, 1.35f, -0.15f), new Vector2(3.4f, 3.4f), -0.7f, Vector3.one * 1.2f);
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
            Paint(go, MobileVisualStyle.GoldAccent);
            AddGoldCacheDecor(go);
            ApplyBuildingVisual(go, "elixir_well", "Buildings/elixir_well", new Vector3(0f, 1.05f, -0.15f), new Vector2(3.0f, 3.0f), -0.75f, Vector3.one * 1.1f);
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
            MobileVisualStyle.ApplyCamera(cam, _defenseMode);
        }

        private void SetupLight()
        {
            var l = FindObjectOfType<Light>();
            if (l == null) { var g = new GameObject("Sun"); l = g.AddComponent<Light>(); l.type = LightType.Directional; }
            MobileVisualStyle.ApplyLight(l);
        }

        private void SetupGround()
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
            g.name = "Ground"; g.transform.localScale = new Vector3(6, 1, 3);
            Paint(g, MobileVisualStyle.GrassBase);
        }

        private void MakeFlatDetail(string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            Paint(go, color);
            RemoveCollider(go);
        }

        private void MakeTree(string name, Vector3 position)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name;
            trunk.transform.position = position + new Vector3(0f, 0.45f, 0f);
            trunk.transform.localScale = new Vector3(0.28f, 0.45f, 0.28f);
            Paint(trunk, MobileVisualStyle.WoodWarm);
            RemoveCollider(trunk);

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = name + "_Crown";
            crown.transform.position = position + new Vector3(0f, 1.15f, 0f);
            crown.transform.localScale = new Vector3(1.0f, 0.9f, 1.0f);
            Paint(crown, MobileVisualStyle.GrassDark);
            RemoveCollider(crown);
        }

        private void MakeForestCluster(string name, Vector3 position, int seed)
        {
            MakeTree(name + "_TreeA", position + new Vector3(-0.75f, 0f, 0.2f));
            MakeTree(name + "_TreeB", position + new Vector3(0.55f, 0f, -0.35f));
            MakeBush(name + "_BushA", position + new Vector3(1.35f, 0f, 0.45f), 0.8f);
            MakeBush(name + "_BushB", position + new Vector3(-1.35f, 0f, -0.35f), 0.65f);
            if (seed % 2 == 0) MakeRockCluster(name + "_Rocks", position + new Vector3(0.3f, 0f, 1.0f));
        }

        private void MakeBush(string name, Vector3 position, float scale)
        {
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = name;
            bush.transform.position = position + new Vector3(0f, 0.28f * scale, 0f);
            bush.transform.localScale = new Vector3(1.25f * scale, 0.55f * scale, 0.85f * scale);
            Paint(bush, MobileVisualStyle.GrassLight);
            RemoveCollider(bush);
        }

        private void MakeGrassTuft(string name, Vector3 position, float scale)
        {
            for (int i = 0; i < 4; i++)
            {
                var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = $"{name}_{i}";
                blade.transform.position = position + new Vector3((i - 1.5f) * 0.18f * scale, 0.16f * scale, (i % 2 == 0 ? 0.1f : -0.1f) * scale);
                blade.transform.localScale = new Vector3(0.08f * scale, 0.32f * scale, 0.08f * scale);
                blade.transform.rotation = Quaternion.Euler(0f, i * 28f, i % 2 == 0 ? 10f : -10f);
                Paint(blade, i % 2 == 0 ? MobileVisualStyle.GrassLight : MobileVisualStyle.GrassPatch);
                RemoveCollider(blade);
            }
        }

        private void MakeFlowerPatch(string name, Vector3 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = $"{name}_{i}";
                flower.transform.position = position + new Vector3((i % 4) * 0.48f, 0.18f, (i / 4) * 0.45f);
                flower.transform.localScale = new Vector3(0.18f, 0.12f, 0.18f);
                Paint(flower, i % 2 == 0 ? MobileVisualStyle.FlowerPink : MobileVisualStyle.FlowerYellow);
                RemoveCollider(flower);
            }
        }

        private void MakeBackgroundRidge(string name, Vector3 position)
        {
            for (int i = 0; i < 7; i++)
            {
                var ridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ridge.name = $"{name}_{i}";
                ridge.transform.position = position + new Vector3(-24f + i * 8f, 0.22f, 0f);
                ridge.transform.localScale = new Vector3(6f, 0.42f + (i % 3) * 0.12f, 0.7f);
                Paint(ridge, MobileVisualStyle.GrassDark);
                RemoveCollider(ridge);
            }
        }

        private void MakeRockCluster(string name, Vector3 position)
        {
            for (int i = 0; i < 3; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"{name}_{i}";
                rock.transform.position = position + new Vector3(i * 0.55f, 0.2f, (i % 2 == 0 ? 0.25f : -0.25f));
                rock.transform.localScale = new Vector3(0.75f - i * 0.12f, 0.38f, 0.55f);
                Paint(rock, i == 1 ? MobileVisualStyle.StoneShadow : MobileVisualStyle.StoneWarm);
                RemoveCollider(rock);
            }
        }

        private void AddBuildingDecor(GameObject root, Color roofColor, bool large)
        {
            float roofY = 0.58f;
            float roofScale = large ? 0.82f : 0.74f;
            AddDecorBlock(root, "Roof", PrimitiveType.Cube, new Vector3(0f, roofY, 0f), new Vector3(roofScale, 0.18f, roofScale), roofColor);
            AddDecorBlock(root, "Trim", PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), new Vector3(1.08f, 0.08f, 1.08f), MobileVisualStyle.GoldAccent);
            AddDecorBlock(root, "Door", PrimitiveType.Cube, new Vector3(0f, -0.12f, -0.51f), new Vector3(0.28f, 0.42f, 0.04f), MobileVisualStyle.WoodWarm);

            if (large)
            {
                AddDecorBlock(root, "KeepTop", PrimitiveType.Cube, new Vector3(0f, 0.78f, 0f), new Vector3(0.42f, 0.2f, 0.42f), roofColor);
                AddDecorBlock(root, "Banner", PrimitiveType.Cube, new Vector3(0f, 0.42f, -0.56f), new Vector3(0.16f, 0.52f, 0.04f), roofColor);
            }
        }

        private void AddTowerDecor(GameObject root, Color roofColor)
        {
            AddDecorBlock(root, "TowerRoof", PrimitiveType.Cube, new Vector3(0f, 0.58f, 0f), new Vector3(0.92f, 0.18f, 0.92f), roofColor);
            AddDecorBlock(root, "Torch", PrimitiveType.Sphere, new Vector3(0f, 0.82f, -0.28f), new Vector3(0.18f, 0.18f, 0.18f), MobileVisualStyle.TorchOrange);
            AddPointGlow(root.transform, new Vector3(0f, 0.82f, -0.28f), MobileVisualStyle.TorchOrange, 0.65f, 2.2f);
        }

        private void AddMageTowerDecor(GameObject root)
        {
            AddDecorBlock(root, "Crystal", PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0f), new Vector3(0.35f, 0.5f, 0.35f), MobileVisualStyle.MageViolet);
            AddPointGlow(root.transform, new Vector3(0f, 0.75f, 0f), MobileVisualStyle.MageViolet, 0.75f, 3f);
        }

        private void AddGoldCacheDecor(GameObject root)
        {
            AddDecorBlock(root, "GoldPile", PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(0.65f, 0.25f, 0.65f), MobileVisualStyle.GoldAccent);
            AddDecorBlock(root, "WoodBase", PrimitiveType.Cube, new Vector3(0f, -0.2f, 0f), new Vector3(1.1f, 0.12f, 1.1f), MobileVisualStyle.WoodWarm);
        }

        private void AddWallCap(GameObject root)
        {
            AddDecorBlock(root, "WallCap", PrimitiveType.Cube, new Vector3(0f, 0.56f, 0f), new Vector3(1.14f, 0.16f, 1.05f), MobileVisualStyle.StoneShadow);
        }

        private void ApplyGeneratedFacade(GameObject root, string artKey, Vector3 localPosition, Vector2 worldSize)
        {
            var sprite = GeneratedArtLibrary.LoadSprite(artKey, 160f);
            if (sprite == null) return;

            foreach (var meshRenderer in root.GetComponentsInChildren<MeshRenderer>())
                meshRenderer.enabled = false;

            var go = new GameObject("GeneratedFacade");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;
            go.AddComponent<GeneratedArtBillboard>();
        }

        private void ApplyToonyUnitVisual(GameObject root, UnitDef def)
        {
            var prefab = ToonyRtsVisualLibrary.LoadUnit(def.assetName);
            var visual = ToonyRtsVisualApplier.Attach(root, prefab, new Vector3(0f, -0.48f, 0f), Vector3.one * 0.88f, Quaternion.identity);
            if (visual != null)
                ToonyRtsVisualApplier.HideRootRenderers(root);
        }

        private void ApplyBuildingVisual(GameObject root, string key, string fallbackArtKey, Vector3 fallbackLocalPosition, Vector2 fallbackWorldSize, float groundOffsetY, Vector3 worldScale)
        {
            if (ApplyToonyBuildingVisual(root, key, groundOffsetY, worldScale, fallbackWorldSize))
                return;

            ApplyGeneratedFacade(root, fallbackArtKey, fallbackLocalPosition, fallbackWorldSize);
        }

        private bool ApplyToonyBuildingVisual(GameObject root, string key, float groundOffsetY, Vector3 worldScale, Vector2 targetFootprint)
        {
            var prefab = ToonyRtsVisualLibrary.LoadBuilding(key);
            var visual = ToonyRtsVisualApplier.Attach(root, prefab, new Vector3(0f, groundOffsetY, 0f), worldScale, Quaternion.identity);
            if (visual == null) return false;

            ToonyRtsVisualApplier.FitFootprintToWorldSize(visual, targetFootprint, 0.45f, 3.5f);
            ToonyRtsVisualApplier.AlignBottomToWorldY(visual, root.transform.position.y + groundOffsetY);
            ToonyRtsVisualApplier.HideRootRenderers(root);
            return true;
        }

        private void AddToonyDecoration(string key, Vector3 position, Vector3 worldScale, float yaw)
        {
            var prefab = ToonyRtsVisualLibrary.LoadDecoration(key);
            if (prefab == null) return;

            var root = new GameObject($"Toony_{key}");
            root.transform.position = position;
            ToonyRtsVisualApplier.Attach(root, prefab, Vector3.zero, worldScale, Quaternion.Euler(0f, yaw, 0f));
        }

        private GameObject AddDecorBlock(GameObject root, string name, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Paint(go, color);
            RemoveCollider(go);
            return go;
        }

        private void AddPointGlow(Transform parent, Vector3 localPosition, Color color, float intensity, float range)
        {
            var go = new GameObject("Glow");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
        }

        private static void RemoveCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private Building MakeBuilding(string n, Vector3 pos, int hp,
            bool isPlayer, Color col, Vector3? scale = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = scale ?? Vector3.one;
            Paint(go, col);
            AddBuildingDecor(go, isPlayer ? MobileVisualStyle.FriendlyBlue : MobileVisualStyle.EnemyRed, hp >= 800);
            ApplyBuildingVisual(
                go,
                hp >= 800 ? (isPlayer ? "player_castle" : "enemy_castle") : "barracks",
                hp >= 800 ? (isPlayer ? "Buildings/player_castle" : "Buildings/enemy_castle") : "Buildings/barracks",
                new Vector3(0f, hp >= 800 ? 1.75f : 1.0f, -0.2f),
                hp >= 800 ? new Vector2(5.4f, 5.4f) : new Vector2(3.4f, 3.4f),
                -(scale ?? Vector3.one).y * 0.5f,
                hp >= 800 ? Vector3.one * 1.8f : Vector3.one * 1.25f);
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
                MobileVisualStyle.EnemyRed, new Vector3(2.5f, 1.2f, 2.5f));
        }

        private void MakeTower(string n, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            Paint(go, MobileVisualStyle.EnemyRed);
            AddTowerDecor(go, MobileVisualStyle.EnemyRed);
            ApplyBuildingVisual(go, "tower", "Buildings/tower", new Vector3(0f, 1.35f, -0.15f), new Vector2(3.0f, 3.0f), -1f, Vector3.one * 1.2f);
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = 220;
            var tb = go.AddComponent<Building>();
            tb.Initialize(data, isPlayerBuilding: false);
            _allEnemyBuildings.Add(tb);
            go.AddComponent<TestTowerAI>().Setup(false, 9f, 18, 1.2f);
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  紐⑤뱶 ?꾪솚
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void ToggleMode()
        {
            _defenseMode = !_defenseMode;
            if (_modeToggleLbl != null)
                _modeToggleLbl.text = _defenseMode ? "?섎퉬 紐⑤뱶 ?썳" : "怨듦꺽 紐⑤뱶 ??;
            _modeToggleBtn.GetComponent<Image>().color = _defenseMode
                ? new Color(0.4f, 0.15f, 0.15f)
                : new Color(0.15f, 0.4f, 0.15f);
            ApplyModePrepVisibility();
        }

        /// <summary>怨듦꺽/?섎퉬 紐⑤뱶???곕씪 以鍮??붾㈃ ?뱀뀡 媛?쒖꽦??議곗젙?⑸땲??</summary>
        private void ApplyModePrepVisibility()
        {
            // 怨듦꺽 紐⑤뱶: 留덈쾿 ?뱀뀡쨌異쒖쟾 踰꾪듉 / ?섎퉬 紐⑤뱶: 吏꾪삎 援ъ꽦 ?쒖옉 踰꾪듉
            if (_spellSectionRoot != null)  _spellSectionRoot.SetActive(!_defenseMode);
            if (_rightPanel != null)        _rightPanel.SetActive(false); // ?섎퉬 ?뱀닔嫄대Ъ? 吏꾪삎 援ъ꽦 HUD???쒖떆
            if (_startBattleBtn != null)    _startBattleBtn.gameObject.SetActive(!_defenseMode);
            if (_enterSetupBtn != null)     _enterSetupBtn.gameObject.SetActive(_defenseMode);
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?섏씠利??꾪솚
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void EnterBattle()
        {
            int total = 0;
            foreach (var c in _roster) total += c;
            // 怨듦꺽 紐⑤뱶???좊떅 ?꾩슂, ?섎퉬 紐⑤뱶????뚮쭔?쇰줈??媛??
            if (total == 0 && !_defenseMode)
            {
                _roster[0] = 3;
                total = 3;
                UpdateRosterText();
                RefreshMobileLoopScreens();
            }

            _prepPanel.SetActive(false);
            if (_dsHud != null) _dsHud.SetActive(false);
            _campaignHubScreen?.SetVisible(false);
            _baseManagementScreen?.SetVisible(false);
            _attackPrepScreen?.SetVisible(false);
            _mobileBattleHud?.SetVisible(true);
            _battleHud.SetActive(true);
            _upgradePanel.SetActive(true);
            _phase = Phase.Battle;
            BuildingEffectSystem.TreasuryAlive = BuildingEffectSystem.GetLevel(SpecialBuildingType.Treasury) > 0;
            RefreshSpellBattleBtns();

            if (_defenseMode)
            {
                if (_defenseSetupActive)
                {
                    // 吏꾪삎 援ъ꽦?먯꽌 ?꾪솚 ??嫄대Ъ쨌?좊떅 ?대? 諛곗튂??
                    BuildUnitTypeButtons();
                    SetInfo("?꾪닾 ?쒖옉! ?곸씠 ?ㅻⅨ履쎌뿉??怨듦꺽?⑸땲??");
                }
                else
                {
                    // 以鍮??⑤꼸?먯꽌 諛붾줈 ?쒖옉 (?대갚)
                    BuildDefenseBase();
                    DeployDefenseArmy();
                }
                StartCoroutine(DefenseEnemyWaveRoutine());
            }
            else
            {
                EnsureFogVisualLayer();
                DeployArmy();
                StartCoroutine(SpawnInitialEnemyForce());
                StartCoroutine(FogOfWarRoutine());
            }
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  蹂묐젰 ?꾩뿴
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void DeployArmy()
        {
            // 4??醫낅?濡?醫뚯륫 諛곗튂
            int col = 0, row = 0;
            for (int i = 0; i < Defs.Length; i++)
            {
                for (int k = 0; k < _roster[i]; k++)
                {
                    // x: -14遺???ㅻ줈, z: 以묒븰 湲곗? 醫뚯슦
                    float x = -14f - col * 2.2f;
                    float z = (row - 1) * 2.4f;
                    var ai = SpawnUnit(i, true, new Vector3(x, 0, z));
                    ai.SetAwaitingOrders();
                    _playerUnits.Add(ai);
                    if (++row >= 3) { row = 0; col++; }
                }
            }
            BuildUnitTypeButtons();
            SetInfo("蹂묐젰???꾩뿴?덉뒿?덈떎 ???쒕옒洹맞룻겢由?쑝濡??좏깮 ???고겢由??먮뒗 諛⑺뼢 踰꾪듉");
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?좊떅 ?ㅽ룿
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private TestSimpleUnitAI SpawnUnit(int idx, bool isPlayer, Vector3 pos)
        {
            var def = Defs[idx];
            var prefab = FindUnitPrefab(def.assetName);
            GameObject go;
            if (prefab != null)
            {
                // NavMeshAgent OnEnable 諛⑹?: 鍮꾪솢???곹깭濡?蹂듭궗 ??而댄룷?뚰듃 ?쒓굅 ???쒖꽦??
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
            ApplyToonyUnitVisual(go, def);

            // 怨듦꺽 紐⑤뱶 ???좊떅: FOW ?곸슜 ?꾧퉴吏 ?④꺼??源쒕컯??諛⑹?
            // ?섎퉬 紐⑤뱶: FOW ?놁쓬 ?????좊떅 利됱떆 ?쒖떆
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

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  怨듦꺽 紐⑤뱶 ??珥덇린 ??諛곗튂 (?섎퉬痢≪? ?ъ깮???놁쓬)
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private IEnumerator SpawnInitialEnemyForce()
        {
            yield return new WaitForSeconds(2f);
            // 1???섎퉬 ?좊떅 (湲곗궗 + 沅곸닔)
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
            // 2?? 留덈쾿??+ ?뺤같蹂?
            var line2 = new[] {
                new Vector3(8,0,5), new Vector3(8,0,0), new Vector3(8,0,-5),
            };
            foreach (var p in line2)
            {
                SpawnUnit(Random.Range(1, 4), false, p);
                yield return new WaitForSeconds(0.25f);
            }
            // 3??(??洹쇱쿂): 媛뺥븳 ?좊떅
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

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?섎퉬 紐⑤뱶 ??湲곗? 援ъ텞 + ?깅꼍 ?앹꽦
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void BuildDefenseBase()
        {
            // ?뚮젅?댁뼱 ??(?쇱そ ??
            _playerCastle = MakePlayerBuilding("PlayerCastle", new Vector3(-21, 1.5f, 0), 900,
                MobileVisualStyle.FriendlyBlue, new Vector3(4, 3, 4));
            AddToonyDecoration("blue_banner", new Vector3(-18.5f, 0f, 2.9f), Vector3.one * 0.9f, 0f);
            AddToonyDecoration("blue_banner", new Vector3(-18.5f, 0f, -2.9f), Vector3.one * 0.9f, 0f);

            // ?뚮젅?댁뼱 ???(????
            MakePlayerTower("PTower_L",  new Vector3(-16, 1f,  6));
            MakePlayerTower("PTower_C",  new Vector3(-16, 1f,  0));
            MakePlayerTower("PTower_R",  new Vector3(-16, 1f, -6));
            MakePlayerTower("PTower_F1", new Vector3(-13, 1f,  4));
            MakePlayerTower("PTower_F2", new Vector3(-13, 1f, -4));

            // ?먮룞 ?깅꼍 ?앹꽦 (x=-10 ?쇱씤, z=-8~8)
            GenerateAutoWall(-10f, -8f, 8f);

            // ??諛⑺뼢 (?ㅻⅨ履? ?쒖떆???쒖?
            SetInfo("?섎퉬 以鍮??꾨즺 ???곸씠 ?ㅻⅨ履쎌뿉??怨듦꺽?⑸땲?? 諛⑺뼢 踰꾪듉?쇰줈 蹂묐젰 諛곗튂");
        }

        private Building MakePlayerBuilding(string n, Vector3 pos, int hp, Color col, Vector3? scale = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = n; go.transform.position = pos;
            go.transform.localScale = scale ?? Vector3.one;
            Paint(go, col);
            AddBuildingDecor(go, MobileVisualStyle.FriendlyBlue, hp >= 800);
            ApplyBuildingVisual(
                go,
                hp >= 800 ? "player_castle" : "barracks",
                hp >= 800 ? "Buildings/player_castle" : "Buildings/barracks",
                new Vector3(0f, hp >= 800 ? 1.75f : 1.0f, -0.2f),
                hp >= 800 ? new Vector2(5.4f, 5.4f) : new Vector2(3.4f, 3.4f),
                -(scale ?? Vector3.one).y * 0.5f,
                hp >= 800 ? Vector3.one * 1.8f : Vector3.one * 1.25f);
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
            Paint(go, MobileVisualStyle.FriendlyBlue);
            AddTowerDecor(go, MobileVisualStyle.FriendlyBlue);
            ApplyBuildingVisual(go, "tower", "Buildings/tower", new Vector3(0f, 1.35f, -0.15f), new Vector2(3.0f, 3.0f), -1f, Vector3.one * 1.2f);
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = n; data.maxHp = 220;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: true);
            _allPlayerBuildings.Add(b);
            // ???AI (?꾧뎔?대?濡?isPlayerSide=true ?????좊떅 怨듦꺽)
            go.AddComponent<TestTowerAI>().Setup(true, 9f, 18, 1.2f);
        }

        private void GenerateAutoWall(float wallX, float zMin, float zMax)
        {
            _wallSegments.Clear();
            float segH = 2f; // 媛??깅꼍 ?멸렇癒쇳듃 ?믪씠
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
                Paint(go, isGate ? MobileVisualStyle.GoldAccent : MobileVisualStyle.StoneWarm);
                AddWallCap(go);
                ApplyBuildingVisual(go, "wall", "Buildings/wall", new Vector3(0f, 1.2f, -0.15f), new Vector2(3.0f, 3.0f), -1f, Vector3.one * 1.15f);
                // 臾?gate)? ?듦낵 媛??(肄쒕씪?대뜑 ?놁빊)
                if (isGate) { Destroy(go.GetComponent<Collider>()); }
                _wallSegments.Add(go);

                // 臾??대┃ ?대깽?몄슜 ?쒓렇 (BuildingData ?놁씠 ?⑥닚 ?ㅻ툕?앺듃)
                if (isGate) go.name = "Gate";
            }
        }

        private void MoveGate(int newIdx)
        {
            if (newIdx < 0 || newIdx >= _wallSegments.Count) return;
            // 湲곗〈 臾????쇰컲 ?깅꼍?쇰줈 蹂듭썝
            var old = _wallSegments[_gateIndex];
            if (old != null)
            {
                Paint(old, MobileVisualStyle.StoneWarm);
                if (old.GetComponent<Collider>() == null) old.AddComponent<BoxCollider>();
                old.name = $"Wall_{_gateIndex}";
            }
            _gateIndex = newIdx;
            var gateGo = _wallSegments[_gateIndex];
            if (gateGo != null)
            {
                Paint(gateGo, MobileVisualStyle.GoldAccent);
                Destroy(gateGo.GetComponent<Collider>());
                gateGo.name = "Gate";
            }
        }

        private void DeployDefenseArmy()
        {
            // ?깅꼍 ?덉そ(?쒖そ)??4??諛곗튂
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
                ShowResourcePopup(new Vector3(0, 3, 0), $"??wave}??怨듦꺽!");
                SetInfo($"??wave}???곸씠 怨듦꺽?⑸땲??");
                for (int i = 0; i < unitCount; i++)
                {
                    float z = Random.Range(-7f, 7f);
                    float x = waveSpawnX[Random.Range(0, waveSpawnX.Length)];
                    SpawnUnit(Random.Range(0, maxUnitIdx + 1), false, new Vector3(x, 0, z));
                    yield return new WaitForSeconds(0.4f);
                }
                if (wave >= 5)
                {
                    // 留덉?留???泥섎━ ???쇱젙 ?쒓컙 ?湲????밸━
                    yield return new WaitForSeconds(22f);
                    if (_phase == Phase.Battle) EndGame(true, "????寃⑺눜!");
                    yield break;
                }
            }
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?쒖빞 ?쒖뒪??(FOW)
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
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
                // ?쒖빞 ?꾨떖 ? 湲곕줉 ??留덈쾿 ?ъ슜 媛??援ъ뿭 ?꾩쟻
                foreach (var (p, r) in sources) MarkRevealed(p, r);
                ApplyFog("EnemyUnit", sources, 10f);
                ApplyFogBuildings(sources, 14f);
                UpdateFogVisualLayer(sources);
            }
        }

        // Visual layer: unexplored cells are dark, explored cells stay lightly misted.
        private void EnsureFogVisualLayer()
        {
            if (_defenseMode) return;
            if (_fowVisualRoot != null)
            {
                SetFogVisualLayerVisible(true);
                return;
            }

            _fowVisualRoot = new GameObject("FogOfWarVisualLayer");
            _fowVisualMaterial = CreateFogVisualMaterial();
            _fowVisualCells.Clear();

            int minCellX = Mathf.FloorToInt(FowMinX / FowCellSize);
            int maxCellX = Mathf.CeilToInt(FowMaxX / FowCellSize);
            int minCellZ = Mathf.FloorToInt(FowMinZ / FowCellSize);
            int maxCellZ = Mathf.CeilToInt(FowMaxZ / FowCellSize);

            for (int x = minCellX; x <= maxCellX; x++)
            for (int z = minCellZ; z <= maxCellZ; z++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"FogCell_{x}_{z}";
                tile.transform.SetParent(_fowVisualRoot.transform, false);
                tile.transform.position = new Vector3(x * FowCellSize, 0.13f, z * FowCellSize);
                tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                tile.transform.localScale = Vector3.one * (FowCellSize * 1.08f);
                RemoveCollider(tile);

                var renderer = tile.GetComponent<Renderer>();
                renderer.sharedMaterial = _fowVisualMaterial;
                renderer.sortingOrder = 20;
                _fowVisualCells.Add(new FowVisualCell
                {
                    cell = new Vector2Int(x, z),
                    worldCenter = tile.transform.position,
                    renderer = renderer
                });
            }
        }

        private Material CreateFogVisualMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Unlit/Transparent")
                      ?? Shader.Find("Sprites/Default")
                      ?? Shader.Find("Standard");
            var mat = new Material(shader);
            mat.name = "FOW_DarkMist";
            mat.renderQueue = 3000;
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.color = new Color(0.02f, 0.035f, 0.055f, 0.7f);
            return mat;
        }

        private void SetFogVisualLayerVisible(bool visible)
        {
            if (_fowVisualRoot != null)
                _fowVisualRoot.SetActive(visible && !_defenseMode);
        }

        private void UpdateFogVisualLayer(List<(Vector3 p, float r)> sources)
        {
            if (_fowVisualRoot == null) return;
            SetFogVisualLayerVisible(true);

            foreach (var cell in _fowVisualCells)
            {
                if (cell.renderer == null) continue;

                bool currentlyVisible = IsWithinAnySightSource(cell.worldCenter, sources, 0.6f);
                bool explored = _revealedCells.Contains(cell.cell);
                if (currentlyVisible)
                {
                    cell.renderer.enabled = false;
                    continue;
                }

                cell.renderer.enabled = true;
                cell.renderer.material.color = explored
                    ? new Color(0.04f, 0.055f, 0.07f, 0.36f)
                    : new Color(0.01f, 0.018f, 0.032f, 0.72f);
            }
        }

        private bool IsWithinAnySightSource(Vector3 point, List<(Vector3 p, float r)> sources, float feather)
        {
            foreach (var (p, r) in sources)
            {
                float radius = r + feather;
                Vector2 delta = new Vector2(point.x - p.x, point.z - p.z);
                if (delta.sqrMagnitude <= radius * radius)
                    return true;
            }

            return false;
        }

        // 嫄대Ъ: ??踰?蹂?寃껋? ?곴뎄 ?쒖떆
        private void ApplyFogBuildings(List<(Vector3 p, float r)> sources, float sightRadius)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag("EnemyBuilding"))
            {
                if (go == null) continue;
                if (_revealedBuildings.Contains(go))
                {
                    // ?대? 諛쒓껄??嫄대Ъ ????긽 ?쒖떆
                    foreach (var rnd in go.GetComponentsInChildren<Renderer>()) rnd.enabled = true;
                    foreach (var col in go.GetComponentsInChildren<Collider>())  col.enabled = true;
                    continue;
                }
                bool vis = false;
                foreach (var (p, r) in sources)
                    if (Vector3.Distance(go.transform.position, p) <= sightRadius) { vis = true; break; }
                if (vis) _revealedBuildings.Add(go); // 泥?諛쒓껄 ???깅줉
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
                // 肄쒕씪?대뜑??鍮꾪솢?깊솕?댁꽌 ?쒖빞 諛?嫄대Ъ ?대┃ 諛⑹?
                foreach (var col in go.GetComponentsInChildren<Collider>())
                    col.enabled = vis;
            }
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?꾪닾 ?낅젰 泥섎━
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void HandleInput()
        {
            bool lmbDown = Input.GetMouseButtonDown(0);
            bool lmbHeld = Input.GetMouseButton(0);
            bool lmbUp   = Input.GetMouseButtonUp(0);
            bool rmb     = Input.GetMouseButtonDown(1);

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            // ?? ?섎퉬 吏꾪삎 援ъ꽦: 嫄대Ъ 諛곗튂 ?좏깮 痍⑥냼 (?고겢由? ??????????
            if (_defenseSetupActive && _selectedPlaceBldg >= 0 && rmb)
            {
                _selectedPlaceBldg = -1;
                for (int i = 0; i < _dsPalBtns.Length; i++)
                    if (_dsPalBtns[i] != null)
                        _dsPalBtns[i].GetComponent<Image>().color = new Color(0.15f, 0.25f, 0.4f);
                SetDsStatus("?좏깮 ?댁젣 ??嫄대Ъ???좏깮?섍굅???좊떅???앹궛?섏꽭??);
                return;
            }

            // ?? ?섎퉬 吏꾪삎 援ъ꽦: 吏硫??대┃ ??嫄대Ъ 諛곗튂 ????????????????
            if (_defenseSetupActive && _selectedPlaceBldg >= 0 && lmbDown && !overUI)
            {
                var pr = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(pr, out RaycastHit ph, 200f))
                {
                    // ?깅꼍 移??대┃ ??臾??대룞?쇰줈 泥섎━
                    int wi = _wallSegments.IndexOf(ph.collider.gameObject);
                    if (wi >= 0) { MoveGate(wi); SetDsStatus($"臾??꾩튂 蹂寃???移?{wi}"); return; }

                    Vector3 pos = ph.point;
                    if (pos.x > -10.5f)
                        SetDsStatus("?깅꼍 ?덉そ?먮쭔 諛곗튂 媛?ν빀?덈떎");
                    else if (pos.x < -24f)
                        SetDsStatus("諛곗튂 媛??踰붿쐞瑜?踰쀬뼱?ъ뒿?덈떎");
                    else
                    {
                        int cost = _placeDefs[_selectedPlaceBldg].cost;
                        if (_gold < cost)
                            SetDsStatus($"怨⑤뱶 遺議?(?꾩슂: {cost}G)");
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

            // ?? ?섎퉬 紐⑤뱶: ?깅꼍 ?대┃ ??臾??꾩튂 蹂寃??????????????????
            if (_defenseMode && lmbDown && !overUI && _wallSegments.Count > 0)
            {
                var wr = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(wr, out RaycastHit wh, 200f))
                {
                    int wi = _wallSegments.IndexOf(wh.collider.gameObject);
                    if (wi >= 0) { MoveGate(wi); SetInfo($"臾??꾩튂 蹂寃???移?{wi}"); return; }
                }
            }

            // ?? 留덈쾿 ?쒖쟾 ?湲??????????????????????????????????
            if (_pendingSpell >= 0)
            {
                // 留??꾨젅?? 留덉슦???꾩튂??踰붿쐞 ?쒖떆湲?媛깆떊
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
                        string hint = vis ? $"[{SpellSystem.Defs[_pendingSpell].name}] ???꾩튂瑜??쒕옒洹맞룻겢由????볦쑝?몄슂  (?고겢由? 痍⑥냼)"
                                          : "?쒖빞媛 ?우? ?딆? 吏????留덈쾿 ?ъ슜 遺덇?";
                        SetInfo(hint);
                    }
                    else HideSpellRangeCircle();
                }

                // ?고겢由? 痍⑥냼
                if (rmb)
                {
                    HideSpellRangeCircle();
                    _pendingSpell = -1;
                    SetInfo("留덈쾿 ?쒖쟾 痍⑥냼");
                    return;
                }

                // 留덉슦??踰꾪듉 由대━利??쒕옒洹???or ?⑥닚 ?대┃): ?쒖쟾
                if (lmbUp && !overUI && Camera.main != null)
                {
                    HideSpellRangeCircle();
                    var castRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(castRay, out RaycastHit ch, 200f))
                    {
                        bool global  = GetSpellIndicatorRadius(_pendingSpell) < 0.1f;
                        bool revealed = _defenseMode || global || IsAreaRevealed(ch.point);
                        if (revealed) CastSpell(_pendingSpell, ch.point);
                        else          SetInfo("?쒖빞媛 ?우? ?딆? 吏??뿉??留덈쾿???ъ슜?????놁뒿?덈떎");
                    }
                    _pendingSpell = -1;
                    return;
                }
                return;
            }

            // ?? ?쒕옒洹??좏깮 ?????????????????????????????????
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

            // 嫄곕━???뺣젹 ??Unit/Building???덈뒗 ?덊듃瑜??곗꽑 ?좏깮
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
                // ?꾧뎔 ?좊떅 ?대┃ ???좏깮 (?ㅻⅨ ?좊떅???좏깮???곹깭?먯꽌???꾪솚 媛??
                if (hitAI != null && hitUnit != null && hitUnit.IsPlayerUnit)
                {
                    if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
                    Select(hitAI);
                    string hint = hitAI.AwaitingOrders ? "???고겢由?쑝濡?紐⑺몴 吏?? : "紐낅졊 ?섑뻾 以?;
                    SetInfo($"[{hitUnit.Data.unitName}] ?좏깮????{hint}");
                    return;
                }

                // ?좊떅???좏깮???곹깭 ?????대┃??紐낅졊?쇰줈 ?숈옉 (?곗튂 ?고겢由??泥?
                if (_selectedUnits.Count > 0)
                {
                    if ((hitUnit != null && !hitUnit.IsPlayerUnit) ||
                        (hitBldg != null && !hitBldg.IsPlayerBuilding))
                    {
                        foreach (var u in _selectedUnits) u.CommandAttack(hit.collider.transform);
                        SetInfo("怨듦꺽 紐낅졊 諛쒕졊 ??紐낅졊? 蹂寃쏀븷 ???놁뒿?덈떎");
                        DeselectAll();
                        return;
                    }
                    // 鍮?吏硫????대룞 紐낅졊
                    Vector3 dest = hit.point;
                    int i = 0;
                    foreach (var u in _selectedUnits)
                    {
                        float off = (i % 3 - 1) * 2f;
                        u.CommandMove(dest + new Vector3(0, 0, off));
                        i++;
                    }
                    SetInfo("?대룞 紐낅졊 諛쒕졊 ??紐낅졊? 蹂寃쏀븷 ???놁뒿?덈떎");
                    DeselectAll();
                    return;
                }

                DeselectAll();
                return;
            }

            // ?고겢由???紐낆떆??紐낅졊 (留덉슦???꾩슜, 湲곗〈 ?숈옉 ?좎?)
            if (rmb && _selectedUnits.Count > 0)
            {
                if ((hitUnit != null && !hitUnit.IsPlayerUnit) ||
                    (hitBldg != null && !hitBldg.IsPlayerBuilding))
                {
                    foreach (var u in _selectedUnits) u.CommandAttack(hit.collider.transform);
                    SetInfo("怨듦꺽 紐낅졊 諛쒕졊 ??紐낅졊? 蹂寃쏀븷 ???놁뒿?덈떎");
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
                    SetInfo("?대룞 紐낅졊 諛쒕졊 ??紐낅졊? 蹂寃쏀븷 ???놁뒿?덈떎");
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
                ? $"{count}湲??좏깮?????고겢由?쑝濡?紐⑺몴 吏???먮뒗 諛⑺뼢 踰꾪듉"
                : "?곸뿭???좏깮 媛?ν븳 ?좊떅 ?놁쓬");
        }

        // ?? ?좊떅 醫낅쪟 踰꾪듉 (?꾪닾 ?쒖옉 ???숈쟻 ?앹꽦) ??????????????
        private void BuildUnitTypeButtons()
        {
            // 湲곗〈 踰꾪듉 ?쒓굅
            foreach (Transform c in _unitTypeBar.transform) Destroy(c.gameObject);

            var types = new List<int>();
            for (int i = 0; i < Defs.Length; i++)
                if (_roster[i] > 0) types.Add(i);
            if (types.Count == 0) return;

            Lbl(_unitTypeBar, "TypeLbl", new Vector2(0f, 0.5f), new Vector2(52, 0),
                new Vector2(90, 40), "醫낅쪟 ?좏깮:", 13, new Color(0.8f,0.8f,0.8f));

            float btnW = 130f, gap = 8f;
            float totalW = btnW * types.Count + gap * (types.Count - 1);
            float startX = 105f - totalW / 2f + btnW / 2f; // ?쇰꺼 ?ㅻⅨ履쎈???

            for (int ti = 0; ti < types.Count; ti++)
            {
                int idx = types[ti];
                float x = startX + ti * (btnW + gap);
                Color c = Defs[idx].color * 0.55f; c.a = 1f;
                Btn(_unitTypeBar, $"Type{idx}", new Vector2(0f, 0.5f),
                    new Vector2(x, 0), new Vector2(btnW, 48),
                    $"{Defs[idx].name}  횞{_roster[idx]}", c,
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
                ? $"{Defs[defIdx].name} {count}湲??좏깮?????고겢由?紐⑺몴 吏???먮뒗 諛⑺뼢 踰꾪듉"
                : $"?湲?以묒씤 {Defs[defIdx].name} ?놁쓬");
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

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  諛⑺뼢 踰꾪듉 (?湲?以묒씤 ?꾩껜 蹂묐젰 ?쇨큵 ?뚭껄)
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void OrderAwaitingUnits(System.Action<TestSimpleUnitAI> cmd)
        {
            foreach (var u in _playerUnits)
                if (u != null && u.AwaitingOrders) cmd(u);
            SetInfo("紐낅졊 諛쒕졊 ?꾨즺");
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?대깽??泥섎━
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void OnBuildingDestroyed(BuildingDestroyedEvent evt)
        {
            if (_phase == Phase.GameOver) return;
            var b = evt.Building;

            // ?섎퉬 紐⑤뱶: ?뚮젅?댁뼱 嫄대Ъ ?뚭눼 泥섎━
            if (_defenseMode && b.IsPlayerBuilding)
            {
                _destroyedBuildings++;
                RefreshStatPanel();
                ShowResourcePopup(b.transform.position, "嫄대Ъ ?뚭눼!");
                if (b == _playerCastle) { EndGame(false, "?꾧뎔 ???⑤씫!"); return; }
                return;
            }

            if (b.IsPlayerBuilding) return;

            int gold  = b.Data.maxHp / 5;
            int valor = b == _enemyCastle ? 3 : 1;
            _gold         += gold;
            _valor        += valor;
            SyncOwnedResources();
            _earnedGold   += gold;
            _earnedValor  += valor;
            _destroyedBuildings++;

            RefreshUpgradeBtns();
            RefreshStatPanel();
            ShowResourcePopup(b.transform.position, $"+{gold}G  +{valor}臾닿났");
            SetInfo($"嫄대Ъ ?뚭눼! +{gold}G  +{valor} 臾닿났");

            if (b == _enemyCastle) { EndGame(true, "?????먮졊!"); return; }

            // ?⑥? ??嫄대Ъ???놁쑝硫??밸━
            bool allGone = true;
            foreach (var eb in _allEnemyBuildings)
                if (eb != null && eb.IsAlive) { allGone = false; break; }
            if (allGone) EndGame(true, "?꾩쟾 ?뺣났!");
        }

        private void EndGame(bool victory, string reason = "")
        {
            if (_phase == Phase.GameOver) return;
            _phase = Phase.GameOver;
            SetFogVisualLayerVisible(false);
            Time.timeScale = 0.25f;
            _resultPanel.SetActive(true);
            _resultText.text  = victory ? $"?밸━!\n{reason}" : $"?⑤같\n{reason}";
            _resultText.color = victory ? Color.yellow : Color.red;
            if (_resultStatsText != null)
                _resultStatsText.text =
                    $"?뚭눼 嫄대Ъ: {_destroyedBuildings}媛?n" +
                    $"?띾뱷 怨⑤뱶: +{_earnedGold}G\n" +
                    $"?띾뱷 臾닿났: +{_earnedValor}";
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  湲곗? 媛쒕컻
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private struct UpgDef { public string label; public int cost; public System.Action action; }

        private UpgDef[] GetUpgrades() => new[]
        {
            new UpgDef{label="?룈 湲곕퀝 ?닿툑\nValor 1",  cost=1, action=()=>{ _unlocked.Add(4); }},
            new UpgDef{label="??怨듭꽦湲??닿툑\nValor 2", cost=2, action=()=>{ _unlocked.Add(5); }},
            new UpgDef{label="???꾩닠 ?덈젴\nValor 1",   cost=1, action=()=>{ _dmgMult += 0.3f; SetInfo("怨듦꺽??+30%!"); }},
            new UpgDef{label="?썳 諛⑹뼱 媛뺥솕\nValor 2",   cost=2, action=()=>{ SetInfo("諛⑹뼱 媛뺥솕 ?꾨즺!"); }},
        };

        private void TryUpgrade(int idx)
        {
            var d = GetUpgrades();
            if (idx >= d.Length || _valor < d[idx].cost) return;
            _valor -= d[idx].cost;
            d[idx].action();
            RefreshUpgradeBtns();
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  UI 援ъ꽦
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
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

        private void BuildMobileLoopScreens()
        {
            _campaignHubScreen = new CampaignHubScreen(_canvas, _font, EnterBattle, ShowAttackPrep, ShowBaseManagement);
            _baseManagementScreen = new BaseManagementScreen(_canvas, _font, CollectStoredResources, ShowCampaignHub);
            _attackPrepScreen = new AttackPrepScreen(_canvas, _font, EnterBattle, ShowArmyEditor, ShowBaseManagement, ShowCampaignHub);
            _mobileBattleHud = new MobileBattleHud(_canvas, _font);
            _mobileBattleHud.SetCommandHandler(HandleMobileBattleCommand);
            _campaignHubScreen.SetVisible(false);
            _baseManagementScreen.SetVisible(false);
            _attackPrepScreen.SetVisible(false);
            _mobileBattleHud.SetVisible(false);
        }

        private void HandleMobileBattleCommand(MobileBattleHud.CommandKind command)
        {
            if (_phase != Phase.Battle) return;
            _pendingSpell = -1;
            HideSpellRangeCircle();

            switch (command)
            {
                case MobileBattleHud.CommandKind.Rally:
                    DeselectAll();
                    foreach (var unit in _playerUnits)
                    {
                        if (unit != null && unit.AwaitingOrders) Select(unit);
                    }
                    SetInfo(_selectedUnits.Count > 0
                        ? $"Mobile rally selected {_selectedUnits.Count} units. Tap a target or Attack."
                        : "No awaiting units to rally.");
                    break;

                case MobileBattleHud.CommandKind.Attack:
                    var target = _defenseMode ? _playerCastle : _enemyCastle;
                    if (target == null)
                    {
                        SetInfo("No priority target available.");
                        break;
                    }
                    if (_selectedUnits.Count > 0)
                    {
                        foreach (var unit in _selectedUnits) unit.CommandAttack(target.transform);
                        DeselectAll();
                    }
                    else
                    {
                        OrderAwaitingUnits(unit => unit.CommandAttack(target.transform));
                    }
                    SetInfo("Mobile attack command issued.");
                    break;

                case MobileBattleHud.CommandKind.Hold:
                    foreach (var unit in _selectedUnits) unit.SetAwaitingOrders();
                    DeselectAll();
                    SetInfo("Mobile hold command armed. Units are waiting for a new order.");
                    break;

                case MobileBattleHud.CommandKind.Spells:
                    for (int i = 0; i < SpellSystem.Defs.Length; i++)
                    {
                        if (!SpellSystem.HasCharge((SpellType)i)) continue;
                        ActivateSpell(i);
                        return;
                    }
                    SetInfo("No spell charges available.");
                    break;
            }
        }
        private void ShowCampaignHub()
        {
            if (_phase != Phase.Prep) return;
            _prepPanel.SetActive(false);
            if (_dsHud != null) _dsHud.SetActive(false);
            _campaignHubScreen?.SetVisible(true);
            _baseManagementScreen?.SetVisible(false);
            _attackPrepScreen?.SetVisible(false);
            RefreshMobileLoopScreens();
        }

        private void ShowAttackPrep()
        {
            if (_phase != Phase.Prep) return;
            _campaignHubScreen?.SetVisible(false);
            _baseManagementScreen?.SetVisible(false);
            _attackPrepScreen?.SetVisible(true);
            _prepPanel.SetActive(false);
            RefreshMobileLoopScreens();
        }

        private void ShowArmyEditor()
        {
            if (_phase != Phase.Prep) return;
            _campaignHubScreen?.SetVisible(false);
            _baseManagementScreen?.SetVisible(false);
            _attackPrepScreen?.SetVisible(false);
            _prepPanel.SetActive(true);
            RefreshPrepGold();
        }

        private void ShowBaseManagement()
        {
            if (_phase != Phase.Prep) return;
            _prepPanel.SetActive(false);
            if (_dsHud != null) _dsHud.SetActive(false);
            _campaignHubScreen?.SetVisible(false);
            _baseManagementScreen?.SetVisible(true);
            _attackPrepScreen?.SetVisible(false);
            RefreshMobileLoopScreens();
        }

        private RaidForecast BuildCurrentRaidForecast()
        {
            if (_resourceStorage == null || _ownedResources == null) return null;
            return RaidLossCalculator.Calculate(
                _resourceStorage.Stored,
                _ownedResources,
                RaidOutcome.ClearFailure,
                _resourceStorage.ProtectionRate);
        }

        private void RefreshMobileLoopScreens()
        {
            if (_ownedResources == null || _resourceStorage == null) return;

            SyncOwnedResources();
            var forecast = BuildCurrentRaidForecast();
            _campaignHubScreen?.Refresh(_ownedResources, _resourceStorage.Stored, forecast);
            _attackPrepScreen?.Refresh(
                BuildRosterSummary(),
                "Fireball / Heal / Freeze",
                "Expected defense: walls, towers, central keep");
            _baseManagementScreen?.Refresh(
                _ownedResources,
                _resourceStorage.Stored,
                forecast,
                _resourceStorage.HeadquartersLevel,
                _resourceStorage.GetHeadquartersCapacity(ResourceType.Gold));
        }

        private void CollectStoredResources()
        {
            if (_resourceStorage == null || _ownedResources == null) return;

            _resourceStorage.CollectAll(_ownedResources);
            _gold = _ownedResources.Get(ResourceType.Gold);
            RefreshPrepGold();
            RefreshBuyBtns();
            RefreshSpecialBldgUI();
            RefreshSpellBuyUI();
            RefreshMobileLoopScreens();
        }

        private string BuildRosterSummary()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Defs.Length; i++)
            {
                if (_roster[i] <= 0) continue;
                if (sb.Length > 0) sb.Append(" / ");
                sb.Append($"{Defs[i].name} x{_roster[i]}");
            }
            return sb.Length == 0 ? "No troops selected" : sb.ToString();
        }

        // ?? 以鍮??붾㈃ ?????????????????????????????????????????
        // 紐⑤뱺 ?붿냼: anchor=(0.5,1), pivot=(0.5,1) ???⑤꼸 ?곷떒 以묒븰 湲곗? Y ?꾩쟻
        private void BuildPrepPanel()
        {
            _prepPanel = NewFillPanel(_canvas, "PrepPanel", new Color(0.04f, 0.04f, 0.10f, 0.93f));

            var a = new Vector2(0.5f, 1f); // ?곷떒 以묒븰 ?듭빱

            Lbl(_prepPanel, "Title", a, new Vector2(0, -38),
                new Vector2(480, 52), "?? ?꾪닾 以鍮?, 36, Color.white);

            _prepGoldText = Lbl(_prepPanel, "Gold", a, new Vector2(0, -100),
                new Vector2(260, 38), $"怨⑤뱶: {_gold}", 24, Color.yellow);

            // 怨듦꺽/?섎퉬 紐⑤뱶 ?좉?
            _modeToggleBtn = Btn(_prepPanel, "ModeToggle", a, new Vector2(0, -138),
                new Vector2(260, 36), "怨듦꺽 紐⑤뱶 ??, new Color(0.15f, 0.4f, 0.15f), ToggleMode);
            _modeToggleLbl = _modeToggleBtn.GetComponentInChildren<Text>();

            // ?좊떅 援щℓ 踰꾪듉 ??3??2??
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
                AddButtonArt(b, UnitArtKey(i), new Vector2(-68f, 0f), new Vector2(58f, 76f), reserveLeftTextSpace: true);
                _buyBtns[i]   = b;
                _buyLabels[i] = b.GetComponentInChildren<Text>();
            }

            // 留덈쾿 援щℓ ?뱀뀡 ??怨듦꺽 紐⑤뱶 ?꾩슜 (而⑦뀒?대꼫濡?臾띠뼱???쒕쾲???좉?)
            _spellSectionRoot = new GameObject("SpellSection");
            _spellSectionRoot.transform.SetParent(_prepPanel.transform, false);
            var ssRt = _spellSectionRoot.AddComponent<RectTransform>();
            ssRt.anchorMin = ssRt.anchorMax = ssRt.pivot = a;
            ssRt.anchoredPosition = Vector2.zero; ssRt.sizeDelta = Vector2.zero;

            Lbl(_spellSectionRoot, "SpellSec", a, new Vector2(0, -390),
                new Vector2(400, 30), "?? 留덈쾿 援щℓ ??", 17, new Color(0.55f, 0.78f, 1f));

            float spellRow1Y = -458f, spellRow2Y = -560f;
            for (int i = 0; i < 5; i++)
            {
                int si = i;
                float sx = i < 3 ? startX + i * colW : startX + (i - 3) * colW + colW * 0.5f;
                float sy = i < 3 ? spellRow1Y : spellRow2Y;
                var sb = Btn(_spellSectionRoot, $"SpellBuy{i}", a, new Vector2(sx, sy), btnSize,
                    BuildSpellBuyLabel(i), SpellBuyColor(i), () => TryBuySpell(si));
                AddButtonArt(sb, SpellArtKey(i), new Vector2(-68f, 0f), new Vector2(58f, 58f), reserveLeftTextSpace: true);
                _spellBuyBtns[i] = sb;
                _spellBuyLbls[i] = sb.GetComponentInChildren<Text>();
            }

            _rosterText = Lbl(_prepPanel, "Roster", a, new Vector2(0, -632),
                new Vector2(720, 36), "蹂묐젰 ?놁쓬", 20, new Color(0.9f, 0.9f, 0.8f));
            if (_rosterText != null) _rosterText.alignment = TextAnchor.UpperCenter;

            Btn(_prepPanel, "HubBtn", new Vector2(0f, 1f), new Vector2(70f, -34f),
                new Vector2(120f, 44f), "Hub", MobileHudTheme.SecondaryButton, ShowCampaignHub);
            Btn(_prepPanel, "BaseBtn", new Vector2(0f, 1f), new Vector2(200f, -34f),
                new Vector2(120f, 44f), "Base", MobileHudTheme.PrimaryButton, ShowBaseManagement);

            _startBattleBtn = Btn(_prepPanel, "StartBtn", a, new Vector2(0, -681),
                new Vector2(250, 66), "異쒖쟾! ??, new Color(0.1f, 0.55f, 0.15f), EnterBattle);
            _enterSetupBtn = Btn(_prepPanel, "SetupBtn", a, new Vector2(0, -681),
                new Vector2(250, 66), "吏꾪삎 援ъ꽦 ?쒖옉 ??, new Color(0.55f, 0.3f, 0.05f), EnterDefenseSetup);

            // ?곗륫 ?⑤꼸 ???뱀닔 嫄대Ъ (?섎퉬 紐⑤뱶 ?꾩슜)
            _rightPanel = NewAnchoredPanel(_prepPanel, "PrepRightPanel",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-390, 50), new Vector2(-5, -5),
                new Color(0.06f, 0.06f, 0.18f, 0.92f));
            BuildSpecialBldgPanel(_rightPanel);

            // 珥덇린 媛?쒖꽦: 怨듦꺽 紐⑤뱶 湲곕낯
            ApplyModePrepVisibility();
        }

        private string BuildBuyLabel(int i)
        {
            var d = Defs[i];
            bool locked = i >= 4 && !BuildingEffectSystem.IsUnitUnlocked(i) && !_unlocked.Contains(i);
            int  effCost = Mathf.RoundToInt(d.cost * BuildingEffectSystem.GetCostMultiplier());
            string lockInfo = i == 4 ? "?ъ씤??Lv1+" : "?ъ씤??Lv2+";
            string sub = locked ? $"?뵏 {lockInfo}" : $"{effCost}G";
            return $"[{d.name}]  {d.desc}\n{sub}   蹂댁쑀: {_roster[i]}";
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
            SyncOwnedResources();
            if (_prepGoldText != null) _prepGoldText.text = $"怨⑤뱶: {_gold}";
            RefreshMobileLoopScreens();
        }

        private void SyncOwnedResources()
        {
            if (_ownedResources == null) return;
            _ownedResources.Set(ResourceType.Gold, _gold);
            _ownedResources.Set(ResourceType.Honor, _valor);
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
            var sb = new StringBuilder("蹂댁쑀 蹂묐젰:  ");
            bool any = false;
            for (int i = 0; i < Defs.Length; i++)
            {
                if (_roster[i] <= 0) continue;
                if (any) sb.Append("   ");
                sb.Append($"{Defs[i].name} 횞{_roster[i]}");
                any = true;
            }
            if (_rosterText != null) _rosterText.text = any ? sb.ToString() : "蹂묐젰 ?놁쓬";
        }

        // ?? ?뱀닔 嫄대Ъ ?⑤꼸 ?????????????????????????????????????
        private void BuildSpecialBldgPanel(GameObject parent)
        {
            var a = new Vector2(0.5f, 1f);
            Lbl(parent, "BldgTitle", a, new Vector2(0, -12), new Vector2(370, 28),
                "??? ?뱀닔 嫄대Ъ ?낃렇?덉씠?????", 15, new Color(0.95f, 0.85f, 0.5f));

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
            string levelStr = maxed ? "Lv MAX" : (unlocked ? $"Lv {lv} ??{lv + 1}" : "?뵏 怨듬갑 ?꾩슂");
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
            RefreshBuyBtns();     // Blacksmith 蹂寃????좊떅 鍮꾩슜 ?쒖떆 媛깆떊
            RefreshSpellBuyUI();  // 怨⑤뱶 蹂寃?
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
            string chargeStr = maxed ? "MAX" : $"{charges}/{def.maxCharges}??;
            return $"[{def.name}]  {def.desc}\n蹂댁쑀: {chargeStr}";
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

        // ?? 留덈쾿 ?꾪닾 ??????????????????????????????????????????
        private string BuildSpellBattleLabel(int i)
        {
            int charges = SpellSystem.GetCharges((SpellType)i);
            return $"{SpellSystem.Defs[i].name}\n({charges}??";
        }

        private void ActivateSpell(int si)
        {
            if (!SpellSystem.HasCharge((SpellType)si)) return;
            _pendingSpell = si;
            SetInfo($"[{SpellSystem.Defs[si].name}] ???쒖쟾???꾩튂瑜??대┃?섏꽭?? (?고겢由? 痍⑥냼)");
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
                    ShowResourcePopup(worldPos, "?붿뿼援?");
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
                        ShowResourcePopup(ltTarget.position, "踰덇컻!");
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
                        ShowResourcePopup(healTarget.transform.position, "移섏쑀!");
                    }
                    break;

                case SpellType.Freeze:
                    StartCoroutine(FreezeEnemies(5f));
                    ShowResourcePopup(worldPos, "鍮숆껐!");
                    break;

                case SpellType.Rage:
                    StartCoroutine(RagePlayerUnits(8f));
                    ShowResourcePopup(worldPos, "遺꾨끂!");
                    break;
            }
            RefreshSpellBattleBtns();
            SetInfo($"[{SpellSystem.Defs[si].name}] ?쒖쟾 ?꾨즺");
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

        // ?? ?꾪닾 HUD ??????????????????????????????????????????
        private void BuildBattleHud()
        {
            _battleHud = new GameObject("BattleHud");
            _battleHud.transform.SetParent(_canvas.transform, false);
            var rt = _battleHud.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            // ?곷떒 諛?諛곌꼍
            var topBar = NewAnchoredPanel(_battleHud, "TopBar",
                new Vector2(0,1), new Vector2(1,1), new Vector2(0,-48), Vector2.zero,
                new Color(0,0,0,0.55f));

            var tl = new Vector2(0f, 1f);
            var tc = new Vector2(0.5f, 1f);
            var tr = new Vector2(1f, 1f);

            _timerText    = Lbl(topBar, "Timer",   tc, new Vector2(-60,-7), new Vector2(130,34), "??0s",    20, Color.white);
            _valorHudText = Lbl(topBar, "Valor",   tl, new Vector2(10, -7), new Vector2(160,34), "臾닿났: 0",  20, new Color(1f,0.8f,0.2f));
            _enemyHpText  = Lbl(topBar, "EnemyHp", tr, new Vector2(-10,-7), new Vector2(220,34), "?곸꽦: 900",20, new Color(1f,0.4f,0.4f));
            if (_enemyHpText != null) _enemyHpText.alignment = TextAnchor.UpperRight;

            // ?좊떅 醫낅쪟 ?좏깮 諛?(?숈쟻 踰꾪듉? DeployArmy?먯꽌 ?앹꽦)
            _unitTypeBar = NewAnchoredPanel(_battleHud, "UnitTypeBar",
                new Vector2(0,0), new Vector2(1,0),
                new Vector2(0,118), new Vector2(0,178),
                new Color(0,0,0,0.4f));

            // 留덈쾿 鍮좊Ⅸ ?쒖쟾 諛?
            var spellBar = NewAnchoredPanel(_battleHud, "SpellBar",
                new Vector2(0,0), new Vector2(1,0),
                new Vector2(0,178), new Vector2(0,232),
                new Color(0.02f,0.04f,0.18f,0.88f));

            Lbl(spellBar, "SpellLbl", new Vector2(0f,0.5f), new Vector2(46,0),
                new Vector2(82,44), "留덈쾿:", 15, new Color(0.7f,0.8f,1f));

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

            // ?섎떒 ?덈궡 + 諛⑺뼢 踰꾪듉 諛곌꼍
            var botBar = NewAnchoredPanel(_battleHud, "BotBar",
                new Vector2(0,0), new Vector2(1,0), Vector2.zero, new Vector2(0,118),
                new Color(0,0,0,0.55f));

            _infoText = Lbl(botBar, "Info", new Vector2(0.5f,1), new Vector2(0,-6),
                new Vector2(800,28), "", 16, new Color(0.9f,0.9f,0.7f));
            if (_infoText != null) _infoText.alignment = TextAnchor.UpperCenter;

            // 諛⑺뼢 踰꾪듉 7媛?(?湲??좊떅 ?쇨큵 ?뚭껄)
            var tactics = new (string lbl, Color col, System.Action act)[]
            {
                ("?꾧뎔 ?좏깮",   new Color(0.2f,0.35f,0.55f), ()=>{ DeselectAll(); foreach(var u in _playerUnits) if(u!=null&&u.AwaitingOrders) Select(u); }),
                ("?꾧뎔 ?뚭꺽",   new Color(0.65f,0.1f,0.1f),  ()=> OrderAwaitingUnits(u=>u.CommandAttack(_enemyCastle?.transform))),
                ("醫뚯륫 怨듦꺽",   new Color(0.2f,0.4f,0.22f),  ()=> OrderAwaitingUnits(u=>u.CommandMove(new Vector3(6,0, 7)))),
                ("以묒븰 ?뚰뙆",   new Color(0.2f,0.4f,0.22f),  ()=> OrderAwaitingUnits(u=>u.CommandMove(new Vector3(6,0, 0)))),
                ("?곗륫 怨듦꺽",   new Color(0.2f,0.4f,0.22f),  ()=> OrderAwaitingUnits(u=>u.CommandMove(new Vector3(6,0,-7)))),
                ("?좏깮?믩ぉ??,  new Color(0.35f,0.25f,0.5f),  ()=> SetInfo("?고겢由?쑝濡?紐⑺몴瑜?吏?뺥븯?몄슂")),
                ("?꾧뎔 ?먮룞",   new Color(0.3f,0.3f,0.3f),   ()=> OrderAwaitingUnits(u=>u.CommandAttack(null))),
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

        // ?? 湲곗? 媛쒕컻 ?⑤꼸 (?곗륫) ?????????????????????????????
        private void BuildUpgradePanel()
        {
            _upgradePanel = NewAnchoredPanel(_canvas, "UpgradePanel",
                new Vector2(1,0), new Vector2(1,1),
                new Vector2(-185,118), Vector2.zero,
                new Color(0.05f,0.05f,0.12f,0.88f));

            Lbl(_upgradePanel, "Title", new Vector2(0.5f,1), new Vector2(0,-14),
                new Vector2(175,32), "湲곗? 媛쒕컻", 18, Color.white);

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
            if (_valorHudText != null) _valorHudText.text = $"臾닿났: {_valor}";
        }

        // ?? ?먯썝 ?꾪솴 ?⑤꼸 (諛고? 以??곗륫 ?섎떒) ???????????????????
        private void BuildStatPanel()
        {
            var panel = NewAnchoredPanel(_battleHud, "StatPanel",
                new Vector2(1,0), new Vector2(1,0),
                new Vector2(-185, 178), new Vector2(0, 310),
                new Color(0.04f, 0.06f, 0.14f, 0.85f));

            Lbl(panel, "Title", new Vector2(0.5f,1), new Vector2(0,-10),
                new Vector2(175,26), "?꾪닾 ?띾뱷", 15, Color.white);

            _statGoldText  = Lbl(panel, "Gold",  new Vector2(0.5f,1), new Vector2(0,-42),
                new Vector2(175,28), "怨⑤뱶   +0G",        16, new Color(1f,0.95f,0.4f));
            _statValorText = Lbl(panel, "Valor", new Vector2(0.5f,1), new Vector2(0,-74),
                new Vector2(175,28), "臾닿났   +0",         16, new Color(0.6f,0.9f,1f));
            _statBldgText  = Lbl(panel, "Bldg",  new Vector2(0.5f,1), new Vector2(0,-106),
                new Vector2(175,28), "?뚭눼   0媛?,        16, new Color(1f,0.6f,0.4f));
        }

        private void RefreshStatPanel()
        {
            if (_statGoldText  != null) _statGoldText.text  = $"怨⑤뱶   +{_earnedGold}G";
            if (_statValorText != null) _statValorText.text = $"臾닿났   +{_earnedValor}";
            if (_statBldgText  != null) _statBldgText.text  = $"?뚭눼   {_destroyedBuildings}媛?;
        }

        // ?? ?먯썝 ?띾뱷 ?앹뾽 (?뚮줈???띿뒪?? ???????????????????????
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

        // ?? 寃곌낵 ?⑤꼸 ?????????????????????????????????????????
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
                new Vector2(140,52), "?ㅼ떆 ?쒖옉", new Color(0.1f,0.5f,0.1f), ()=>{
                    Time.timeScale = 1f;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                });
            Btn(_resultPanel, "Quit", new Vector2(0.5f,0.5f), new Vector2(80,-80),
                new Vector2(140,52), "醫낅즺", new Color(0.5f,0.1f,0.1f), Application.Quit);
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  UI ?ы띁
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
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

        // stretch 諛⑹떇 ?⑤꼸 (offsetMin/offsetMax)
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

        private static string UnitArtKey(int i)
        {
            switch (i)
            {
                case 0: return "Units/knight";
                case 1: return "Units/archer";
                case 2: return "Units/mage";
                case 3: return "Units/knight";
                case 4: return "Units/cavalry";
                case 5: return "Units/siege";
                default: return null;
            }
        }

        private static string SpellArtKey(int i)
        {
            switch (i)
            {
                case 0: return "Icons/fireball";
                case 1: return "Icons/lightning";
                case 2: return "Icons/heal";
                case 3: return "Icons/freeze";
                case 4: return "Icons/rage";
                default: return null;
            }
        }

        private void AddButtonArt(Button button, string artKey, Vector2 anchoredPosition, Vector2 size, bool reserveLeftTextSpace)
        {
            if (button == null) return;
            var sprite = GeneratedArtLibrary.LoadSprite(artKey, 160f);
            if (sprite == null) return;

            var go = new GameObject("ArtIcon");
            go.transform.SetParent(button.transform, false);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;

            if (!reserveLeftTextSpace) return;
            var label = button.GetComponentInChildren<Text>();
            if (label == null) return;
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.offsetMin = new Vector2(62f, 0f);
            label.rectTransform.offsetMax = new Vector2(-8f, 0f);
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  ?섎퉬 吏꾪삎 援ъ꽦
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        private void EnterDefenseSetup()
        {
            if (_defenseSetupActive) return;
            _defenseSetupActive  = true;
            _selectedPlaceBldg   = -1;
            _stagingCol = _stagingRow = 0;

            // 湲곕낯 湲곗?: ??+ ?먮룞 ?깅꼍
            _playerCastle = MakePlayerBuilding("PlayerCastle",
                new Vector3(-21f, 1.5f, 0f), 900,
                MobileVisualStyle.FriendlyBlue, new Vector3(4f, 3f, 4f));
            AddToonyDecoration("blue_banner", new Vector3(-18.5f, 0f, 2.9f), Vector3.one * 0.9f, 0f);
            AddToonyDecoration("blue_banner", new Vector3(-18.5f, 0f, -2.9f), Vector3.one * 0.9f, 0f);
            GenerateAutoWall(-10f, -8f, 8f);

            // 移대찓?쇰? ?뚮젅?댁뼱 援ъ뿭 以묒떖?쇰줈 ?대룞
            var cam = Camera.main;
            if (cam != null)
                cam.transform.SetPositionAndRotation(
                    new Vector3(-10f, 28f, -22f), Quaternion.Euler(50f, 0f, 0f));

            _prepPanel.SetActive(false);
            if (_dsHud != null) _dsHud.SetActive(true);

            RefreshDsGold();
            RefreshDsUnitBtns();
            RefreshDsSpecBtns();
            SetDsStatus("諛⑹뼱???깅꼍???좏깮?????깅꼍 ?덉そ???대┃??諛곗튂  |  ?좊떅 踰꾪듉?쇰줈 利됱떆 ?앹궛  |  ?고겢由? 諛곗튂 痍⑥냼");
        }

        private void BuildDefenseSetupHud()
        {
            _dsHud = new GameObject("DefenseSetupHud");
            _dsHud.transform.SetParent(_canvas.transform, false);
            var rt = _dsHud.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            var a = new Vector2(0.5f, 1f);

            // ?? ?쇱そ ?⑤꼸: 嫄대Ъ 諛곗튂 + ?좊떅 ?앹궛 ??????????????????????
            var left = NewAnchoredPanel(_dsHud, "DS_Left",
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(5f, 5f), new Vector2(222f, -5f),
                new Color(0.04f, 0.06f, 0.14f, 0.93f));

            Lbl(left, "Title", a, new Vector2(0f, -16f), new Vector2(212f, 28f),
                "?섎퉬 吏꾪삎 援ъ꽦", 16, Color.white);
            _dsGoldText = Lbl(left, "Gold", a, new Vector2(0f, -44f), new Vector2(212f, 22f),
                $"怨⑤뱶: {_gold}", 14, Color.yellow);

            // 嫄대Ъ 諛곗튂 ?붾젅??
            Lbl(left, "BldgHdr", a, new Vector2(0f, -72f), new Vector2(212f, 18f),
                "?? 嫄대Ъ 諛곗튂 ??", 11, new Color(0.7f, 0.85f, 1f));

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

            // ?좊떅 ?앹궛
            float uHdrY = -94f - _placeDefs.Length * 46f - 10f;
            Lbl(left, "UnitHdr", a, new Vector2(0f, uHdrY), new Vector2(212f, 18f),
                "?? ?좊떅 ?앹궛 ??", 11, new Color(0.7f, 1f, 0.7f));

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

            // ?꾪닾 ?쒖옉 (?섎떒 怨좎젙)
            Btn(left, "BattleStart", new Vector2(0.5f, 0f), new Vector2(0f, 8f),
                new Vector2(208f, 52f), "?꾪닾 ?쒖옉 ??,
                new Color(0.6f, 0.12f, 0.12f), EnterBattle);

            // ?? ?ㅻⅨ履??⑤꼸: ?뱀닔 嫄대Ъ ?낃렇?덉씠???????????????????????
            var right = NewAnchoredPanel(_dsHud, "DS_Right",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-222f, 5f), new Vector2(-5f, -5f),
                new Color(0.06f, 0.06f, 0.18f, 0.93f));

            Lbl(right, "SpecHdr", a, new Vector2(0f, -12f), new Vector2(208f, 24f),
                "?? ?뱀닔 嫄대Ъ ?낃렇?덉씠????", 13, new Color(0.95f, 0.85f, 0.5f));

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

            // ?? ?섎떒 ?곹깭 諛????????????????????????????????????????????
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
                ? $"[{_placeDefs[_selectedPlaceBldg].label}] ?좏깮?????깅꼍 ?덉そ ?대┃?쇰줈 諛곗튂  |  ?고겢由? 痍⑥냼"
                : "?좏깮 ?댁젣");
        }

        private void BuyUnitDefenseSetup(int idx)
        {
            bool locked = idx >= 4 && !BuildingEffectSystem.IsUnitUnlocked(idx) && !_unlocked.Contains(idx);
            int effCost = Mathf.RoundToInt(Defs[idx].cost * BuildingEffectSystem.GetCostMultiplier());
            if (locked || _gold < effCost) { SetDsStatus("怨⑤뱶 遺議??먮뒗 ?좉툑 ?곹깭"); return; }
            _gold -= effCost;
            _roster[idx]++;

            // ???ㅼそ ?ㅽ뀒?댁쭠 援ъ뿭??利됱떆 諛곗튂
            float x = -19f - _stagingCol * 2.2f;
            float z = (_stagingRow - 2) * 2.4f;
            var ai = SpawnUnit(idx, true, new Vector3(x, 0f, z));
            ai.SetAwaitingOrders();
            _playerUnits.Add(ai);
            if (++_stagingRow >= 5) { _stagingRow = 0; _stagingCol++; }

            RefreshDsGold();
            RefreshDsUnitBtns();
            SetDsStatus($"{Defs[idx].name} ?앹궛 ?꾨즺 ???⑹깋 留??좊떅???대┃ ???고겢由?쑝濡??대룞/怨듦꺽 紐낅졊 媛??);
        }

        private void PlacePlayerTower(Vector3 hitPos)
        {
            hitPos.y = 1f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PTower_Custom";
            go.transform.position = hitPos;
            go.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            Paint(go, MobileVisualStyle.FriendlyBlue);
            AddTowerDecor(go, MobileVisualStyle.FriendlyBlue);
            ApplyBuildingVisual(go, "tower", "Buildings/tower", new Vector3(0f, 1.35f, -0.15f), new Vector2(3.0f, 3.0f), -1f, Vector3.one * 1.2f);
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = "諛⑹뼱??; data.maxHp = 220;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: true);
            _allPlayerBuildings.Add(b);
            go.AddComponent<TestTowerAI>().Setup(isPlayer: true, range: 9f, dmg: 18, cooldown: 1.2f);
            SetDsStatus($"諛⑹뼱??諛곗튂 ?꾨즺  |  ?⑥? 怨⑤뱶: {_gold}G");
        }

        private void PlacePlayerWall(Vector3 hitPos)
        {
            hitPos.y = 1f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PCustomWall";
            go.transform.position = hitPos;
            go.transform.localScale = new Vector3(0.8f, 2f, 2.3f);
            Paint(go, MobileVisualStyle.StoneWarm);
            AddWallCap(go);
            ApplyBuildingVisual(go, "wall", "Buildings/wall", new Vector3(0f, 1.2f, -0.15f), new Vector2(3.0f, 3.0f), -1f, Vector3.one * 1.15f);
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.buildingName = "?깅꼍"; data.maxHp = 400;
            var b = go.AddComponent<Building>();
            b.Initialize(data, isPlayerBuilding: true);
            _allPlayerBuildings.Add(b);
            SetDsStatus($"?깅꼍 諛곗튂 ?꾨즺  |  ?⑥? 怨⑤뱶: {_gold}G");
        }

        private void TryUpgradeSpecialBuildingDs(int idx)
        {
            TryUpgradeSpecialBuilding(idx);
            RefreshDsGold();
            RefreshDsSpecBtns();
        }

        private void RefreshDsGold()
        {
            if (_dsGoldText != null) _dsGoldText.text = $"怨⑤뱶: {_gold}";
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
            string costStr = locked ? "?좉툑" : $"{effCost}G";
            return $"[{d.name}]\n{costStr} 蹂댁쑀:{_roster[i]}";
        }

        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
        //  留덈쾿 踰붿쐞 ?쒖떆湲?& FOW ? 異붿쟻
        // ?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧?먥븧??
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

        /// <summary>留덈쾿蹂?踰붿쐞 ?쒖떆 諛섍꼍. 0 = ?꾩뿭 留덈쾿(??誘명몴??.</summary>
        private static float GetSpellIndicatorRadius(int si) => (SpellType)si switch
        {
            SpellType.Fireball  => 3f,
            SpellType.Lightning => 1.5f,
            SpellType.Heal      => 1.5f,
            SpellType.Freeze    => 0f,   // ???????꾩뿭
            SpellType.Rage      => 0f,   // ???꾧뎔 ???꾩뿭
            _                   => 1f,
        };

        /// <summary>FOW: ?대떦 以묒떖?먯꽌 諛섍꼍 ??????곴뎄 怨듦컻 紐⑸줉??異붽?.</summary>
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

        /// <summary>?대떦 ?꾩튂媛 怨쇨굅????踰덉씠?쇰룄 ?쒖빞???ㅼ뼱?붾뒗吏 ?뺤씤.</summary>
        private bool IsAreaRevealed(Vector3 pos)
        {
            int cx = Mathf.RoundToInt(pos.x / FowCellSize);
            int cz = Mathf.RoundToInt(pos.z / FowCellSize);
            return _revealedCells.Contains(new Vector2Int(cx, cz));
        }
    }
}

