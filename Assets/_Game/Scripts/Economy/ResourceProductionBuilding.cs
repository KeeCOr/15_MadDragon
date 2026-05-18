using System;

namespace MedievalRTS.Economy
{
    [Serializable]
    public class ResourceProductionBuilding
    {
        public string Id { get; }
        public ResourceType ResourceType { get; }
        public float ProductionPerSecond { get; }
        public int Capacity { get; }

        private float _fractionalCarry;

        public ResourceProductionBuilding(string id, ResourceType resourceType, float productionPerSecond, int capacity)
        {
            Id = id;
            ResourceType = resourceType;
            ProductionPerSecond = Math.Max(0f, productionPerSecond);
            Capacity = Math.Max(0, capacity);
        }

        public int Produce(float deltaSeconds, int currentStored)
        {
            if (deltaSeconds <= 0f || ProductionPerSecond <= 0f) return 0;
            float raw = ProductionPerSecond * deltaSeconds + _fractionalCarry;
            int generated = (int)Math.Floor(raw);
            _fractionalCarry = raw - generated;
            int remainingCapacity = Math.Max(0, Capacity - currentStored);
            return Math.Min(generated, remainingCapacity);
        }
    }
}
