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

    [Test]
    public void Delete_RemovesFile()
    {
        SaveSystem.Save(new SaveData(), _testPath);
        SaveSystem.Delete(_testPath);
        Assert.IsFalse(File.Exists(_testPath));
    }
}
