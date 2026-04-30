using UnityEngine;

namespace MedievalRTS.Data
{
    public enum UnitType { Knight, Archer, Catapult, Scout, Mage }

    [CreateAssetMenu(fileName = "UnitData", menuName = "Medieval RTS/Unit Data")]
    public class UnitData : ScriptableObject
    {
        public string unitName;
        public UnitType unitType;
        public int maxHp;
        public int damage;
        public float moveSpeed;
        public int goldCost;
        public float attackRange;
        public float attackCooldown;
        public float sightRange;
        public GameObject prefab;
        public Sprite icon;
    }
}
