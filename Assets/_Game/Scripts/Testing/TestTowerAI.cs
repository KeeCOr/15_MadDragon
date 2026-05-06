// Assets/_Game/Scripts/Testing/TestTowerAI.cs
// 테스트 씬용 타워 AI — NavMesh 불필요, 범위 내 적 유닛 자동 공격
using UnityEngine;
using MedievalRTS.Units;
using MedievalRTS.Buildings;

namespace MedievalRTS.Testing
{
    [RequireComponent(typeof(Building))]
    public class TestTowerAI : MonoBehaviour
    {
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private int damage = 15;
        [SerializeField] private bool isPlayerTower = true;

        private float _timer;
        private Building _building;

        public void Setup(bool isPlayer, float range = 8f, int dmg = 15, float cooldown = 1.5f)
        {
            isPlayerTower = isPlayer;
            attackRange = range;
            damage = dmg;
            attackCooldown = cooldown;
        }

        private void Awake()
        {
            _building = GetComponent<Building>();
        }

        private void Update()
        {
            if (_building == null || _building.CurrentHp <= 0) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            string enemyTag = isPlayerTower ? "EnemyUnit" : "PlayerUnit";
            Unit nearest = null;
            float bestDist = attackRange;

            foreach (var go in GameObject.FindGameObjectsWithTag(enemyTag))
            {
                var u = go.GetComponent<Unit>();
                if (u == null || u.CurrentHp <= 0) continue;
                float d = Vector3.Distance(transform.position, go.transform.position);
                if (d < bestDist) { bestDist = d; nearest = u; }
            }

            if (nearest != null)
            {
                nearest.TakeDamage(damage);
                _timer = attackCooldown;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
