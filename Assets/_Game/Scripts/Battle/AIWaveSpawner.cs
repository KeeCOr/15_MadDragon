// Assets/_Game/Scripts/Battle/AIWaveSpawner.cs
using System.Collections;
using UnityEngine;
using MedievalRTS.Data;

namespace MedievalRTS.Battle
{
    public class AIWaveSpawner : MonoBehaviour
    {
        [SerializeField] private SpawnZone _enemySpawnZone;

        private UnitSpawner _spawner;

        private void Start() => _spawner = FindObjectOfType<UnitSpawner>();

        public void BeginWaves(StageData stage)
        {
            foreach (var wave in stage.waves)
                StartCoroutine(SpawnWave(wave));
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            yield return new WaitForSeconds(wave.waveStartTime);
            foreach (var unitData in wave.units)
            {
                _spawner.SpawnEnemyUnit(unitData, _enemySpawnZone.GetRandomPosition());
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }
}
