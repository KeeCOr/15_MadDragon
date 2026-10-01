namespace MedievalRTS.UI
{
    public struct BattlePriority { public string Risk; public string Resource; public string Action; }
    public enum BattleTimeTone { Stable, Warning, Critical }

    public struct BattleHudStatus
    {
        public string TimeText;
        public string ObjectiveText;
        public string RewardText;
        public BattleTimeTone TimeTone;
    }

    public static class BattlePriorityModel
    {
        public static BattlePriority Build(float baseHealthRatio, int availableSupply, bool enemyNearBase)
        {
            if (baseHealthRatio <= 0.35f) return new BattlePriority { Risk = "Base critical", Resource = $"Supply {availableSupply}", Action = "Recall defenders now" };
            if (enemyNearBase) return new BattlePriority { Risk = "Enemy at base", Resource = $"Supply {availableSupply}", Action = "Deploy a frontline unit" };
            if (availableSupply < 2) return new BattlePriority { Risk = "Low supply", Resource = $"Supply {availableSupply}", Action = "Hold position and regenerate" };
            return new BattlePriority { Risk = "Front stable", Resource = $"Supply {availableSupply}", Action = "Advance one lane" };
        }

        public static BattleHudStatus BuildHudStatus(int secondsRemaining, int targetHp, int earnedGold, int earnedHonor)
        {
            int seconds = secondsRemaining < 0 ? 0 : secondsRemaining;
            int hp = targetHp < 0 ? 0 : targetHp;
            int gold = earnedGold < 0 ? 0 : earnedGold;
            int honor = earnedHonor < 0 ? 0 : earnedHonor;

            return new BattleHudStatus
            {
                TimeText = $"TIME  {seconds}s",
                ObjectiveText = $"OBJECTIVE  BASE HP  {hp:n0}",
                RewardText = $"REWARD  +{gold:n0}G  +{honor:n0} HONOR",
                TimeTone = seconds <= 10
                    ? BattleTimeTone.Critical
                    : seconds <= 30 ? BattleTimeTone.Warning : BattleTimeTone.Stable
            };
        }
    }
}
