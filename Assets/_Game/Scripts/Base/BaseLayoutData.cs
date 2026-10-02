// Assets/_Game/Scripts/Base/BaseLayoutData.cs
using System;
using System.Collections.Generic;

namespace MedievalRTS.Base
{
    [Serializable]
    public class PlacedBuilding
    {
        public string BuildingName;
        public int GridX;
        public int GridY;
    }

    [Serializable]
    public class BaseLayoutData
    {
        public List<PlacedBuilding> Buildings { get; set; } = new();
    }
}
