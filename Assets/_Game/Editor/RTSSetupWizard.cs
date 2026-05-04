// Assets/_Game/Editor/RTSSetupWizard.cs
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MedievalRTS.Base;
using MedievalRTS.Battle;
using MedievalRTS.Buildings;
using MedievalRTS.Campaign;
using MedievalRTS.Core;
using MedievalRTS.Data;
using MedievalRTS.Grid;
using MedievalRTS.UI;
using MedievalRTS.Units;

namespace MedievalRTS.Editor
{
    public static class RTSSetupWizard
    {
        const string PrefabRoot = "Assets/_Game/Prefabs";
        const string MatRoot    = "Assets/_Game/Materials";
        const string SORoot     = "Assets/_Game/ScriptableObjects";
        const string SceneRoot  = "Assets/_Game/Scenes";

        // ═══════════════════════════════════════════════════════
        //  MENU ITEMS
        // ═══════════════════════════════════════════════════════

        [MenuItem("Medieval RTS/1. Create Assets (Prefabs + ScriptableObjects)", priority = 10)]
        public static void CreateAssets()
        {
            EnsureFolders();
            AddTags();
            AddLayer("Ground");
            var mats   = CreateMaterials();
            var prefabs = CreatePrefabs(mats);
            CreateScriptableObjects(prefabs);
            CreateUIButtonPrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Assets created! Now run Step 2.");
        }

        [MenuItem("Medieval RTS/2. Setup All Scenes", priority = 11)]
        public static void SetupScenes()
        {
            var unitDatas     = LoadAll<UnitData>(SORoot + "/Units");
            var buildingDatas = LoadAll<BuildingData>(SORoot + "/Buildings");
            var stageDatas    = LoadAll<StageData>(SORoot + "/Stages");
            var uiBtn   = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/UI/UnitButtonPrefab.prefab");
            var buildBtn = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/UI/BuildingButtonPrefab.prefab");

            OpenAndSetup(SceneRoot + "/MainMenu.unity",    () => SetupMainMenuScene(stageDatas));
            OpenAndSetup(SceneRoot + "/BaseBuilder.unity", () => SetupBaseBuilderScene(buildingDatas, buildBtn));
            OpenAndSetup(SceneRoot + "/Battle.unity",      () => SetupBattleScene(unitDatas, uiBtn));
            OpenAndSetup(SceneRoot + "/Result.unity",      () => SetupResultScene());

            AssetDatabase.SaveAssets();
            Debug.Log("✅ All scenes set up!");
            Debug.Log("⚠️  NavMesh: Battle 씬 열기 → Plane 선택 → Navigation Static 체크 → Window > AI > Navigation > Bake");
        }

        [MenuItem("Medieval RTS/Run Full Setup (1+2)", priority = 0)]
        public static void RunFullSetup()
        {
            CreateAssets();
            SetupScenes();
        }

        // ═══════════════════════════════════════════════════════
        //  FOLDERS / TAGS / LAYERS
        // ═══════════════════════════════════════════════════════

        static void EnsureFolders()
        {
            var folders = new[]
            {
                PrefabRoot, PrefabRoot+"/Units", PrefabRoot+"/Buildings", PrefabRoot+"/UI",
                MatRoot,
                SORoot+"/Units", SORoot+"/Buildings", SORoot+"/Stages",
                "Assets/_Game/Editor"
            };
            foreach (var path in folders)
            {
                if (!AssetDatabase.IsValidFolder(path))
                {
                    var parent = Path.GetDirectoryName(path).Replace('\\', '/');
                    AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
                }
            }
        }

        static void AddTags()
        {
            foreach (var t in new[] { "PlayerUnit", "EnemyUnit", "PlayerBuilding", "EnemyBuilding" })
                AddTag(t);
        }

        static void AddTag(string tag)
        {
            var tm = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tm.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            int n = tags.arraySize;
            tags.InsertArrayElementAtIndex(n);
            tags.GetArrayElementAtIndex(n).stringValue = tag;
            tm.ApplyModifiedProperties();
        }

        static void AddLayer(string layerName)
        {
            var tm = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tm.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                var l = layers.GetArrayElementAtIndex(i);
                if (l.stringValue == layerName) return;
                if (string.IsNullOrEmpty(l.stringValue))
                {
                    l.stringValue = layerName;
                    tm.ApplyModifiedProperties();
                    return;
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  MATERIALS
        // ═══════════════════════════════════════════════════════

        static Dictionary<string, Material> CreateMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var colorMap = new Dictionary<string, Color>
            {
                { "Knight",      new Color(0.3f, 0.5f, 0.9f) },
                { "Archer",      new Color(0.3f, 0.9f, 0.4f) },
                { "Catapult",    new Color(0.8f, 0.6f, 0.2f) },
                { "Scout",       new Color(0.9f, 0.9f, 0.3f) },
                { "Mage",        new Color(0.7f, 0.3f, 0.9f) },
                { "Castle",      new Color(0.7f, 0.7f, 0.7f) },
                { "ArcherTower", new Color(0.5f, 0.4f, 0.3f) },
                { "CannonTower", new Color(0.4f, 0.4f, 0.4f) },
                { "Wall",        new Color(0.6f, 0.55f, 0.5f) },
                { "Barracks",    new Color(0.6f, 0.4f, 0.2f) },
                { "GoldMine",    new Color(0.9f, 0.8f, 0.1f) },
            };
            var dict = new Dictionary<string, Material>();
            foreach (var kvp in colorMap)
            {
                string path = $"{MatRoot}/{kvp.Key}_Mat.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(shader) { color = kvp.Value };
                    AssetDatabase.CreateAsset(mat, path);
                }
                dict[kvp.Key] = mat;
            }
            return dict;
        }

        // ═══════════════════════════════════════════════════════
        //  PREFABS
        // ═══════════════════════════════════════════════════════

        static Dictionary<string, GameObject> CreatePrefabs(Dictionary<string, Material> mats)
        {
            var dict = new Dictionary<string, GameObject>();

            // Units (Capsule)
            foreach (var name in new[] { "Knight", "Archer", "Catapult", "Scout", "Mage" })
                dict[name] = CreateUnitPrefab(name, mats[name]);

            // Buildings (Cube)
            var bldgConfigs = new (string name, Vector3 scale)[]
            {
                ("Castle",      new Vector3(2, 1,    2)),
                ("ArcherTower", new Vector3(1, 1.5f, 1)),
                ("CannonTower", new Vector3(2, 1.2f, 2)),
                ("Wall",        new Vector3(1, 1,    1)),
                ("Barracks",    new Vector3(2, 1,    2)),
                ("GoldMine",    new Vector3(1, 0.8f, 1)),
            };
            foreach (var (name, scale) in bldgConfigs)
            {
                bool isTower = name is "ArcherTower" or "CannonTower";
                dict[name] = CreateBuildingPrefab(name, mats[name], scale, isTower);
            }
            return dict;
        }

        static GameObject CreateUnitPrefab(string unitName, Material mat)
        {
            string path = $"{PrefabRoot}/Units/{unitName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = unitName;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.AddComponent<Unit>();
            go.AddComponent<UnitAI>();        // RequireComponent adds NavMeshAgent automatically
            go.AddComponent<FogRevealAgent>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateBuildingPrefab(string buildingName, Material mat, Vector3 scale, bool isTower)
        {
            string path = $"{PrefabRoot}/Buildings/{buildingName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = buildingName;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.AddComponent<Building>();
            if (isTower) go.AddComponent<DefenseTower>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ═══════════════════════════════════════════════════════
        //  SCRIPTABLE OBJECTS
        // ═══════════════════════════════════════════════════════

        static void CreateScriptableObjects(Dictionary<string, GameObject> prefabs)
        {
            CreateUnitDatas(prefabs);
            CreateBuildingDatas(prefabs);
            CreateStageData();
        }

        static void CreateUnitDatas(Dictionary<string, GameObject> prefabs)
        {
            var configs = new (string name, UnitType type, int hp, int dmg, float spd, int cost, float range, float cd, float sight)[]
            {
                ("Knight",   UnitType.Knight,   200, 20, 3.0f,  50, 1.5f, 1.0f,  5f),
                ("Archer",   UnitType.Archer,    80, 15, 3.5f,  40, 8.0f, 1.2f,  8f),
                ("Catapult", UnitType.Catapult, 120, 50, 1.5f,  80, 6.0f, 3.0f,  5f),
                ("Scout",    UnitType.Scout,     60,  5, 6.0f,  30, 2.0f, 1.0f, 15f),
                ("Mage",     UnitType.Mage,     100, 40, 2.5f, 100, 5.0f, 2.0f,  6f),
            };
            foreach (var (name, type, hp, dmg, spd, cost, range, cd, sight) in configs)
            {
                string path = $"{SORoot}/Units/{name}.asset";
                var d = AssetDatabase.LoadAssetAtPath<UnitData>(path);
                if (d == null) { d = ScriptableObject.CreateInstance<UnitData>(); AssetDatabase.CreateAsset(d, path); }
                d.unitName = name; d.unitType = type; d.maxHp = hp; d.damage = dmg;
                d.moveSpeed = spd; d.goldCost = cost; d.attackRange = range;
                d.attackCooldown = cd; d.sightRange = sight;
                d.prefab = prefabs.TryGetValue(name, out var p) ? p : null;
                EditorUtility.SetDirty(d);
            }
        }

        static void CreateBuildingDatas(Dictionary<string, GameObject> prefabs)
        {
            var configs = new (string name, BuildingType type, int hp, int gw, int gh, bool tower, float range, int atkDmg, float atkCd, int goldPS)[]
            {
                ("Castle",      BuildingType.Castle,      2000, 2, 2, false,  0,  0,   0, 0),
                ("ArcherTower", BuildingType.ArcherTower,  500, 1, 1, true,   8, 12, 1.0f, 0),
                ("CannonTower", BuildingType.CannonTower,  800, 2, 2, true,   6, 40, 3.0f, 0),
                ("Wall",        BuildingType.Wall,         300, 1, 1, false,  0,  0,   0, 0),
                ("Barracks",    BuildingType.Barracks,     400, 2, 2, false,  0,  0,   0, 0),
                ("GoldMine",    BuildingType.GoldMine,     300, 1, 1, false,  0,  0,   0, 2),
            };
            foreach (var (name, type, hp, gw, gh, tower, range, atkDmg, atkCd, goldPS) in configs)
            {
                string path = $"{SORoot}/Buildings/{name}.asset";
                var d = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                if (d == null) { d = ScriptableObject.CreateInstance<BuildingData>(); AssetDatabase.CreateAsset(d, path); }
                d.buildingName = name; d.buildingType = type; d.maxHp = hp;
                d.gridSize = new Vector2Int(gw, gh); d.isDefenseTower = tower;
                d.attackRange = range; d.attackDamage = atkDmg; d.attackCooldown = atkCd;
                d.goldProductionPerSecond = goldPS;
                d.prefab = prefabs.TryGetValue(name, out var p) ? p : null;
                EditorUtility.SetDirty(d);
            }
        }

        static void CreateStageData()
        {
            string path = $"{SORoot}/Stages/Stage01_Tutorial.asset";
            var d = AssetDatabase.LoadAssetAtPath<StageData>(path);
            if (d == null) { d = ScriptableObject.CreateInstance<StageData>(); AssetDatabase.CreateAsset(d, path); }

            d.stageName = "Tutorial Village";
            d.stageIndex = 0;
            d.battleDuration = 180f;
            d.unlockRequirementStars = 0;

            var castle      = AssetDatabase.LoadAssetAtPath<BuildingData>($"{SORoot}/Buildings/Castle.asset");
            var archerTower = AssetDatabase.LoadAssetAtPath<BuildingData>($"{SORoot}/Buildings/ArcherTower.asset");
            d.enemyBase = new BuildingPlacement[]
            {
                new BuildingPlacement { data = castle,      gridPosition = new Vector2Int(5, 5) },
                new BuildingPlacement { data = archerTower, gridPosition = new Vector2Int(3, 3) },
                new BuildingPlacement { data = archerTower, gridPosition = new Vector2Int(7, 3) },
            };

            var knight = AssetDatabase.LoadAssetAtPath<UnitData>($"{SORoot}/Units/Knight.asset");
            d.waves = new WaveData[]
            {
                new WaveData { units = new UnitData[] { knight, knight }, spawnInterval = 2f, waveStartTime = 10f }
            };
            EditorUtility.SetDirty(d);
        }

        // ═══════════════════════════════════════════════════════
        //  UI BUTTON PREFABS
        // ═══════════════════════════════════════════════════════

        static void CreateUIButtonPrefabs()
        {
            CreateButtonPrefab("UnitButtonPrefab",     new Vector2(80, 80),  new Color(0.2f, 0.2f, 0.2f, 0.9f));
            CreateButtonPrefab("BuildingButtonPrefab", new Vector2(100, 60), new Color(0.3f, 0.2f, 0.1f, 0.9f));
        }

        static void CreateButtonPrefab(string prefabName, Vector2 size, Color bgColor)
        {
            string path = $"{PrefabRoot}/UI/{prefabName}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var go  = new GameObject(prefabName);
            var img = go.AddComponent<Image>(); img.color = bgColor;
            go.AddComponent<Button>();
            var rt  = go.GetComponent<RectTransform>(); rt.sizeDelta = size;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<Text>();
            text.font      = DefaultFont();
            text.fontSize  = 12;
            text.color     = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var trt = text.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.sizeDelta = Vector2.zero;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        // ═══════════════════════════════════════════════════════
        //  SCENE HELPERS
        // ═══════════════════════════════════════════════════════

        static void OpenAndSetup(string scenePath, System.Action action)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            action();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static GameObject FindOrCreate(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) go = new GameObject(name);
            return go;
        }

        static T Ensure<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        static void SetRef(Component c, string field, Object val)
        {
            var so = new SerializedObject(c);
            var p  = so.FindProperty(field);
            if (p != null) { p.objectReferenceValue = val; so.ApplyModifiedProperties(); }
        }

        static void SetInt(Component c, string field, int val)
        {
            var so = new SerializedObject(c);
            var p  = so.FindProperty(field);
            if (p != null) { p.intValue = val; so.ApplyModifiedProperties(); }
        }

        static void SetObjArray<T>(Component c, string field, T[] vals) where T : Object
        {
            var so = new SerializedObject(c);
            var p  = so.FindProperty(field);
            if (p == null) return;
            p.arraySize = vals.Length;
            for (int i = 0; i < vals.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = vals[i];
            so.ApplyModifiedProperties();
        }

        static T[] LoadAll<T>(string folder) where T : Object
        {
            var guids  = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            var result = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
                result[i] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
            return result;
        }

        static Font DefaultFont()
            => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // ─── Canvas / UI creation ─────────────────────────────

        static GameObject GetOrCreateCanvas()
        {
            var existing = Object.FindObjectOfType<Canvas>();
            if (existing != null) return existing.gameObject;

            var go     = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
            return go;
        }

        static Text MakeText(Transform parent, string goName, string label,
            Vector2 ancMin, Vector2 ancMax, Vector2 pos, Vector2 size, int fontSize = 24)
        {
            var t   = parent.Find(goName);
            var go  = t != null ? t.gameObject : new GameObject(goName);
            go.transform.SetParent(parent, false);
            var txt = Ensure<Text>(go);
            txt.text = label; txt.font = DefaultFont(); txt.fontSize = fontSize;
            txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = ancMin; rt.anchorMax = ancMax;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            return txt;
        }

        static Button MakeButton(Transform parent, string goName, string label,
            Vector2 ancMin, Vector2 ancMax, Vector2 pos, Vector2 size)
        {
            var t   = parent.Find(goName);
            var go  = t != null ? t.gameObject : new GameObject(goName);
            go.transform.SetParent(parent, false);
            var img = Ensure<Image>(go);   img.color = new Color(0.15f, 0.15f, 0.6f, 0.95f);
            var btn = Ensure<Button>(go);
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = ancMin; rt.anchorMax = ancMax;
            rt.anchoredPosition = pos; rt.sizeDelta = size;

            var lt  = go.transform.Find("Text");
            var lgo = lt != null ? lt.gameObject : new GameObject("Text");
            lgo.transform.SetParent(go.transform, false);
            var ltxt = Ensure<Text>(lgo);
            ltxt.text = label; ltxt.font = DefaultFont();
            ltxt.fontSize = 18; ltxt.color = Color.white;
            ltxt.alignment = TextAnchor.MiddleCenter;
            var lrt = lgo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;

            return btn;
        }

        static void AddCamera(Vector3 pos, Vector3 euler)
        {
            if (Camera.main != null) return;
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.AddComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
            go.AddComponent<AudioListener>();
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(euler));
        }

        static void AddLight()
        {
            if (Object.FindObjectOfType<Light>() != null) return;
            var go = new GameObject("Directional Light");
            var l  = go.AddComponent<Light>();
            l.type = LightType.Directional; l.intensity = 1f;
            go.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        static GameObject AddGround()
        {
            var existing = GameObject.Find("Ground");
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Ground";
            go.transform.localScale  = new Vector3(4, 1, 4);
            go.transform.position    = new Vector3(10, 0, 10);
            int gl = LayerMask.NameToLayer("Ground");
            if (gl >= 0) go.layer = gl;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
            return go;
        }

        // ═══════════════════════════════════════════════════════
        //  SCENE: MainMenu
        // ═══════════════════════════════════════════════════════

        static void SetupMainMenuScene(StageData[] stageDatas)
        {
            AddCamera(new Vector3(0, 5, -10), new Vector3(15, 0, 0));
            AddLight();

            var gmGO = FindOrCreate("GameManager");
            Ensure<GameManager>(gmGO);

            var cmGO = FindOrCreate("CampaignManager");
            var cm   = Ensure<CampaignManager>(cmGO);
            SetObjArray(cm, "stages", stageDatas);

            var canvas = GetOrCreateCanvas();
            MakeText(canvas.transform, "Title", "Medieval RTS",
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(500, 80), 52)
                .color = Color.yellow;

            MakeButton(canvas.transform, "PlayButton", "▶ Play",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 60));

            MakeButton(canvas.transform, "EditBaseButton", "🏰 Edit Base",
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(220, 60));
        }

        // ═══════════════════════════════════════════════════════
        //  SCENE: BaseBuilder
        // ═══════════════════════════════════════════════════════

        static void SetupBaseBuilderScene(BuildingData[] buildingDatas, GameObject buildBtn)
        {
            AddCamera(new Vector3(10, 20, -2), new Vector3(70, 0, 0));
            AddLight();
            AddGround();

            var mgrGO = FindOrCreate("BaseBuilderManager");
            var mgr   = Ensure<BaseBuilderManager>(mgrGO);
            Ensure<GridVisualizer>(mgrGO);
            SetObjArray(mgr, "buildingCatalog", buildingDatas);

            var placerGO = FindOrCreate("BuildingPlacer");
            var placer   = Ensure<BuildingPlacer>(placerGO);
            SetRef(placer, "_manager", mgr);
            SetRef(placer, "_cam", Camera.main);
            int gl = LayerMask.NameToLayer("Ground");
            if (gl >= 0) SetInt(placer, "_groundLayer", 1 << gl);

            var canvas = GetOrCreateCanvas();
            var uiGO   = canvas.transform.Find("BaseBuilderUI")?.gameObject ?? new GameObject("BaseBuilderUI");
            uiGO.transform.SetParent(canvas.transform, false);
            var ui = Ensure<BaseBuilderUI>(uiGO);

            // Left panel for building buttons
            var panelT = canvas.transform.Find("BuildingPanel");
            var panel  = panelT != null ? panelT.gameObject : new GameObject("BuildingPanel");
            panel.transform.SetParent(canvas.transform, false);
            var pImg = Ensure<Image>(panel); pImg.color = new Color(0, 0, 0, 0.6f);
            var vl   = Ensure<VerticalLayoutGroup>(panel);
            vl.spacing = 5; vl.padding = new RectOffset(5, 5, 5, 5);
            vl.childControlWidth = false; vl.childControlHeight = false;
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0, 0.5f); prt.anchorMax = new Vector2(0, 0.5f);
            prt.anchoredPosition = new Vector2(60, 0); prt.sizeDelta = new Vector2(110, 420);

            var saveBtn = MakeButton(canvas.transform, "SaveButton", "Save & Exit",
                new Vector2(1, 0), new Vector2(1, 0), new Vector2(-80, 40), new Vector2(150, 50));

            SetRef(ui, "placer",              placer);
            SetRef(ui, "manager",             mgr);
            SetRef(ui, "saveButton",          saveBtn);
            SetRef(ui, "buttonContainer",     panel.transform);
            SetRef(ui, "buildingButtonPrefab", buildBtn);
            SetObjArray(ui, "catalog", buildingDatas);
        }

        // ═══════════════════════════════════════════════════════
        //  SCENE: Battle
        // ═══════════════════════════════════════════════════════

        static void SetupBattleScene(UnitData[] unitDatas, GameObject uiBtn)
        {
            AddCamera(new Vector3(10, 18, -4), new Vector3(65, 0, 0));
            AddLight();
            AddGround();

            // Managers (needed when testing Battle scene directly)
            Ensure<GameManager>(FindOrCreate("GameManager"));
            var cm = Ensure<CampaignManager>(FindOrCreate("CampaignManager"));
            var stageDatas = LoadAll<StageData>(SORoot + "/Stages");
            SetObjArray(cm, "stages", stageDatas);

            Ensure<ResourceManager>(FindOrCreate("ResourceManager"));
            Ensure<BattleManager>(FindOrCreate("BattleManager"));

            // Spawn zones
            var pzGO = FindOrCreate("PlayerSpawnZone");
            var pz   = Ensure<SpawnZone>(pzGO);
            var pbc  = Ensure<BoxCollider>(pzGO);
            pbc.isTrigger = true; pbc.size = new Vector3(4, 0.1f, 4);
            pzGO.transform.position = new Vector3(2, 0, 10);
            SetRef(pz, "_area", pbc);

            var ezGO = FindOrCreate("EnemySpawnZone");
            var ez   = Ensure<SpawnZone>(ezGO);
            var ebc  = Ensure<BoxCollider>(ezGO);
            ebc.isTrigger = true; ebc.size = new Vector3(4, 0.1f, 4);
            ezGO.transform.position = new Vector3(18, 0, 10);
            SetRef(ez, "_area", ebc);

            var spawnerGO = FindOrCreate("UnitSpawner");
            var spawner   = Ensure<UnitSpawner>(spawnerGO);
            SetRef(spawner, "_playerSpawnZone", pz);

            var aiGO = FindOrCreate("AIWaveSpawner");
            var ai   = Ensure<AIWaveSpawner>(aiGO);
            SetRef(ai, "_enemySpawnZone", ez);

            // HUD
            var canvas  = GetOrCreateCanvas();
            var hudGO   = canvas.transform.Find("BattleHUD")?.gameObject ?? new GameObject("BattleHUD");
            hudGO.transform.SetParent(canvas.transform, false);
            var hud = Ensure<BattleHUD>(hudGO);

            var goldTxt  = MakeText(canvas.transform, "GoldText",  "100",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(100, -30), new Vector2(200, 45), 28);
            goldTxt.color = Color.yellow;

            var timerTxt = MakeText(canvas.transform, "TimerText", "180s",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(180, 45), 28);

            var cntT = canvas.transform.Find("UnitButtonContainer");
            var cnt  = cntT != null ? cntT.gameObject : new GameObject("UnitButtonContainer");
            cnt.transform.SetParent(canvas.transform, false);
            Ensure<Image>(cnt).color = new Color(0, 0, 0, 0.5f);
            var hl = Ensure<HorizontalLayoutGroup>(cnt);
            hl.spacing = 5; hl.padding = new RectOffset(5, 5, 5, 5);
            hl.childControlWidth = false; hl.childControlHeight = false;
            var crt = cnt.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 0); crt.anchorMax = new Vector2(0, 0);
            crt.anchoredPosition = new Vector2(250, 55); crt.sizeDelta = new Vector2(500, 90);

            SetRef(hud, "goldText",            goldTxt);
            SetRef(hud, "timerText",           timerTxt);
            SetRef(hud, "unitButtonContainer", cnt.transform);
            SetRef(hud, "unitButtonPrefab",    uiBtn);
            SetObjArray(hud, "availableUnits", unitDatas);
        }

        // ═══════════════════════════════════════════════════════
        //  SCENE: Result
        // ═══════════════════════════════════════════════════════

        static void SetupResultScene()
        {
            AddCamera(new Vector3(0, 5, -10), new Vector3(15, 0, 0));
            AddLight();

            var canvas   = GetOrCreateCanvas();
            var resultGO = canvas.transform.Find("ResultUI")?.gameObject ?? new GameObject("ResultUI");
            resultGO.transform.SetParent(canvas.transform, false);
            var resultUI = Ensure<ResultUI>(resultGO);

            var titleTxt = MakeText(canvas.transform, "TitleText", "Victory!",
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(500, 90), 52);
            titleTxt.color = Color.yellow;

            // 3 Star objects
            var stars = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                string sn  = $"Star{i + 1}";
                var st = canvas.transform.Find(sn);
                var sGO = st != null ? st.gameObject : new GameObject(sn);
                sGO.transform.SetParent(canvas.transform, false);
                Ensure<Image>(sGO).color = Color.yellow;
                var srt = sGO.GetComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.5f, 0.57f);
                srt.anchorMax = new Vector2(0.5f, 0.57f);
                srt.anchoredPosition = new Vector2((i - 1) * 75f, 0);
                srt.sizeDelta = new Vector2(65, 65);
                stars[i] = sGO;
            }

            var continueBtn = MakeButton(canvas.transform, "ContinueButton", "Continue",
                new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(-95, 0), new Vector2(170, 55));
            var retryBtn = MakeButton(canvas.transform, "RetryButton", "Retry",
                new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(95, 0),  new Vector2(170, 55));

            SetRef(resultUI, "titleText",     titleTxt);
            SetRef(resultUI, "continueButton", continueBtn);
            SetRef(resultUI, "retryButton",    retryBtn);
            SetObjArray(resultUI, "starObjects", stars);
        }
    }
}
