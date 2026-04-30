// Assets/_Game/Scripts/Progression/UnitUpgradeSystem.cs
using MedievalRTS.Data;

namespace MedievalRTS.Progression
{
    public static class UnitUpgradeSystem
    {
        private const float LevelMultiplier = 0.2f; // 레벨당 +20%

        public static int GetMaxHp(UnitData data, SaveData save)
        {
            int level = GetLevel(data, save);
            return UnityEngine.Mathf.RoundToInt(data.maxHp * (1f + (level - 1) * LevelMultiplier));
        }

        public static int GetDamage(UnitData data, SaveData save)
        {
            int level = GetLevel(data, save);
            return UnityEngine.Mathf.RoundToInt(data.damage * (1f + (level - 1) * LevelMultiplier));
        }

        public static int GetLevel(UnitData data, SaveData save)
        {
            return save.UnitLevels.TryGetValue(data.unitName, out int lvl)
                ? UnityEngine.Mathf.Clamp(lvl, 1, 3)
                : 1;
        }

        public static bool TryLevelUp(UnitData data, SaveData save)
        {
            int current = GetLevel(data, save);
            if (current >= 3) return false;
            save.UnitLevels[data.unitName] = current + 1;
            return true;
        }
    }
}
