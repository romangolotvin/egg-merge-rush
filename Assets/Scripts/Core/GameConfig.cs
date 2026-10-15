namespace EggMerge.Core
{
    public static class GameConfig
    {
        public const string AppName = "Egg Merge Rush";
        public const string SoftCurrencyName = "Монеты";

        public const int MaxEnergy = 8;
        public const int EnergyPerLevel = 1;
        public const int EnergyRegenSeconds = 15 * 60;
        public const int EnergyFromAd = 3;

        public const int BoardWidth = 5;
        public const int BoardHeight = 6;
        public const int MaxEggTier = 5;

        public const int BaseMoves = 22;
        public const int MinMoves = 10;
        public const int ContinueExtraMoves = 5;
        public const int MaxContinuesPerLevel = 2;

        public const int BaseTargetScore = 80;
        public const float TargetScoreGrowth = 1.08f;

        public const int CoinRewardBase = 20;
        public const int HatchSecondsTier3 = 5 * 60;
        public const int HatchSecondsTier4 = 15 * 60;
        public const int HatchSecondsTier5 = 30 * 60;
    }
}
