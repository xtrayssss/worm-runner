using System;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature.Services
{
    [Serializable]
    public class CurrencyService : IService
    {
        [ShowInInspector]
        private int _money;

        [ShowInInspector]
        private int _silver;

        [ShowInInspector]
        private int _gems;

        [ShowInInspector]
        private int _gold;

        private readonly SaveLoadService _saveLoadService;

        public readonly struct CurrencyUpdateInfo
        {
            public readonly CurrencyType CurrencyType;
            public readonly float Delta;
            public readonly float NewValue;

            public CurrencyUpdateInfo(CurrencyType currencyType, float delta, float newValue)
            {
                CurrencyType = currencyType;
                Delta = delta;
                NewValue = newValue;
            }
        }

        public int Money => _money;
        public float Silver => _silver;
        public int Gems => _gems;

        public event Action<CurrencyUpdateInfo> OnCurrencyChanged;

        public CurrencyService(ConfigsService configs,
            CurrencyFlyAnimationService currencyFlyAnimationService,
            SaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
        }

        public void Initialize() =>
            LoadCurrencyData();

        private void LoadCurrencyData()
        {
            PlayerSaveData saveData = _saveLoadService.PlayerSaveData;

            _gems = saveData.Gems;
            _money = saveData.Money;
        }

        private void SaveCurrencyData()
        {
            PlayerSaveData saveData = _saveLoadService.PlayerSaveData;

            saveData.Gems = _gems;
            saveData.Money = _money;

            _saveLoadService.SaveData();
        }

        public void AddMoney(int amount, bool queueAnimation = false)
        {
            if (Mathf.Approximately(_money + amount, _money))
                return;

            _money += amount;

            OnCurrencyChanged?.Invoke(
                new CurrencyUpdateInfo(
                    CurrencyType.MONEY,
                    amount,
                    _money
                ));

            SaveCurrencyData();
        }

        public void AddSilver(int amount)
        {
            if (Mathf.Approximately(_silver + amount, _silver))
                return;

            _silver += amount;

            OnCurrencyChanged?.Invoke(
                new CurrencyUpdateInfo(
                    CurrencyType.SILVER,
                    amount,
                    _silver
                ));

            SaveCurrencyData();
        }

        public void SetSilver(int amount)
        {
            if (Mathf.Approximately(_silver, amount))
                return;

            _silver = amount;

            OnCurrencyChanged?.Invoke(
                new CurrencyUpdateInfo(
                    CurrencyType.SILVER,
                    0,
                    _silver));

            SaveCurrencyData();
        }

        public void AddGems(int amount, bool queueAnimation = false)
        {
            if (Mathf.Approximately(_gems + amount, _gems))
                return;

            _gems += amount;

            OnCurrencyChanged?.Invoke(
                new CurrencyUpdateInfo(
                    CurrencyType.GEMS,
                    amount,
                    _gems));

            SaveCurrencyData();
        }

        public void AddGold(int amount, bool queueAnimation = false)
        {
            if (Mathf.Approximately(_gold + amount, _gold))
                return;

            _gold += amount;

            OnCurrencyChanged?.Invoke(
                new CurrencyUpdateInfo(
                    CurrencyType.GOLD,
                    amount,
                    _gold
                ));

            SaveCurrencyData();
        }

        public bool TrySpendGold(int amount)
        {
            if (HasEnough(CurrencyType.GOLD, amount))
            {
                _gold -= amount;

                OnCurrencyChanged?.Invoke(
                    new CurrencyUpdateInfo(
                        CurrencyType.GOLD,
                        -amount,
                        _gold));

                SaveCurrencyData();

                return true;
            }

            return false;
        }

        public bool TrySpendMoney(int amount)
        {
            if (HasEnough(CurrencyType.MONEY, amount))
            {
                _money -= amount;
                OnCurrencyChanged?.Invoke(
                    new CurrencyUpdateInfo(
                        CurrencyType.MONEY,
                        -amount,
                        _money));

                SaveCurrencyData();

                return true;
            }

            return false;
        }

        public bool TrySpendSilver(int amount)
        {
            if (HasEnough(CurrencyType.SILVER, amount))
            {
                _silver -= amount;

                OnCurrencyChanged?.Invoke(
                    new CurrencyUpdateInfo(
                        CurrencyType.SILVER,
                        -amount,
                        _silver));

                SaveCurrencyData();

                return true;
            }

            return false;
        }

        public bool TrySpendGems(int amount)
        {
            if (HasEnough(CurrencyType.GEMS, amount))
            {
                _gems -= amount;

                OnCurrencyChanged?.Invoke(
                    new CurrencyUpdateInfo(
                        CurrencyType.GEMS,
                        -amount,
                        _gems));

                SaveCurrencyData();

                return true;
            }

            return false;
        }

        public bool HasEnough(CurrencyType currencyType, int amount)
        {
            return currencyType switch
            {
                CurrencyType.MONEY => HasEnoughCurrency(_money),
                CurrencyType.SILVER => HasEnoughCurrency(_silver),
                CurrencyType.GEMS => HasEnoughCurrency(_gems),
                CurrencyType.GOLD => HasEnoughCurrency(_gold),
                _ => false
            };

            bool HasEnoughCurrency(float currency)
                => amount <= currency;
        }

        [Button]
        public bool TrySpend(CurrencyType currencyType, int amount)
        {
            return currencyType switch
            {
                CurrencyType.MONEY => TrySpendMoney(amount),
                CurrencyType.SILVER => TrySpendSilver(amount),
                CurrencyType.GEMS => TrySpendGems(amount),
                _ => false
            };
        }

        [Button]
        public void AddCurrency(CurrencyType currencyType = CurrencyType.MONEY, int amount = 100,
            bool queueAnimation = false)
        {
            switch (currencyType)
            {
                case CurrencyType.MONEY:
                    AddMoney(amount);
                    break;
                case CurrencyType.SILVER:
                    AddSilver(amount);
                    break;
                case CurrencyType.GEMS:
                    AddGems(amount);
                    break;
                case CurrencyType.FRAGMENTS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currencyType), currencyType, null);
            }
        }

        public int GetCurrency(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.MONEY => _money,
                CurrencyType.SILVER => _silver,
                CurrencyType.GEMS => _gems,
                CurrencyType.FRAGMENTS => 0,
                _ => 0
            };
        }
    }
}