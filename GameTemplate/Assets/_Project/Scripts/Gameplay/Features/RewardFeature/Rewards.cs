using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.RewardFeature
{
    [Serializable]
    public class Rewards
    {
        [SerializeReference] private List<IReward> _rewards;

        public List<IReward> AllRewards
        {
            get => _rewards ?? new List<IReward>();
            set => _rewards = value;
        }

        public Rewards()
        {
            _rewards = new List<IReward>();
        }

        public Rewards(params IReward[] rewards)
        {
            _rewards = new List<IReward>(rewards);
        }

        public T GetReward<T>() where T : class, IReward
        {
            return AllRewards.OfType<T>().FirstOrDefault();
        }

        public List<T> GetRewards<T>() where T : class, IReward
        {
            return AllRewards.OfType<T>().ToList();
        }

        public Rewards AddReward(IReward reward)
        {
            if (reward == null || reward.IsEmpty)
                return this;

            IReward existingReward =
                AllRewards.FirstOrDefault(r => r.Type == reward.Type && r.GetType() == reward.GetType());

            if (existingReward != null)
            {
                IReward combinedReward = existingReward.Combine(reward);
                AllRewards.Remove(existingReward);
                AllRewards.Add(combinedReward);
            }
            else
            {
                AllRewards.Add(reward);
            }

            return this;
        }

        public Rewards Combine(Rewards other)
        {
            if (other == null)
                return this;

            Rewards combined = new Rewards();

            foreach (IReward reward in AllRewards)
                combined.AddReward(reward.Copy());

            foreach (IReward reward in other.AllRewards)
                combined.AddReward(reward.Copy());

            return combined;
        }

        public void GenerateFinalValues()
        {
            foreach (IReward reward in AllRewards)
                reward.GenerateFinalValues();
        }

        public Rewards Multiply(float multiplier)
        {
            if (multiplier <= 0)
                return new Rewards();

            Rewards multipliedRewards = new Rewards();

            foreach (IReward reward in AllRewards)
            {
                IReward multipliedReward = reward.Multiply(multiplier);
            
                if (!multipliedReward.IsEmpty)
                    multipliedRewards.AddReward(multipliedReward);
            }

            return multipliedRewards;
        }

        public int GetTotalRewardTypeCount()
        {
            if (AllRewards.Count == 0)
                return 0;

            int count = 0;

            foreach (var reward in AllRewards)
            {
                if (!reward.IsEmpty)
                {
                    switch (reward)
                    {
                        case CurrencyReward:
                            count += 1;
                            break;
                        case FragmentsReward { Fragments: not null } fragmentsReward:
                            count += fragmentsReward.Fragments.Keys.Count;
                            break;
                    }
                }
            }

            return count;
        }
    }
}