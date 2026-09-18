using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StatisticsFeature
{
    [Serializable]
    public sealed class RunStatisticsService : IService
    {
        [ShowInInspector]
        public RunStatistics CurrentRun { get; } = new RunStatistics();

        private float _runStartTime;

        public void StartNewRun()
        {
            CurrentRun.Reset();
            _runStartTime = Time.time;
        }

        public void EndRun() => 
            CurrentRun.RunTime = Time.time - _runStartTime;

        public void AddRewards(Rewards rewards)
        {
            if (rewards.AllRewards.Count == 0)
                return;

            foreach (IReward reward in rewards.AllRewards)
            {
                switch (reward)
                {
                    case CurrencyReward currencyReward when currencyReward.Amount.FinalValue <= 0:
                        continue;
                    case CurrencyReward currencyReward:
                    {
                        switch (currencyReward.CurrencyType)
                        {
                            case CurrencyType.MONEY:
                                AddMoney(currencyReward.Amount.FinalValue);
                                break;
                        }

                        break;
                    }
                }
            }
        }

        private void AddMoney(int amount)
        {
            if (amount <= 0)
                return;

            CurrentRun.CollectedMoney += amount;
        }

        public void AddCrowdMemberLost() => 
            CurrentRun.CrowdMembersLost++;

        public void SetBonusChestTriggered(bool triggered) => 
            CurrentRun.IsBonusChestTriggered = triggered;

        public void SetDistanceTraveled(float distance) =>
            CurrentRun.DistanceTraveled = distance;
    }

    [Serializable]
    public class RunStatistics
    {
        [Header("Basic Stats")]
        [SerializeField]
        private int _collectedMoney;

        [SerializeField]
        private int _crowdMembersLost;

        [Header("Time & Distance")]
        [SerializeField]
        private float _runTime;

        [SerializeField]
        private float _distanceTraveled;

        [ShowInInspector]
        private bool _isBonusChestTriggered;

        public bool IsBonusChestTriggered
        {
            get => _isBonusChestTriggered;
            set => _isBonusChestTriggered = value;
        }

        public int CollectedMoney
        {
            get => _collectedMoney;
            set => _collectedMoney = value;
        }

        public int CrowdMembersLost
        {
            get => _crowdMembersLost;
            set => _crowdMembersLost = value;
        }

        public float RunTime
        {
            get => _runTime;
            set
            {
                if (!Mathf.Approximately(_runTime, value))
                {
                    _runTime = value;
                }
            }
        }

        public float DistanceTraveled
        {
            get => _distanceTraveled;
            set
            {
                if (!Mathf.Approximately(_distanceTraveled, value))
                {
                    _distanceTraveled = value;
                }
            }
        }

        public void Reset()
        {
            CollectedMoney = 0;
            CrowdMembersLost = 0;
            RunTime = 0f;
            DistanceTraveled = 0f;
            IsBonusChestTriggered = false;
        }
    }
}