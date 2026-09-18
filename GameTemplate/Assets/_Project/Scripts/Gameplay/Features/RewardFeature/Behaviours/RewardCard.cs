using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours
{
    public sealed class RewardCard : BaseRewardCard
    {
        public class RewardDetail
        {
            public bool IsUnknown { get; set; }
            public RewardType RewardType { get; set; }
            public Sprite Icon { get; set; }
            public bool IsRange { get; set; }
            public int FixedAmount { get; set; }
            public Vector2Int RangeAmount { get; set; }
        }

        private Vector2Int _baseQuantityRange;
        private int _baseQuantity;

        public void DisplayReward(RewardDetail detail, bool isDoubled)
        {
            Icon.sprite = detail.Icon;

            if (!detail.IsRange)
            {
                _baseQuantity = detail.FixedAmount;
                UpdateAmountLabel(isDoubled);
            }
            else
            {
                _baseQuantityRange = detail.RangeAmount;
                UpdateRangeAmountLabel(isDoubled);
            }

            Display(detail.RewardType);
        }

        public void UpdateAmountLabel(bool isDoubled)
        {
            int quantity = isDoubled ? _baseQuantity * 2 : _baseQuantity;
            AmountLabel.text = $"x{quantity}";

            if (isDoubled)
                ShowDoubleEffect();
        }

        private void UpdateRangeAmountLabel(bool isDoubled)
        {
            Vector2Int quantityRange = isDoubled ? _baseQuantityRange * 2 : _baseQuantityRange;
            AmountLabel.text = $"{quantityRange.x}-{quantityRange.y}";

            if (isDoubled)
                ShowDoubleEffect();
        }
    }
}