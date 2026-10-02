using System;
using UnityEngine;

namespace MedievalRTS.Data
{
    [Serializable]
    public struct BuildingPlacement
    {
        public BuildingData data;
        public Vector2Int gridPosition;
    }

    [Serializable]
    public struct WaveData
    {
        public UnitData[] units;
        public float spawnInterval;
        public float waveStartTime;
    }

    [CreateAssetMenu(fileName = "StageData", menuName = "Medieval RTS/Stage Data")]
    public class StageData : ScriptableObject
    {
        public string stageName;
        public int stageIndex;
        public float battleDuration;
        public int unlockRequirementStars;
        public BuildingPlacement[] enemyBase;
        public WaveData[] waves;
    }
}
