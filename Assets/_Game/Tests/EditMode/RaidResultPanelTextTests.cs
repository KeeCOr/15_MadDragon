using NUnit.Framework;
using MedievalRTS.Economy;

public class RaidResultPanelTextTests
{
    [Test]
    public void CreatesReadableVictoryTitleWithTrimmedReason()
    {
        var summary = RaidResultSummaryModel.Create(
            RaidLossCalculator.Calculate(new ResourceWallet(), new ResourceWallet(), RaidOutcome.DefenseSuccess, 0.35f));

        var panel = RaidResultPanelText.Create(true, "  Defense held  ", 0, 80, 1, summary);

        Assert.AreEqual("Victory\nDefense held", panel.Title);
        StringAssert.Contains("Earned gold: +80G", panel.Body);
        StringAssert.Contains("Defense held. No resources lost.", panel.Body);
    }

    [Test]
    public void ShowsRaidLossBreakdownAndHintInBody()
    {
        var stored = new ResourceWallet();
        var owned = new ResourceWallet();
        stored.Add(ResourceType.Gold, 1200);
        owned.Add(ResourceType.Gold, 8000);
        var summary = RaidResultSummaryModel.Create(
            RaidLossCalculator.Calculate(stored, owned, RaidOutcome.NarrowFailure, 0.35f));

        var panel = RaidResultPanelText.Create(false, "Castle breached", 2, 150, 3, summary);

        StringAssert.Contains("Defeat", panel.Title);
        StringAssert.Contains("Destroyed buildings: 2", panel.Body);
        StringAssert.Contains("Stored resources lost: Gold 234", panel.Body);
        StringAssert.Contains("Wallet resources lost: Gold 260", panel.Body);
        StringAssert.Contains("front-line defense", panel.Body);
    }

    [Test]
    public void ClampsNegativeRewardsAndFallsBackWhenSummaryIsMissing()
    {
        var panel = RaidResultPanelText.Create(false, "", -4, -30, -2, null);

        Assert.AreEqual("Defeat\nBattle ended", panel.Title);
        StringAssert.Contains("Destroyed buildings: 0", panel.Body);
        StringAssert.Contains("Earned gold: +0G", panel.Body);
        StringAssert.Contains("Earned valor: +0", panel.Body);
        StringAssert.Contains("no defense forecast available", panel.Body);
    }
}