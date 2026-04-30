// Assets/_Game/Scripts/Units/UnitAI.cs
using UnityEngine;
using UnityEngine.AI;
using MedievalRTS.Buildings;

namespace MedievalRTS.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Unit))]
    public class UnitAI : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Unit _unit;
        private Transform _target;
        private float _attackTimer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _unit = GetComponent<Unit>();
        }

        private void Start()
        {
            _agent.speed = _unit.Data.moveSpeed;
            _agent.stoppingDistance = _unit.Data.attackRange * 0.9f;
        }

        private void Update()
        {
            if (!_unit.IsAlive) return;

            if (_target == null || !_target.gameObject.activeInHierarchy)
                FindTarget();

            if (_target == null) return;

            float dist = Vector3.Distance(transform.position, _target.position);
            if (dist > _unit.Data.attackRange)
            {
                _agent.SetDestination(_target.position);
            }
            else
            {
                _agent.ResetPath();
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    Attack();
                    _attackTimer = _unit.Data.attackCooldown;
                }
            }
        }

        private void FindTarget()
        {
            string enemyUnitTag     = _unit.IsPlayerUnit ? "EnemyUnit"     : "PlayerUnit";
            string enemyBuildingTag = _unit.IsPlayerUnit ? "EnemyBuilding" : "PlayerBuilding";

            float nearest = float.MaxValue;
            _target = null;

            foreach (var tag in new[] { enemyUnitTag, enemyBuildingTag })
            foreach (var go in GameObject.FindGameObjectsWithTag(tag))
            {
                float d = Vector3.Distance(transform.position, go.transform.position);
                if (d < nearest) { nearest = d; _target = go.transform; }
            }
        }

        private void Attack()
        {
            if (_target == null) return;

            var targetUnit = _target.GetComponent<Unit>();
            if (targetUnit != null) { targetUnit.TakeDamage(_unit.EffectiveDamage); return; }

            var targetBuilding = _target.GetComponent<Building>();
            targetBuilding?.TakeDamage(_unit.EffectiveDamage);
        }
    }
}
