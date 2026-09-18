using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.RewardFeature
{
    [Serializable]
    public sealed class RewardValue
    {
        [SerializeField]
        private int _fixedValue = 5;

        [SerializeField]
        private Vector2Int _rangeValue;

        [SerializeField]
        private bool _isRange;

        public int FixedValue => _fixedValue;
        public Vector2Int RangeValue => _rangeValue;
        public bool IsRange => _isRange;
        public int FinalValue { get; set; }

        public RewardValue(int fixedValue)
        {
            _fixedValue = fixedValue;
            _isRange = false;
            FinalValue = fixedValue;
        }

        public RewardValue(int min, int max)
        {
            _rangeValue = new Vector2Int(min, max);
            _isRange = true;
            FinalValue = 0;
        }

        public void GenerateValue()
        {
            FinalValue = _isRange
                ? UnityEngine.Random.Range(_rangeValue.x, _rangeValue.y + 1)
                : _fixedValue;
        }

        public RewardValue Combine(RewardValue other)
        {
            if (IsZero()) return other.Copy();
            if (other.IsZero()) return Copy();

            if (!_isRange && !other._isRange)
                return new RewardValue(_fixedValue + other._fixedValue);

            Vector2Int thisRange = _isRange ? _rangeValue : new Vector2Int(_fixedValue, _fixedValue);
            Vector2Int otherRange =
                other._isRange ? other._rangeValue : new Vector2Int(other._fixedValue, other._fixedValue);

            return new RewardValue(thisRange.x + otherRange.x, thisRange.y + otherRange.y);
        }

        public RewardValue Multiply(float multiplier)
        {
            if (multiplier <= 0)
                return new RewardValue(0);

            if (_isRange)
            {
                int newMin = Mathf.RoundToInt(_rangeValue.x * multiplier);
                int newMax = Mathf.RoundToInt(_rangeValue.y * multiplier);
                return new RewardValue(newMin, newMax);
            }

            int newValue = Mathf.RoundToInt(_fixedValue * multiplier);
            return new RewardValue(newValue);
        }

        public RewardValue Copy()
        {
            return _isRange ? new RewardValue(_rangeValue.x, _rangeValue.y) : new RewardValue(_fixedValue);
        }

        public bool IsZero() =>
            _isRange ? (_rangeValue.x == 0 && _rangeValue.y == 0) : _fixedValue == 0;
    }
}