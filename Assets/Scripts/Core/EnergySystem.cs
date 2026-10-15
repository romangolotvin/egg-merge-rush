using System;
using UnityEngine;

namespace EggMerge.Core
{
    public class EnergySystem
    {
        readonly SaveData _data;

        public int Energy => _data.energy;
        public int Max => GameConfig.MaxEnergy;
        public event Action Changed;

        public EnergySystem(SaveData data)
        {
            _data = data;
            ApplyRegen();
        }

        public void Tick()
        {
            if (_data.energy >= GameConfig.MaxEnergy)
            {
                _data.lastEnergyUtcTicks = DateTime.UtcNow.Ticks;
                return;
            }

            if (ApplyRegen())
                Changed?.Invoke();
        }

        bool ApplyRegen()
        {
            if (_data.energy >= GameConfig.MaxEnergy)
                return false;

            var last = new DateTime(_data.lastEnergyUtcTicks, DateTimeKind.Utc);
            var elapsed = (DateTime.UtcNow - last).TotalSeconds;
            if (elapsed < GameConfig.EnergyRegenSeconds)
                return false;

            var gained = (int)(elapsed / GameConfig.EnergyRegenSeconds);
            if (gained <= 0)
                return false;

            var before = _data.energy;
            _data.energy = Mathf.Min(GameConfig.MaxEnergy, _data.energy + gained);
            var used = _data.energy - before;
            _data.lastEnergyUtcTicks = last.AddSeconds(used * GameConfig.EnergyRegenSeconds).Ticks;
            return used > 0;
        }

        public bool CanSpend(int amount = GameConfig.EnergyPerLevel)
        {
            ApplyRegen();
            return _data.energy >= amount;
        }

        public bool TrySpend(int amount = GameConfig.EnergyPerLevel)
        {
            ApplyRegen();
            if (_data.energy < amount)
                return false;

            if (_data.energy >= GameConfig.MaxEnergy)
                _data.lastEnergyUtcTicks = DateTime.UtcNow.Ticks;

            _data.energy -= amount;
            Changed?.Invoke();
            return true;
        }

        public void Add(int amount)
        {
            ApplyRegen();
            _data.energy = Mathf.Min(GameConfig.MaxEnergy, _data.energy + amount);
            if (_data.energy >= GameConfig.MaxEnergy)
                _data.lastEnergyUtcTicks = DateTime.UtcNow.Ticks;
            Changed?.Invoke();
        }

        public TimeSpan TimeToNext()
        {
            ApplyRegen();
            if (_data.energy >= GameConfig.MaxEnergy)
                return TimeSpan.Zero;

            var last = new DateTime(_data.lastEnergyUtcTicks, DateTimeKind.Utc);
            var next = last.AddSeconds(GameConfig.EnergyRegenSeconds);
            var left = next - DateTime.UtcNow;
            return left < TimeSpan.Zero ? TimeSpan.Zero : left;
        }
    }
}
