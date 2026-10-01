// Assets/_Game/Scripts/Buildings/DefenseTower.cs
using UnityEngine;
using MedievalRTS.Units;

namespace MedievalRTS.Buildings
{
    [RequireComponent(typeof(Building))]
    public class DefenseTower : MonoBehaviour
    {
        private Building _building;
        private float _attackTimer;

        private void Awake() => _building = GetComponent<Building>();

        private void Update()
        {
            if (!_building.IsAlive || !_building.Data.isDefenseTower) return;

            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f) return;

            var target = FindNearestEnemyUnit();
            if (target == null) return;

            target.TakeDamage(_building.Data.attackDamage);
            _attackTimer = _building.Data.attackCooldown;
        }

        private Unit FindNearestEnemyUnit()
        {
            string enemyTag = _building.IsPlayerBuilding ? "EnemyUnit" : "PlayerUnit";
            var candidates = GameObject.FindGameObjectsWithTag(enemyTag);
            float nearest = float.MaxValue;
            Unit result = null;

            foreach (var c in candidates)
            {
                var unit = c.GetComponent<Unit>();
                if (unit == null || !unit.IsAlive) continue;
                float d = Vector3.Distance(transform.position, c.transform.position);
                if (d <= _building.Data.attackRange && d < nearest)
                {
                    nearest = d;
                    result = unit;
                }
            }
            return result;
        }
    }
}
