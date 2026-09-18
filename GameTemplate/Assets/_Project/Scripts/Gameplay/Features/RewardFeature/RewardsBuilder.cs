using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;

namespace _Project.Scripts.Gameplay.Features.RewardFeature
{
    public sealed class RewardsBuilder
    {
        private readonly Rewards _rewards;

        private RewardsBuilder()
        {
            _rewards = new Rewards();
        }

        public static RewardsBuilder Create() => new RewardsBuilder();

        public RewardsBuilder WithGold(int amount)
        {
            _rewards.AddReward(new CurrencyReward(CurrencyType.MONEY, amount));
            return this;
        }

        public RewardsBuilder WithGold(int min, int max)
        {
            _rewards.AddReward(new CurrencyReward(CurrencyType.MONEY, min, max));
            return this;
        }

        public RewardsBuilder WithCrystals(int amount)
        {
            _rewards.AddReward(new CurrencyReward(CurrencyType.GEMS, amount));
            return this;
        }

        public RewardsBuilder WithCrystals(int min, int max)
        {
            _rewards.AddReward(new CurrencyReward(CurrencyType.GEMS, min, max));
            return this;
        }

        public RewardsBuilder WithFragment(int type, int amount)
        {
            FragmentsReward reward = _rewards.GetReward<FragmentsReward>() ?? new FragmentsReward();
            reward.AddFragment(type, amount);
            _rewards.AddReward(reward);
            return this;
        }

        public RewardsBuilder WithFragment(int type, int min, int max)
        {
            FragmentsReward reward = _rewards.GetReward<FragmentsReward>() ?? new FragmentsReward();
            reward.AddFragment(type, min, max);
            _rewards.AddReward(reward);
            return this;
        }
        
        public RewardsBuilder WithMoney(int amount)
        {
            _rewards.AddReward(new CurrencyReward(CurrencyType.MONEY, amount));
            return this;
        }

        public RewardsBuilder WithMoney(int min, int max)
        {
            _rewards.AddReward(new CurrencyReward(CurrencyType.MONEY, min, max));
            return this;
        }
        
        public RewardsBuilder WithMultiplier(float multiplier)
        {
            if (multiplier <= 0)
                return this;
            
            Rewards multipliedRewards = _rewards.Multiply(multiplier);
            _rewards.AllRewards.Clear();
            
            foreach (IReward reward in multipliedRewards.AllRewards)
                _rewards.AddReward(reward);
                
            return this;
        }

        public RewardsBuilder WithMaxFragments(int maxTotal)
        {
            FragmentsReward reward = _rewards.GetReward<FragmentsReward>() ?? new FragmentsReward();
            reward.SetMaxTotal(maxTotal);
            _rewards.AddReward(reward);
            return this;
        }

        public RewardsBuilder FromExistingRewards(Rewards existingRewards)
        {
            _rewards.AllRewards.Clear();

            foreach (IReward reward in existingRewards.AllRewards)
            {
                _rewards.AddReward(reward.Copy());
            }

            return this;
        }

        public RewardsBuilder CombineWith(Rewards additionalRewards)
        {
            _rewards.Combine(additionalRewards);

            return this;
        }

        public Rewards Build(bool convertRangeToFixed = false)
        {
            FragmentsReward fragmentReward = _rewards.GetReward<FragmentsReward>();
            List<CurrencyReward> otherRewards = _rewards.GetRewards<CurrencyReward>();

            foreach (CurrencyReward reward in otherRewards)
                reward.GenerateFinalValues();

            if (fragmentReward is { IsEmpty: false })
                fragmentReward.GenerateFinalValues();

            _rewards.AllRewards.RemoveAll(static r => r.IsEmpty);

            if (convertRangeToFixed)
            {
                _rewards.AllRewards = _rewards.AllRewards.Select(static r =>
                {
                    switch (r)
                    {
                        case CurrencyReward currencyReward when currencyReward.Amount.IsRange:
                        {
                            return new CurrencyReward(currencyReward.CurrencyType, currencyReward.Amount.FinalValue);
                        }
                        case FragmentsReward fragmentsReward:
                        {
                            FragmentsReward newFragmentsReward = new FragmentsReward(fragmentsReward.MaxTotalFragments);

                            foreach ((int key, RewardValue value) in fragmentsReward.Fragments)
                                newFragmentsReward.AddFragment(key, value.FinalValue);

                            return newFragmentsReward;
                        }
                        default:
                            return r;
                    }
                }).ToList();
            }

            return _rewards;
        }
    }
}