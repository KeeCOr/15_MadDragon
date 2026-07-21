using MedievalRTS.Economy;

static void Equal<T>(T actual, T expected, string name)
{
    if (!EqualityComparer<T>.Default.Equals(actual, expected))
        throw new Exception($"{name}: expected {expected}, got {actual}");
}

static void Contains(string actual, string expected, string name)
{
    if (!actual.Contains(expected))
        throw new Exception($"{name}: expected to contain {expected}, got {actual}");
}

var forecastSuccess = RaidLossCalculator.Calculate(new ResourceWallet(), new ResourceWallet(), RaidOutcome.DefenseSuccess, 0.35f);
var success = RaidResultSummaryModel.Create(forecastSuccess);
Equal(success.IsDefenseSuccess, true, "success flag");
Equal(success.OutcomeLabel, "Defense success", "success label");
Equal(success.ProtectionLabel, "Headquarters protection: 35%", "success protection");
Equal(success.StoredLossText, "Stored resources lost: 0", "success stored loss");
Equal(success.OwnedLossText, "Wallet resources lost: 0", "success wallet loss");
Equal(success.TotalLossText, "Total raid loss: 0", "success total loss");
Contains(success.NextActionHint, "No resources lost", "success hint");

var storedNarrow = new ResourceWallet();
var ownedNarrow = new ResourceWallet();
storedNarrow.Add(ResourceType.Gold, 1200);
ownedNarrow.Add(ResourceType.Gold, 8000);
var forecastNarrow = RaidLossCalculator.Calculate(storedNarrow, ownedNarrow, RaidOutcome.NarrowFailure, 0.35f);
var narrow = RaidResultSummaryModel.Create(forecastNarrow);
Equal(narrow.IsDefenseSuccess, false, "narrow flag");
Equal(narrow.OutcomeLabel, "Narrow breach", "narrow label");
Equal(narrow.StoredLossText, "Stored resources lost: Gold 234", "narrow stored loss");
Equal(narrow.OwnedLossText, "Wallet resources lost: Gold 260", "narrow wallet loss");
Equal(narrow.TotalLossText, "Total raid loss: 494", "narrow total loss");
Contains(narrow.NextActionHint, "front-line defense", "narrow hint");

var storedDestroyed = new ResourceWallet();
var ownedDestroyed = new ResourceWallet();
storedDestroyed.Add(ResourceType.Gold, 1200);
storedDestroyed.Add(ResourceType.Honor, 100);
ownedDestroyed.Add(ResourceType.Gold, 8000);
var forecastDestroyed = RaidLossCalculator.Calculate(storedDestroyed, ownedDestroyed, RaidOutcome.HeadquartersDestroyed, 0.0f);
var destroyed = RaidResultSummaryModel.Create(forecastDestroyed);
Equal(destroyed.OutcomeLabel, "Headquarters destroyed", "destroyed label");
Equal(destroyed.StoredLossText, "Stored resources lost: Gold 1200, Honor 100", "destroyed stored loss");
Equal(destroyed.OwnedLossText, "Wallet resources lost: Gold 1600", "destroyed wallet loss");
Equal(destroyed.TotalLossText, "Total raid loss: 2900", "destroyed total loss");
Contains(destroyed.NextActionHint, "Rebuild Headquarters protection", "destroyed hint");

var resultPanel = RaidResultPanelText.Create(
    victory: false,
    reason: "Castle breached",
    destroyedBuildings: 2,
    earnedGold: 150,
    earnedValor: 3,
    raidSummary: destroyed);

Contains(resultPanel.Title, "Defeat", "result panel title");
Contains(resultPanel.Title, "Castle breached", "result panel reason");
Contains(resultPanel.Body, "Destroyed buildings: 2", "result panel destroyed count");
Contains(resultPanel.Body, "Earned gold: +150G", "result panel gold");
Contains(resultPanel.Body, "Stored resources lost: Gold 1200, Honor 100", "result panel stored loss");
Contains(resultPanel.Body, "Rebuild Headquarters protection", "result panel hint");

Console.WriteLine("RaidResultSummaryModel independent C# check passed: 4 scenarios");