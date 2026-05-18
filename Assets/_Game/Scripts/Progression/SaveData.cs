// Assets/_Game/Scripts/Progression/SaveData.cs
using System.Collections.Generic;
using MedievalRTS.Base;
using MedievalRTS.Economy;

namespace MedievalRTS.Progression
{
    public class SaveData
    {
        public Dictionary<int, int> StageStars { get; set; } = new();
        public Dictionary<string, int> UnitLevels { get; set; } = new();
        public BaseLayoutData PlayerBase { get; set; }

        public ResourceWallet OwnedResources { get; set; } = new();
        public ResourceWallet StoredResources { get; set; } = new();
        public int HeadquartersLevel { get; set; } = 1;
        public long LastCollectionUnixSeconds { get; set; }
    }
}
