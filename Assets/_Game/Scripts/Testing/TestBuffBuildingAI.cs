// Assets/_Game/Scripts/Testing/TestBuffBuildingAI.cs
// 버프 건물: 주기적으로 인근 아군 유닛 공격력·속도 강화
using System.Collections;
using UnityEngine;
using MedievalRTS.Buildings;

namespace MedievalRTS.Testing
{
    public class TestBuffBuildingAI : MonoBehaviour
    {
        private Building _building;
        private bool  _isPlayer;
        private float _radius;
        private float _dmgMult;
        private float _speedMult;
        private float _buffDuration;
        private float _interval;

        public void Setup(bool isPlayer, float radius = 9f,
                          float dmgMult = 1.3f, float speedMult = 1.2f,
                          float duration = 6f, float interval = 8f)
        {
            _isPlayer    = isPlayer;
            _radius      = radius;
            _dmgMult     = dmgMult;
            _speedMult   = speedMult;
            _buffDuration = duration;
            _interval    = interval;
        }

        private void Awake() { _building = GetComponent<Building>(); }
        private void Start()  { StartCoroutine(BuffRoutine()); }

        private IEnumerator BuffRoutine()
        {
            yield return new WaitForSeconds(_interval * 0.5f); // 첫 발동 딜레이
            while (_building != null && _building.IsAlive)
            {
                string tag = _isPlayer ? "PlayerUnit" : "EnemyUnit";
                foreach (var go in GameObject.FindGameObjectsWithTag(tag))
                {
                    if (go == null) continue;
                    var ai = go.GetComponent<TestSimpleUnitAI>();
                    if (ai == null) continue;
                    if (Vector3.Distance(transform.position, go.transform.position) <= _radius)
                        StartCoroutine(ApplyBuff(ai));
                }
                yield return new WaitForSeconds(_interval);
            }
        }

        private IEnumerator ApplyBuff(TestSimpleUnitAI ai)
        {
            if (ai == null) yield break;
            ai.DamageMultiplier = _dmgMult;
            ai.SpeedMultiplier  = _speedMult;
            yield return new WaitForSeconds(_buffDuration);
            if (ai != null) { ai.DamageMultiplier = 1f; ai.SpeedMultiplier = 1f; }
        }
    }
}
