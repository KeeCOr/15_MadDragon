namespace MedievalRTS.Economy
{
    public class RaidForecast
    {
        public RaidOutcome Outcome { get; }
        public float ProtectionRate { get; }
        public ResourceWallet StoredLoss { get; }
        public ResourceWallet OwnedLoss { get; }

        public RaidForecast(RaidOutcome outcome, float protectionRate, ResourceWallet storedLoss, ResourceWallet ownedLoss)
        {
            Outcome = outcome;
            ProtectionRate = protectionRate;
            StoredLoss = storedLoss;
            OwnedLoss = ownedLoss;
        }
    }
}
