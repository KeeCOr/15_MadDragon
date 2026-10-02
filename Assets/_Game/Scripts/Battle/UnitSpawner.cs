// Assets/_Game/Scripts/Battle/UnitSpawner.cs
using UnityEngine;
using MedievalRTS.Data;
using MedievalRTS.Units;
using MedievalRTS.Progression;

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
            var go = Instantiate(data.prefab, _playerSpawnZone.GetRandomPosition(), Quaternion.identity);
            go.GetComponent<Unit>().Initialize(data, isPlayerUnit: true, save: SaveSystem.Load());
            return true;
        }

        public void SpawnEnemyUnit(UnitData data, Vector3 position)
        {
            var go = Instantiate(data.prefab, position, Quaternion.identity);
            go.GetComponent<Unit>().Initialize(data, isPlayerUnit: false);
        }
    }
}
