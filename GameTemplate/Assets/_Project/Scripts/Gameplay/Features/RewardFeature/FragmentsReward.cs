using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.RewardFeature
{
    [Serializable]
    public class FragmentsReward : IReward
    {
        [SerializeField] 
            private Dictionary<int, RewardValue> _fragments;
        [SerializeField] 
            private int _maxTotalFragments;

        public RewardType Type => RewardType.FRAGMENT;

        public bool IsEmpty => _fragments == null || _fragments.Count == 0 ||
                               _fragments.All(static kvp => kvp.Value.IsZero());

        public Dictionary<int, RewardValue> Fragments => _fragments;
        public int MaxTotalFragments => _maxTotalFragments;

        public FragmentsReward(int maxTotalFragments = 0)
        {
            _fragments = new Dictionary<int, RewardValue>();
            _maxTotalFragments = maxTotalFragments;
        }

        public FragmentsReward AddFragment(int type, int amount)
        {
            _fragments[type] = new RewardValue(amount);
            return this;
        }

        public FragmentsReward AddFragment(int type, int min, int max)
        {
            _fragments[type] = new RewardValue(min, max);
            return this;
        }

        public FragmentsReward SetMaxTotal(int maxTotal)
        {
            _maxTotalFragments = maxTotal;
            return this;
        }

        public IReward Combine(IReward other)
        {
            if (other is not FragmentsReward otherArsenal)
                return this;

            FragmentsReward combined =
                new FragmentsReward(Math.Max(_maxTotalFragments, otherArsenal._maxTotalFragments));

            foreach (KeyValuePair<int, RewardValue> kvp in _fragments)
                combined._fragments[kvp.Key] = kvp.Value.Copy();

            foreach (KeyValuePair<int, RewardValue> kvp in otherArsenal._fragments)
            {
                if (combined._fragments.ContainsKey(kvp.Key))
                    combined._fragments[kvp.Key] = combined._fragments[kvp.Key].Combine(kvp.Value);
                else
                    combined._fragments[kvp.Key] = kvp.Value.Copy();
            }

            return combined;
        }
        
        public IReward Multiply(float multiplier)
        {
            int newMaxTotal = Mathf.RoundToInt(MaxTotalFragments * multiplier);
            FragmentsReward multipliedReward = new FragmentsReward(newMaxTotal);
        
            foreach ((int id, RewardValue value) in Fragments)
            {
                RewardValue multipliedValue = value.Multiply(multiplier);
                if (!multipliedValue.IsZero())
                    multipliedReward.Fragments[id] = multipliedValue;
            }
        
            return multipliedReward;
        }

        public IReward Copy()
        {
            FragmentsReward copy = new FragmentsReward(_maxTotalFragments);
            foreach (KeyValuePair<int, RewardValue> kvp in _fragments)
                copy._fragments[kvp.Key] = kvp.Value.Copy();
            return copy;
        }

        public void GenerateFinalValues()
        {
            if (_fragments == null || _fragments.Count == 0) return;

            List<KeyValuePair<int, RewardValue>> fixedFragments =
                _fragments.Where(static kvp => !kvp.Value.IsRange).ToList();
            List<KeyValuePair<int, RewardValue>> rangeFragments =
                _fragments.Where(static kvp => kvp.Value.IsRange).ToList();

            int totalFixed = 0;
            foreach ((_, RewardValue value) in fixedFragments)
            {
                value.GenerateValue();
                int clampedValue = Math.Min(value.FinalValue, _maxTotalFragments - totalFixed);
                value.FinalValue = clampedValue;
                totalFixed += clampedValue;
            }

            int remaining = Math.Max(0, _maxTotalFragments - totalFixed);
            if (rangeFragments.Count > 0 && remaining > 0)
            {
                DistributeRemainingFragments(rangeFragments, remaining);
            }

            List<int> keysToRemove = _fragments.Where(static kvp => kvp.Value.FinalValue <= 0)
                .Select(static kvp => kvp.Key)
                .ToList();

            foreach (int key in keysToRemove)
                _fragments.Remove(key);
        }

        private void DistributeRemainingFragments(List<KeyValuePair<int, RewardValue>> rangeFragments, int remaining)
        {
            int typesToUse = UnityEngine.Random.Range(1, Math.Min(rangeFragments.Count, remaining) + 1);
            List<KeyValuePair<int, RewardValue>> selectedFragments =
                rangeFragments.OrderBy(static _ => UnityEngine.Random.value).Take(typesToUse).ToList();

            for (int i = 0; i < selectedFragments.Count - 1; i++)
            {
                if (remaining <= 0)
                    break;

                KeyValuePair<int, RewardValue> fragment = selectedFragments[i];
                int maxForType = fragment.Value.RangeValue.y;
                int minForType = Math.Max(fragment.Value.RangeValue.x, 0);

                int amountForType = UnityEngine.Random.Range(minForType, Math.Min(remaining, maxForType) + 1);
                fragment.Value.FinalValue = amountForType;
                remaining -= amountForType;
            }

            if (selectedFragments.Count > 0 && remaining > 0)
            {
                KeyValuePair<int, RewardValue> lastFragment = selectedFragments.Last();
                int maxForLast = lastFragment.Value.RangeValue.y;
                int minForLast = lastFragment.Value.RangeValue.x;
                int finalAmount = Mathf.Clamp(remaining, minForLast, maxForLast);

                lastFragment.Value.FinalValue = finalAmount;
            }
        }
    }
}