using MedievalRTS.Core;
using MedievalRTS.Data;
using UnityEngine;

namespace MedievalRTS.Campaign
{
    public static class StarRatingSystem
    {
        private const float FastClearThreshold = 0.7f;

        public static int Calculate(BattleStats stats, StageData stage)
        {
            if (!stats.Victory) return 0;

            int stars = 1; // 승리 기본 1개

            if (stats.UnitsLost == 0) stars++;

            if (stats.TimeElapsed <= stage.battleDuration * FastClearThreshold) stars++;

            return Mathf.Clamp(stars, 0, 3);
        }
    }
}
