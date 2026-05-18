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
            double lossMultiplier = 1d - clampedProtectionRate;

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

        private static (double storedRate, double ownedRate) GetRates(RaidOutcome outcome)
        {
            return outcome switch
            {
                RaidOutcome.DefenseSuccess => (0d, 0d),
                RaidOutcome.NarrowFailure => (0.30d, 0.05d),
                RaidOutcome.ClearFailure => (0.70d, 0.15d),
                RaidOutcome.HeadquartersDestroyed => (1.00d, 0.20d),
                _ => (0d, 0d)
            };
        }

        private static int CalculateLoss(int amount, double rate, double lossMultiplier)
        {
            return Math.Max(0, (int)Math.Round(amount * rate * lossMultiplier));
        }
    }
}
