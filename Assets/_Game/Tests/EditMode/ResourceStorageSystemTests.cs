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
    public void TickProduction_ClampsToAggregateCapacityForSameResourceProducers()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 1);
        storage.AddProducer(new ResourceProductionBuilding("GoldMineA", ResourceType.Gold, 10, 100));
        storage.AddProducer(new ResourceProductionBuilding("GoldMineB", ResourceType.Gold, 10, 100));

        storage.TickProduction(20f);

        Assert.AreEqual(200, storage.Stored.Get(ResourceType.Gold));
    }

    [Test]
    public void TickProduction_ClampsToHeadquartersCapacity()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 1);
        storage.AddProducer(new ResourceProductionBuilding("GoldMine", ResourceType.Gold, 1000, 5000));

        storage.TickProduction(2f);

        Assert.AreEqual(1000, storage.Stored.Get(ResourceType.Gold));
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

    [Test]
    public void ProtectionRate_CapsAtSeventyFivePercent()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 8);

        Assert.AreEqual(0.75f, storage.ProtectionRate, 0.0001f);
    }

    [Test]
    public void SetHeadquartersLevel_ClampsToMinimumLevelOne()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 3);

        storage.SetHeadquartersLevel(0);

        Assert.AreEqual(1, storage.HeadquartersLevel);
        Assert.AreEqual(1000, storage.GetHeadquartersCapacity(ResourceType.Gold));
    }

    [Test]
    public void AddProducer_NullProducer_ThrowsArgumentNullException()
    {
        var storage = new ResourceStorageSystem(headquartersLevel: 1);

        Assert.Throws<System.ArgumentNullException>(() => storage.AddProducer(null));
    }
}
