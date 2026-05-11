// Assets/_Game/Scripts/Buildings/Building.cs
using System;
using UnityEngine;
using MedievalRTS.Core;
using MedievalRTS.Data;

namespace MedievalRTS.Buildings
{
    public class Building : MonoBehaviour
    {
        public BuildingData Data { get; private set; }
        public int CurrentHp { get; private set; }
        public int MaxHp     { get; private set; }
        public bool IsAlive => CurrentHp > 0;
        public bool IsPlayerBuilding { get; private set; }

        public event Action<Building> OnDestroyed;

        public void Initialize(BuildingData data, bool isPlayerBuilding)
        {
            Data = data;
            MaxHp = isPlayerBuilding
                ? Mathf.RoundToInt(data.maxHp * BuildingEffectSystem.GetBuildingHPMultiplier())
                : data.maxHp;
            CurrentHp = MaxHp;
            IsPlayerBuilding = isPlayerBuilding;
            gameObject.tag = isPlayerBuilding ? "PlayerBuilding" : "EnemyBuilding";
        }

        public void Repair(int amount)
        {
            if (!IsAlive) return;
            CurrentHp = Mathf.Min(CurrentHp + amount, MaxHp);
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            if (CurrentHp == 0) DestroyBuilding();
        }

        private void DestroyBuilding()
        {
            OnDestroyed?.Invoke(this);
            EventBus.Publish(new BuildingDestroyedEvent(this));
            Destroy(gameObject, 0.3f);
        }
    }
}
