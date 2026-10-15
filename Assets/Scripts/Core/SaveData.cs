using System;
using System.Collections.Generic;

namespace EggMerge.Core
{
    [Serializable]
    public class SaveData
    {
        public int coins = 0;
        public int energy = GameConfig.MaxEnergy;
        public long lastEnergyUtcTicks = DateTime.UtcNow.Ticks;
        public int level = 1;
        public int bestTierUnlocked = 1;
        public List<PetSave> pets = new List<PetSave>();
        public HatchSave activeHatch;
        public int totalAdsWatched;
    }

    [Serializable]
    public class PetSave
    {
        public string id;
        public int level = 1;
        public int count = 1;
    }

    [Serializable]
    public class HatchSave
    {
        public int eggTier;
        public long finishUtcTicks;
        public string rewardPetId;
    }
}
