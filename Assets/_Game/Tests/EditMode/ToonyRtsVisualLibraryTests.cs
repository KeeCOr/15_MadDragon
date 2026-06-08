using MedievalRTS.Visuals;
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

    [Test]
    public void ToonyRts_LibraryLoadsMappedAssets()
    {
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadUnit("Knight"));
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadUnit("Archer"));
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadUnit("Mage"));
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadUnit("Catapult"));
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadBuilding("player_castle"));
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadBuilding("tower"));
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadDecoration("blue_banner"));
        Assert.IsNotNull(ToonyRtsVisualLibrary.LoadEffect("building_destroyed"));
    }

    [Test]
    public void ToonyRts_ApplierStripsPhysicsFromVisualChild()
    {
        var root = new GameObject("Root");
        var visualPrefab = new GameObject("VisualPrefab");
        visualPrefab.AddComponent<BoxCollider>();
        visualPrefab.AddComponent<Rigidbody>();

        var visual = ToonyRtsVisualApplier.Attach(root, visualPrefab, Vector3.zero, Vector3.one, Quaternion.identity);

        Assert.IsNotNull(visual);
        Assert.IsNull(visual.GetComponent<BoxCollider>());
        Assert.IsNull(visual.GetComponent<Rigidbody>());

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(visualPrefab);
    }

    [Test]
    public void ToonyRts_ApplierFitsVisualFootprintToTargetWorldSize()
    {
        var root = new GameObject("Root");
        var visualPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualPrefab.name = "VisualPrefab";

        var visual = ToonyRtsVisualApplier.Attach(root, visualPrefab, Vector3.zero, Vector3.one, Quaternion.identity);
        ToonyRtsVisualApplier.FitFootprintToWorldSize(visual, new Vector2(4f, 2f));

        var bounds = visual.GetComponentInChildren<Renderer>().bounds;
        Assert.That(bounds.size.x, Is.EqualTo(2f).Within(0.05f));
        Assert.That(bounds.size.z, Is.EqualTo(2f).Within(0.05f));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(visualPrefab);
    }

    [Test]
    public void ToonyRts_ApplierAlignsVisualBottomToWorldY()
    {
        var root = new GameObject("Root");
        var visualPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualPrefab.name = "VisualPrefab";

        var visual = ToonyRtsVisualApplier.Attach(root, visualPrefab, Vector3.up * 2f, Vector3.one, Quaternion.identity);
        ToonyRtsVisualApplier.AlignBottomToWorldY(visual, 0f);

        var bounds = visual.GetComponentInChildren<Renderer>().bounds;
        Assert.That(bounds.min.y, Is.EqualTo(0f).Within(0.05f));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(visualPrefab);
    }
}
