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
