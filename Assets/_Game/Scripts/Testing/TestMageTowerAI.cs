// Assets/_Game/Scripts/Testing/TestMageTowerAI.cs
// 마법사 탑: 범위 내 최대 N개 유닛 동시 공격
using UnityEngine;
using MedievalRTS.Buildings;
using MedievalRTS.Units;

namespace MedievalRTS.Testing
{
    public class TestMageTowerAI : MonoBehaviour
    {
        private Building _building;
        private bool  _isPlayer;
        private float _range, _cooldown;
        private int   _damage, _targetCount;
        private float _timer;

        public void Setup(bool isPlayer, float range, int dmg, float cooldown, int targets = 2)
        {
            _isPlayer    = isPlayer;
            _range       = range;
            _damage      = dmg;
            _cooldown    = cooldown;
            _targetCount = targets;
        }

        private void Awake() { _building = GetComponent<Building>(); }

        private void Update()
        {
            if (_building == null || !_building.IsAlive) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = _cooldown;

            string tag = _isPlayer ? "EnemyUnit" : "PlayerUnit";
            int hits = 0;
            foreach (var go in GameObject.FindGameObjectsWithTag(tag))
            {
                if (hits >= _targetCount) break;
                if (go == null) continue;
                var u = go.GetComponent<Unit>();
                if (u == null || !u.IsAlive) continue;
                if (Vector3.Distance(transform.position, go.transform.position) <= _range)
                {
                    u.TakeDamage(_damage);
                    hits++;
                }
            }
        }
    }
}
