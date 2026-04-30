// Assets/_Game/Scripts/Battle/UnitSpawner.cs
using UnityEngine;
using MedievalRTS.Data;
using MedievalRTS.Units;

namespace MedievalRTS.Battle
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private SpawnZone _playerSpawnZone;

        private ResourceManager _resources;

        private void Start() => _resources = FindObjectOfType<ResourceManager>();

        public bool TrySpawnPlayerUnit(UnitData data)
        {
            if (!_resources.TrySpend(data.goldCost)) return false;
            SpawnUnit(data, _playerSpawnZone.GetRandomPosition(), isPlayer: true);
            return true;
        }

        public void SpawnEnemyUnit(UnitData data, Vector3 position)
        {
            SpawnUnit(data, position, isPlayer: false);
        }

        private void SpawnUnit(UnitData data, Vector3 position, bool isPlayer)
        {
            var go = Instantiate(data.prefab, position, Quaternion.identity);
            go.GetComponent<Unit>().Initialize(data, isPlayer);
        }
    }
}
