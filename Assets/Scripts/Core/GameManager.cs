using System;
using EggMerge.Ads;
using EggMerge.Merge;
using EggMerge.UI;
using UnityEngine;

namespace EggMerge.Core
{
    public enum GameScreen
    {
        Hub,
        Level,
        Win,
        Lose,
        NoEnergy
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public SaveData Save { get; private set; }
        public EnergySystem Energy { get; private set; }
        public CurrencySystem Coins { get; private set; }
        public HatchSystem Hatch { get; private set; }
        public MergeBoard Board { get; private set; }
        public GameScreen Screen { get; private set; } = GameScreen.Hub;

        public int CurrentLevel => Save.level;
        public int TargetScore { get; private set; }
        public int ContinuesUsed { get; private set; }
        public int LastReward { get; private set; }
        public string StatusMessage { get; private set; } = "";

        public event Action StateChanged;

        float _saveTimer;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (AdService.Instance == null)
            {
                var adsGo = new GameObject("AdService");
                adsGo.AddComponent<AdService>();
            }

            Save = SaveSystem.Load();
            Energy = new EnergySystem(Save);
            Coins = new CurrencySystem(Save);
            Hatch = new HatchSystem(Save);

            Energy.Changed += PersistAndNotify;
            Coins.Changed += PersistAndNotify;
            Hatch.Changed += PersistAndNotify;

            gameObject.AddComponent<GameUI>();
            Notify();
        }

        void Update()
        {
            Energy.Tick();
            _saveTimer += Time.unscaledDeltaTime;
            if (_saveTimer > 5f)
            {
                _saveTimer = 0f;
                SaveSystem.Save(Save);
            }
        }

        void OnApplicationPause(bool pause)
        {
            if (pause) SaveSystem.Save(Save);
        }

        void OnApplicationQuit() => SaveSystem.Save(Save);

        public void GoHub()
        {
            Screen = GameScreen.Hub;
            Board = null;
            StatusMessage = "";
            Notify();
        }

        public void TryStartLevel()
        {
            Energy.Tick();
            if (!Energy.CanSpend())
            {
                Screen = GameScreen.NoEnergy;
                StatusMessage = "Нет энергии. Посмотри рекламу: +3.";
                Notify();
                return;
            }

            if (!Energy.TrySpend())
                return;

            ContinuesUsed = 0;
            TargetScore = LevelRules.TargetScore(Save.level);
            var moves = LevelRules.MovesForLevel(Save.level);
            Board = new MergeBoard(moves, Save.level * 997 + Save.coins);
            Board.Changed += OnBoardChanged;
            Screen = GameScreen.Level;
            StatusMessage = $"Уровень {Save.level}";
            PersistAndNotify();
        }

        void OnBoardChanged()
        {
            if (Screen != GameScreen.Level || Board == null)
                return;

            Save.bestTierUnlocked = Mathf.Max(Save.bestTierUnlocked, Board.HighestTier);

            if (Board.HighestTier >= 3 && !Hatch.HasActive)
                Hatch.TryStartFromTier(Board.HighestTier);

            if (Board.Score >= TargetScore)
            {
                WinLevel();
                return;
            }

            if (!Board.HasMoves)
            {
                Screen = GameScreen.Lose;
                StatusMessage = "Ходы закончились";
                Notify();
            }
            else
            {
                Notify();
            }
        }

        void WinLevel()
        {
            var mult = PetCatalog.TotalCoinMultiplier(Save.pets);
            LastReward = LevelRules.CoinReward(Save.level, mult);
            Coins.Add(LastReward);
            Save.level++;
            Screen = GameScreen.Win;
            StatusMessage = "Победа!";
            PersistAndNotify();

            if (Save.level % 3 == 0)
                AdService.Instance?.ShowInterstitial("between_levels");
        }

        public void DoubleRewardWithAd()
        {
            if (Screen != GameScreen.Win || LastReward <= 0)
                return;

            AdService.Instance.ShowRewarded("double_coins", ok =>
            {
                if (!ok) return;
                Coins.Add(LastReward);
                LastReward *= 2;
                Save.totalAdsWatched++;
                StatusMessage = "Награда x2!";
                PersistAndNotify();
            });
        }

        public void ContinueWithAd()
        {
            if (Screen != GameScreen.Lose || Board == null)
                return;
            if (ContinuesUsed >= GameConfig.MaxContinuesPerLevel)
            {
                StatusMessage = "Лимит продолжений на уровень";
                Notify();
                return;
            }

            AdService.Instance.ShowRewarded("continue", ok =>
            {
                if (!ok) return;
                ContinuesUsed++;
                Save.totalAdsWatched++;
                Board.AddMoves(GameConfig.ContinueExtraMoves);
                Screen = GameScreen.Level;
                StatusMessage = $"+{GameConfig.ContinueExtraMoves} ходов";
                PersistAndNotify();
            });
        }

        public void RefillEnergyWithAd()
        {
            AdService.Instance.ShowRewarded("energy_plus3", ok =>
            {
                if (!ok) return;
                Energy.Add(GameConfig.EnergyFromAd);
                Save.totalAdsWatched++;
                StatusMessage = $"+{GameConfig.EnergyFromAd} энергии";
                Screen = GameScreen.Hub;
                PersistAndNotify();
            });
        }

        public void SpeedUpHatch()
        {
            Hatch.SpeedUpWithAd(AdService.Instance, ok =>
            {
                StatusMessage = ok ? "Яйцо готово!" : "Реклама не показалась";
                Notify();
            });
        }

        public void ClaimHatch()
        {
            var pet = Hatch.ClaimIfReady();
            if (pet == null)
            {
                StatusMessage = "Ещё рано";
                Notify();
                return;
            }

            var info = PetCatalog.Get(pet.id);
            StatusMessage = $"Вылупился: {info.Name} ур.{pet.level}";
            PersistAndNotify();
        }

        void PersistAndNotify()
        {
            SaveSystem.Save(Save);
            Notify();
        }

        void Notify() => StateChanged?.Invoke();
    }
}
