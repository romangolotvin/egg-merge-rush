using System;

namespace EggMerge.Core
{
    public class CurrencySystem
    {
        readonly SaveData _data;

        public int Coins => _data.coins;
        public event Action Changed;

        public CurrencySystem(SaveData data) => _data = data;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _data.coins += amount;
            Changed?.Invoke();
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || _data.coins < amount)
                return false;
            _data.coins -= amount;
            Changed?.Invoke();
            return true;
        }
    }
}
