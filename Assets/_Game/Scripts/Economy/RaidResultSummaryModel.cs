using System;
using System.Linq;

namespace MedievalRTS.Economy
{
    public sealed class RaidResultSummary
    {
        public string OutcomeLabel { get; }
        public string ProtectionLabel { get; }
        public string StoredLossText { get; }
        public string OwnedLossText { get; }
        public string TotalLossText { get; }
        public string NextActionHint { get; }
        public bool IsDefenseSuccess { get; }

        public RaidResultSummary(
            string outcomeLabel,
            string protectionLabel,
            string storedLossText,
            string ownedLossText,
            string totalLossText,
            string nextActionHint,
            bool isDefenseSuccess)
        {
            OutcomeLabel = outcomeLabel;
            ProtectionLabel = protectionLabel;
            StoredLossText = storedLossText;
            OwnedLossText = ownedLossText;
            TotalLossText = totalLossText;
            NextActionHint = nextActionHint;
            IsDefenseSuccess = isDefenseSuccess;
        }
    }

    public static class RaidResultSummaryModel
    {
        public static RaidResultSummary Create(RaidForecast forecast)
        {
            if (forecast == null) throw new ArgumentNullException(nameof(forecast));

            int storedTotal = Total(forecast.StoredLoss);
            int ownedTotal = Total(forecast.OwnedLoss);
            int total = storedTotal + ownedTotal;

            return new RaidResultSummary(
                FormatOutcome(forecast.Outcome),
                $"Headquarters protection: {Math.Round(forecast.ProtectionRate * 100f)}%",
                $"Stored resources lost: {FormatWallet(forecast.StoredLoss)}",
                $"Wallet resources lost: {FormatWallet(forecast.OwnedLoss)}",
                $"Total raid loss: {total}",
                FormatHint(forecast.Outcome, storedTotal, ownedTotal),
                forecast.Outcome == RaidOutcome.DefenseSuccess);
        }

        private static string FormatOutcome(RaidOutcome outcome)
        {
            return outcome switch
            {
                RaidOutcome.DefenseSuccess => "Defense success",
                RaidOutcome.NarrowFailure => "Narrow breach",
                RaidOutcome.ClearFailure => "Raid broke through",
                RaidOutcome.HeadquartersDestroyed => "Headquarters destroyed",
                _ => "Raid result"
            };
        }

        private static string FormatHint(RaidOutcome outcome, int storedLoss, int ownedLoss)
        {
            return outcome switch
            {
                RaidOutcome.DefenseSuccess => "Defense held. No resources lost.",
                RaidOutcome.NarrowFailure => storedLoss > ownedLoss
                    ? "Storage took the bigger hit. Upgrade Headquarters protection before the next raid."
                    : "Wallet loss stayed low, but the breach should be answered with more front-line defense.",
                RaidOutcome.ClearFailure => "Raiders reached the economy. Add defensive buildings or reduce exposed resources.",
                RaidOutcome.HeadquartersDestroyed => "Core defense collapsed. Rebuild Headquarters protection before chasing more loot.",
                _ => "Review the raid result before starting the next battle."
            };
        }

        private static string FormatWallet(ResourceWallet wallet)
        {
            if (wallet == null) return "0";

            var parts = Enum.GetValues(typeof(ResourceType))
                .Cast<ResourceType>()
                .Where(type => wallet.Get(type) > 0)
                .Select(type => $"{type} {wallet.Get(type)}")
                .ToArray();

            return parts.Length == 0 ? "0" : string.Join(", ", parts);
        }

        private static int Total(ResourceWallet wallet)
        {
            if (wallet == null) return 0;

            return Enum.GetValues(typeof(ResourceType))
                .Cast<ResourceType>()
                .Sum(type => wallet.Get(type));
        }
    }
}