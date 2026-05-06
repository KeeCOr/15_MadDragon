// Assets/_Game/Scripts/Testing/TestSimpleUnitAI.cs
using UnityEngine;
using MedievalRTS.Units;
using MedievalRTS.Buildings;

namespace MedievalRTS.Testing
{
    [RequireComponent(typeof(Unit))]
    public class TestSimpleUnitAI : MonoBehaviour
    {
        // ── 스탯 ───────────────────────────────────────────────
        private float _moveSpeed      = 3f;
        private float _attackRange    = 1.8f;
        private float _attackCooldown = 1f;
        private float _threatRange    = 7f;
        private float _bldgDmgMult    = 1f;

        // ── 상태 ───────────────────────────────────────────────
        private Unit      _unit;
        private Transform _autoTarget;
        private Transform _cmdTarget;
        private Vector3?  _cmdPos;
        private float     _attackTimer;
        private float     _retargetTimer;
        private const float RetargetInterval = 0.5f;

        /// <summary>생산 직후 명령 대기 중 — 황색 링 표시</summary>
        public bool AwaitingOrders { get; private set; }

        /// <summary>명령 잠금 — 한 번 받은 명령은 변경 불가</summary>
        public bool CommandLocked { get; private set; }

        /// <summary>버프 건물이 일시 적용하는 공격력 배율</summary>
        public float DamageMultiplier { get; set; } = 1f;

        /// <summary>버프 건물이 일시 적용하는 이동속도 배율</summary>
        public float SpeedMultiplier  { get; set; } = 1f;

        // ── 선택/대기 링 ────────────────────────────────────────
        private GameObject _ring;
        private Material   _ringMat;

        private static readonly Color ColorAwaiting  = Color.yellow;
        private static readonly Color ColorSelected  = Color.green;

        public bool IsSelected
        {
            get => _ring != null && _ring.activeSelf && !AwaitingOrders;
            set
            {
                if (_ring == null) return;
                if (value)
                {
                    _ring.SetActive(true);
                    _ringMat.color = ColorSelected;
                }
                else
                {
                    // 명령 대기 중이면 황색 유지, 아니면 끄기
                    _ring.SetActive(AwaitingOrders);
                    if (AwaitingOrders) _ringMat.color = ColorAwaiting;
                }
            }
        }

        // ── 초기화 ─────────────────────────────────────────────
        public void Setup(float speed, float atkRange, float cooldown,
                          float threat = 7f, float bldgMult = 1f)
        {
            _moveSpeed      = speed;
            _attackRange    = atkRange;
            _attackCooldown = cooldown;
            _threatRange    = threat;
            _bldgDmgMult    = bldgMult;
        }

        /// <summary>플레이어 유닛 생산 시 호출 — 명령 대기 상태로 진입</summary>
        public void SetAwaitingOrders()
        {
            AwaitingOrders = true;
            CommandLocked  = false;
            if (_ring != null) { _ring.SetActive(true); _ringMat.color = ColorAwaiting; }
        }

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            BuildRing();
        }

        private void BuildRing()
        {
            _ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _ring.name = "Ring";
            _ring.transform.SetParent(transform, false);
            _ring.transform.localPosition = Vector3.down * 0.48f;
            _ring.transform.localScale    = new Vector3(1.6f, 0.02f, 1.6f);
            DestroyImmediate(_ring.GetComponent<Collider>());

            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Standard");
            _ringMat = new Material(shader);
            _ring.GetComponent<Renderer>().material = _ringMat;
            _ring.SetActive(false);
        }

        // ── 전술 명령 (잠금 준수) ──────────────────────────────
        public void CommandAttack(Transform target)
        {
            if (CommandLocked) return;
            AwaitingOrders = false;
            CommandLocked  = true;
            _cmdTarget = target;
            _cmdPos    = null;
            IsSelected = false;
        }

        public void CommandMove(Vector3 worldPos)
        {
            if (CommandLocked) return;
            AwaitingOrders = false;
            CommandLocked  = true;
            _cmdPos    = worldPos;
            _cmdTarget = null;
            IsSelected = false;
        }

        public void ResumeAuto()
        {
            _cmdTarget     = null;
            _cmdPos        = null;
            // CommandLocked / AwaitingOrders 는 건드리지 않음 (플레이어가 해제 불가)
        }

        // ── 업데이트 ───────────────────────────────────────────
        private void Update()
        {
            if (_unit == null || _unit.CurrentHp <= 0) return;
            if (AwaitingOrders) return; // 명령 대기 중 → 정지

            // 이동 명령 처리
            if (_cmdPos.HasValue)
            {
                float d = Vector3.Distance(transform.position, _cmdPos.Value);
                if (d > 0.8f) { MoveToward(_cmdPos.Value); return; }
                _cmdPos = null; // 도착 → 자동 교전
            }

            var target = ResolveTarget();
            if (target == null) return;

            float dist = Vector3.Distance(transform.position, target.position);
            if (dist > _attackRange)
                MoveToward(target.position);
            else
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f) { _attackTimer = _attackCooldown; Attack(target); }
            }
        }

        private Transform ResolveTarget()
        {
            if (_cmdTarget != null && IsAlive(_cmdTarget)) return _cmdTarget;
            _cmdTarget = null;

            _retargetTimer -= Time.deltaTime;
            if (_retargetTimer <= 0f || _autoTarget == null || !IsAlive(_autoTarget))
            {
                _retargetTimer = RetargetInterval;
                _autoTarget = FindBestTarget();
            }
            return _autoTarget;
        }

        private Transform FindBestTarget()
        {
            string unitTag  = _unit.IsPlayerUnit ? "EnemyUnit"     : "PlayerUnit";
            string bldgTag  = _unit.IsPlayerUnit ? "EnemyBuilding" : "PlayerBuilding";

            Transform best = null;
            float bestDist = _threatRange;
            foreach (var go in GameObject.FindGameObjectsWithTag(unitTag))
            {
                var u = go.GetComponent<Unit>();
                if (u == null || u.CurrentHp <= 0) continue;
                float d = Vector3.Distance(transform.position, go.transform.position);
                if (d < bestDist) { bestDist = d; best = go.transform; }
            }
            if (best != null) return best;

            bestDist = float.MaxValue;
            foreach (var go in GameObject.FindGameObjectsWithTag(bldgTag))
            {
                var b = go.GetComponent<Building>();
                if (b == null || b.CurrentHp <= 0) continue;
                float d = Vector3.Distance(transform.position, go.transform.position);
                if (d < bestDist) { bestDist = d; best = go.transform; }
            }
            return best;
        }

        private void MoveToward(Vector3 dest)
        {
            Vector3 dir = (dest - transform.position).normalized;
            transform.position += dir * _moveSpeed * SpeedMultiplier * Time.deltaTime;
        }

        private void Attack(Transform target)
        {
            int dmg = Mathf.RoundToInt(_unit.EffectiveDamage * DamageMultiplier);
            var b = target.GetComponent<Building>();
            if (b != null) { b.TakeDamage(Mathf.RoundToInt(dmg * _bldgDmgMult)); return; }
            target.GetComponent<Unit>()?.TakeDamage(dmg);
        }

        private static bool IsAlive(Transform t)
        {
            if (t == null) return false;
            var u = t.GetComponent<Unit>(); if (u != null) return u.CurrentHp > 0;
            var b = t.GetComponent<Building>(); if (b != null) return b.CurrentHp > 0;
            return false;
        }
    }
}
