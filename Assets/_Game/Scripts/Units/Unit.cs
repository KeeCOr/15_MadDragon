// Assets/_Game/Scripts/Units/Unit.cs
using System;
using UnityEngine;
using MedievalRTS.Core;
using MedievalRTS.Data;
using MedievalRTS.Progression;

namespace MedievalRTS.Units
{
    public class Unit : MonoBehaviour
    {
        public UnitData Data { get; private set; }
        public int CurrentHp { get; private set; }
        public bool IsAlive => CurrentHp > 0;
        public bool IsPlayerUnit { get; private set; }
        public int EffectiveDamage => _effectiveDamage;

        private int _effectiveDamage;

        public event Action<Unit> OnDied;

        public void Initialize(UnitData data, bool isPlayerUnit, SaveData save = null)
        {
            Data = data;
            if (isPlayerUnit && save != null)
            {
                CurrentHp = UnitUpgradeSystem.GetMaxHp(data, save);
                _effectiveDamage = UnitUpgradeSystem.GetDamage(data, save);
            }
            else
            {
                CurrentHp = data.maxHp;
                _effectiveDamage = data.damage;
            }
            IsPlayerUnit = isPlayerUnit;
            gameObject.tag = isPlayerUnit ? "PlayerUnit" : "EnemyUnit";
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            if (CurrentHp == 0) Die();
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;
            CurrentHp = Mathf.Min(CurrentHp + amount, Data.maxHp);
        }

        private void Die()
        {
            OnDied?.Invoke(this);
            EventBus.Publish(new UnitDiedEvent(this));
            Destroy(gameObject, 0.3f);
        }
    }
}
