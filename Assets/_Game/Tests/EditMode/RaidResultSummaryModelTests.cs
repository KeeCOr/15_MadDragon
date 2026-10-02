using NUnit.Framework;
using MedievalRTS.Economy;

public class RaidResultSummaryModelTests
{
    [Test]
    public void DefenseSuccessExplainsThatNoResourcesWereLost()
    {
        var forecast = RaidLossCalculator.Calculate(new ResourceWallet(), new ResourceWallet(), RaidOutcome.DefenseSuccess, 0.35f);

        var summary = RaidResultSummaryModel.Create(forecast);

        Assert.IsTrue(summary.IsDefenseSuccess);
        Assert.AreEqual("Defense success", summary.OutcomeLabel);
        Assert.AreEqual("Headquarters protection: 35%", summary.ProtectionLabel);
        Assert.AreEqual("Stored resources lost: 0", summary.StoredLossText);
        Assert.AreEqual("Wallet resources lost: 0", summary.OwnedLossText);
        Assert.AreEqual("Total raid loss: 0", summary.TotalLossText);
        StringAssert.Contains("No resources lost", summary.NextActionHint);
    }

    [Test]
    public void NarrowFailureSeparatesStoredAndWalletLossesForFastReading()
    {
        var stored = new ResourceWallet();
        var owned = new ResourceWallet();
        stored.Add(ResourceType.Gold, 1200);
        owned.Add(ResourceType.Gold, 8000);
        var forecast = RaidLossCalculator.Calculate(stored, owned, RaidOutcome.NarrowFailure, 0.35f);

        var summary = RaidResultSummaryModel.Create(forecast);

        Assert.IsFalse(summary.IsDefenseSuccess);
        Assert.AreEqual("Narrow breach", summary.OutcomeLabel);
        Assert.AreEqual("Stored resources lost: Gold 234", summary.StoredLossText);
        Assert.AreEqual("Wallet resources lost: Gold 260", summary.OwnedLossText);
        Assert.AreEqual("Total raid loss: 494", summary.TotalLossText);
        StringAssert.Contains("front-line defense", summary.NextActionHint);
    }

    [Test]
    public void HeadquartersDestroyedWarnsThePlayerToRebuildCoreDefense()
    {
        var stored = new ResourceWallet();
        var owned = new ResourceWallet();
        stored.Add(ResourceType.Gold, 1200);
        stored.Add(ResourceType.Honor, 100);
        owned.Add(ResourceType.Gold, 8000);
        var forecast = RaidLossCalculator.Calculate(stored, owned, RaidOutcome.HeadquartersDestroyed, 0.0f);

        var summary = RaidResultSummaryModel.Create(forecast);

        Assert.AreEqual("Headquarters destroyed", summary.OutcomeLabel);
        Assert.AreEqual("Stored resources lost: Gold 1200, Honor 100", summary.StoredLossText);
        Assert.AreEqual("Wallet resources lost: Gold 1600", summary.OwnedLossText);
        Assert.AreEqual("Total raid loss: 2900", summary.TotalLossText);
        StringAssert.Contains("Rebuild Headquarters protection", summary.NextActionHint);
    }
}