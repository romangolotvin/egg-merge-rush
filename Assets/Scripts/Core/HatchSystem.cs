using System;
using EggMerge.Ads;
using UnityEngine;

namespace EggMerge.Core
{
    public class HatchSystem
    {
        readonly SaveData _data;
        readonly System.Random _rng = new System.Random();

        public HatchSave Active => _data.activeHatch;
        public bool HasActive => _data.activeHatch != null && _data.activeHatch.eggTier > 0;
        public event Action Changed;

        public HatchSystem(SaveData data) => _data = data;

        public bool TryStartFromTier(int tier)
        {
            if (tier < 3 || HasActive)
                return false;

            var seconds = tier switch
            {
                3 => GameConfig.HatchSecondsTier3,
                4 => GameConfig.HatchSecondsTier4,
                _ => GameConfig.HatchSecondsTier5
            };

            _data.activeHatch = new HatchSave
            {
                eggTier = tier,
                finishUtcTicks = DateTime.UtcNow.AddSeconds(seconds).Ticks,
                rewardPetId = PetCatalog.RandomId(_rng)
            };
            Changed?.Invoke();
            return true;
        }

        public TimeSpan TimeLeft()
        {
            if (!HasActive) return TimeSpan.Zero;
            var finish = new DateTime(_data.activeHatch.finishUtcTicks, DateTimeKind.Utc);
            var left = finish - DateTime.UtcNow;
            return left < TimeSpan.Zero ? TimeSpan.Zero : left;
        }

        public bool IsReady() => HasActive && TimeLeft() <= TimeSpan.Zero;

        public PetSave ClaimIfReady()
        {
            if (!IsReady()) return null;

            var petId = _data.activeHatch.rewardPetId;
            _data.activeHatch = null;

            var existing = _data.pets.Find(p => p.id == petId);
            if (existing != null)
            {
                existing.count++;
                existing.level = Mathf.Min(10, existing.level + 1);
                Changed?.Invoke();
                return existing;
            }

            var created = new PetSave { id = petId, level = 1, count = 1 };
            _data.pets.Add(created);
            Changed?.Invoke();
            return created;
        }

        public void SpeedUpWithAd(AdService ads, Action<bool> done)
        {
            if (!HasActive)
            {
                done?.Invoke(false);
                return;
            }

            ads.ShowRewarded("hatch_speedup", success =>
            {
                if (success && HasActive)
                {
                    _data.activeHatch.finishUtcTicks = DateTime.UtcNow.Ticks;
                    _data.totalAdsWatched++;
                    Changed?.Invoke();
                }

                done?.Invoke(success);
            });
        }
    }
}
