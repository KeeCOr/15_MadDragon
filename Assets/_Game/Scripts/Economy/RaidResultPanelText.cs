using System;

namespace MedievalRTS.Economy
{
    public sealed class RaidResultPanelText
    {
        public string Title { get; }
        public string Body { get; }

        private RaidResultPanelText(string title, string body)
        {
            Title = title;
            Body = body;
        }

        public static RaidResultPanelText Create(
            bool victory,
            string reason,
            int destroyedBuildings,
            int earnedGold,
            int earnedValor,
            RaidResultSummary raidSummary)
        {
            string outcome = victory ? "Victory" : "Defeat";
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "Battle ended" : reason.Trim();
            string title = $"{outcome}\n{safeReason}";
            string body =
                $"Destroyed buildings: {Math.Max(0, destroyedBuildings)}\n" +
                $"Earned gold: +{Math.Max(0, earnedGold)}G\n" +
                $"Earned valor: +{Math.Max(0, earnedValor)}\n" +
                BuildRaidSummaryBlock(raidSummary);

            return new RaidResultPanelText(title, body);
        }

        private static string BuildRaidSummaryBlock(RaidResultSummary raidSummary)
        {
            if (raidSummary == null)
                return "Raid losses: no defense forecast available.";

            return
                $"\n{raidSummary.OutcomeLabel}\n" +
                $"{raidSummary.ProtectionLabel}\n" +
                $"{raidSummary.StoredLossText}\n" +
                $"{raidSummary.OwnedLossText}\n" +
                $"{raidSummary.TotalLossText}\n" +
                raidSummary.NextActionHint;
        }
    }
}