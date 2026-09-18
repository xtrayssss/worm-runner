using System;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.RewardFeature
{
    [Serializable]
    public class CurrencyReward : IReward
    {
        [SerializeField]
        private CurrencyType _currencyType = CurrencyType.MONEY;

        [SerializeField]
        private RewardValue _amount = new RewardValue(0);

        public RewardType Type => _currencyType == CurrencyType.GOLD ? RewardType.GOLD : RewardType.CRYSTAL;
        public bool IsEmpty => _amount.IsZero();
        public CurrencyType CurrencyType => _currencyType;
        public RewardValue Amount => _amount;

#if UNITY_EDITOR
        public CurrencyReward()
        {
        }
#endif

        public CurrencyReward(CurrencyType currencyType, int amount)
        {
            _currencyType = currencyType;
            _amount = new RewardValue(amount);
        }

        public CurrencyReward(CurrencyType currencyType, int min, int max)
        {
            _currencyType = currencyType;
            _amount = new RewardValue(min, max);
        }

        private CurrencyReward(CurrencyType currencyType, RewardValue amount)
        {
            _currencyType = currencyType;
            _amount = amount;
        }

        public IReward Combine(IReward other)
        {
            if (other is not CurrencyReward otherCurrency || otherCurrency._currencyType != _currencyType)
                return this;

            return new CurrencyReward(_currencyType, _amount.Combine(otherCurrency._amount));
        }

        public IReward Multiply(float multiplier)
        {
            RewardValue multipliedAmount = Amount.Multiply(multiplier);
            return new CurrencyReward(CurrencyType, multipliedAmount);
        }

        public IReward Copy()
        {
            return new CurrencyReward(_currencyType, _amount.Copy());
        }

        public void GenerateFinalValues()
        {
            _amount.GenerateValue();
        }
    }
}