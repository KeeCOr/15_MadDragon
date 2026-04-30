// Assets/_Game/Scripts/Units/FogRevealAgent.cs
// 실제 FOW 에셋 API에 맞게 Update() 내부를 교체한다.
using UnityEngine;

namespace MedievalRTS.Units
{
    [RequireComponent(typeof(Unit))]
    public class FogRevealAgent : MonoBehaviour
    {
        private Unit _unit;

        private void Awake() => _unit = GetComponent<Unit>();

        private void Update()
        {
            if (!_unit.IsAlive) return;
            // TODO: 구매한 FOW 에셋 API 호출로 교체
            // 예시: FogOfWarManager.Instance.Reveal(transform.position, _unit.Data.sightRange);
        }
    }
}
