using UnityEngine;

namespace EggMerge.Core
{
    public static class LevelRules
    {
        public static int TargetScore(int level)
        {
            return Mathf.RoundToInt(GameConfig.BaseTargetScore * Mathf.Pow(GameConfig.TargetScoreGrowth, level - 1));
        }

        public static int MovesForLevel(int level)
        {
            var moves = GameConfig.BaseMoves - (level - 1) / 3;
            return Mathf.Max(GameConfig.MinMoves, moves);
        }

        public static int CoinReward(int level, float petMult)
        {
            var baseReward = GameConfig.CoinRewardBase + level * 3;
            return Mathf.RoundToInt(baseReward * petMult);
        }
    }
}
