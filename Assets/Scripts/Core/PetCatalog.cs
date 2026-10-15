using System.Collections.Generic;

namespace EggMerge.Core
{
    public class PetInfo
    {
        public string Id;
        public string Name;
        public string BonusText;
        public float CoinBonusPerLevel;
    }

    public static class PetCatalog
    {
        public static readonly PetInfo[] All =
        {
            new PetInfo
            {
                Id = "chick",
                Name = "Цыпёнок",
                BonusText = "+монеты за уровень",
                CoinBonusPerLevel = 0.05f
            },
            new PetInfo
            {
                Id = "bunny",
                Name = "Кролик",
                BonusText = "+удача к дропу яиц",
                CoinBonusPerLevel = 0.03f
            },
            new PetInfo
            {
                Id = "dragon",
                Name = "Дракончик",
                BonusText = "+очки за merge",
                CoinBonusPerLevel = 0.08f
            }
        };

        public static PetInfo Get(string id)
        {
            foreach (var p in All)
                if (p.Id == id) return p;
            return All[0];
        }

        public static string RandomId(System.Random rng)
        {
            return All[rng.Next(All.Length)].Id;
        }

        public static float TotalCoinMultiplier(IList<PetSave> pets)
        {
            var mult = 1f;
            foreach (var pet in pets)
            {
                var info = Get(pet.id);
                mult += info.CoinBonusPerLevel * pet.level * pet.count;
            }

            return mult;
        }
    }
}
