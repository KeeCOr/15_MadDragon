// Assets/_Game/Scripts/Battle/SpawnZone.cs
using UnityEngine;

namespace MedievalRTS.Battle
{
    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private BoxCollider _area;

        public Vector3 GetRandomPosition()
        {
            var b = _area.bounds;
            return new Vector3(
                Random.Range(b.min.x, b.max.x),
                b.center.y,
                Random.Range(b.min.z, b.max.z));
        }
    }
}
