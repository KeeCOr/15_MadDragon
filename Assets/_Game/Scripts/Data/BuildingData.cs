using UnityEngine;

namespace MedievalRTS.Data
{
    public enum BuildingType { Castle, ArcherTower, CannonTower, Wall, Barracks, GoldMine }

    [CreateAssetMenu(fileName = "BuildingData", menuName = "Medieval RTS/Building Data")]
    public class BuildingData : ScriptableObject
    {
        public string buildingName;
        public BuildingType buildingType;
        public int maxHp;
        public Vector2Int gridSize;
        public int goldCost;
        public bool isDefenseTower;
        public float attackRange;
        public int attackDamage;
        public float attackCooldown;
        public int goldProductionPerSecond;
        public GameObject prefab;
        public Sprite icon;
    }
}
