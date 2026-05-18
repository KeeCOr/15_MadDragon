using System;

namespace MedievalRTS.Economy
{
    public static class RaidLossCalculator
    {
        public static RaidForecast Calculate(ResourceWallet stored, ResourceWallet owned, RaidOutcome outcome, float protectionRate)
        {
            if (stored == null) throw new ArgumentNullException(nameof(stored));
            if (owned == null) throw new ArgumentNullException(nameof(owned));

            var storedLoss = new ResourceWallet();
            var ownedLoss = new ResourceWallet();
            var rates = GetRates(outcome);
            float clampedProtectionRate = Math.Clamp(protectionRate, 0f, 0.75f);
            float lossMultiplier = 1f - clampedProtectionRate;

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                storedLoss.Set(type, CalculateLoss(stored.Get(type), rates.storedRate, lossMultiplier));
                ownedLoss.Set(type, CalculateLoss(owned.Get(type), rates.ownedRate, lossMultiplier));
            }

            return new RaidForecast(outcome, clampedProtectionRate, storedLoss, ownedLoss);
        }

        public static void ApplyLoss(ResourceWallet stored, ResourceWallet owned, RaidForecast forecast)
        {
            if (stored == null) throw new ArgumentNullException(nameof(stored));
            if (owned == null) throw new ArgumentNullException(nameof(owned));
            if (forecast == null) throw new ArgumentNullException(nameof(forecast));

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                stored.Remove(type, forecast.StoredLoss.Get(type));
                owned.Remove(type, forecast.OwnedLoss.Get(type));
            }
        }

        private static (float storedRate, float ownedRate) GetRates(RaidOutcome outcome)
        {
            return outcome switch
            {
                RaidOutcome.DefenseSuccess => (0f, 0f),
                RaidOutcome.NarrowFailure => (0.30f, 0.05f),
                RaidOutcome.ClearFailure => (0.70f, 0.15f),
                RaidOutcome.HeadquartersDestroyed => (1.00f, 0.20f),
                _ => (0f, 0f)
            };
        }

        private static int CalculateLoss(int amount, float rate, float lossMultiplier)
        {
            return Math.Max(0, (int)Math.Round(amount * rate * lossMultiplier));
        }
    }
}
