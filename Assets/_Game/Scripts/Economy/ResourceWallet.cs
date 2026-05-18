using System;
using System.Collections.Generic;

namespace MedievalRTS.Economy
{
    [Serializable]
    public class ResourceWallet
    {
        public Dictionary<ResourceType, int> Amounts { get; set; } = new();

        public int Get(ResourceType type)
        {
            return Amounts.TryGetValue(type, out var value) ? value : 0;
        }

        public void Set(ResourceType type, int amount)
        {
            Amounts[type] = Math.Max(0, amount);
        }

        public void Add(ResourceType type, int amount)
        {
            if (amount <= 0) return;
            Set(type, Get(type) + amount);
        }

        public int Remove(ResourceType type, int amount)
        {
            if (amount <= 0) return 0;
            int current = Get(type);
            int removed = Math.Min(current, amount);
            Set(type, current - removed);
            return removed;
        }

        public bool CanSpend(ResourceType type, int amount)
        {
            return amount >= 0 && Get(type) >= amount;
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            if (!CanSpend(type, amount)) return false;
            Remove(type, amount);
            return true;
        }

        public ResourceWallet Copy()
        {
            var copy = new ResourceWallet();
            foreach (var pair in Amounts)
                copy.Set(pair.Key, pair.Value);
            return copy;
        }

        public void Clear()
        {
            Amounts.Clear();
        }
    }
}
