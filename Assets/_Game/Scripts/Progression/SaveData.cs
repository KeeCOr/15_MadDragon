// Assets/_Game/Scripts/Progression/SaveData.cs
using System.Collections.Generic;
using MedievalRTS.Base;

namespace MedievalRTS.Progression
{
    public class SaveData
    {
        public Dictionary<int, int> StageStars { get; set; } = new();
        public Dictionary<string, int> UnitLevels { get; set; } = new();
        public BaseLayoutData PlayerBase { get; set; }
    }
}
