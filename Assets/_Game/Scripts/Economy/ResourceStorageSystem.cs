using System;
using System.Collections.Generic;

namespace MedievalRTS.Economy
{
    public class ResourceStorageSystem
    {
        private readonly List<ResourceProductionBuilding> _producers = new();

        public int HeadquartersLevel { get; private set; }
        public ResourceWallet Stored { get; } = new();

        public float ProtectionRate => Math.Min(0.75f, HeadquartersLevel * 0.10f);

        public ResourceStorageSystem(int headquartersLevel)
        {
            HeadquartersLevel = Math.Max(1, headquartersLevel);
        }

        public void SetHeadquartersLevel(int level)
        {
            HeadquartersLevel = Math.Max(1, level);
        }

        public void AddProducer(ResourceProductionBuilding producer)
        {
            if (producer == null) throw new ArgumentNullException(nameof(producer));
            _producers.Add(producer);
        }

        public int GetHeadquartersCapacity(ResourceType type)
        {
            return type switch
            {
                ResourceType.Gold => HeadquartersLevel * 1000,
                ResourceType.Honor => HeadquartersLevel * 100,
                ResourceType.Stars => int.MaxValue,
                _ => HeadquartersLevel * 1000
            };
        }

        public void TickProduction(float deltaSeconds)
        {
            var producerCapacityByResource = new Dictionary<ResourceType, int>();
            foreach (var producer in _producers)
            {
                int currentCapacity = producerCapacityByResource.TryGetValue(producer.ResourceType, out var capacity)
                    ? capacity
                    : 0;
                long aggregateCapacity = (long)currentCapacity + producer.Capacity;
                producerCapacityByResource[producer.ResourceType] = (int)Math.Min(
                    int.MaxValue,
                    aggregateCapacity);
            }

            foreach (var producer in _producers)
            {
                int currentStored = Stored.Get(producer.ResourceType);
                int generated = producer.Produce(deltaSeconds, 0);
                int capacity = Math.Min(
                    producerCapacityByResource[producer.ResourceType],
                    GetHeadquartersCapacity(producer.ResourceType));
                int clamped = Math.Min(generated, Math.Max(0, capacity - currentStored));
                Stored.Add(producer.ResourceType, clamped);
            }
        }

        public ResourceWallet CollectAll(ResourceWallet owned)
        {
            if (owned == null) throw new ArgumentNullException(nameof(owned));
            var collected = Stored.Copy();
            foreach (var pair in collected.Amounts)
                owned.Add(pair.Key, pair.Value);
            Stored.Clear();
            return collected;
        }
    }
}
