using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours
{
    public abstract class BaseRewardCard : MonoBehaviour
    {
        [FormerlySerializedAs("_icon")] [SerializeField]
        protected Image Icon;

        [FormerlySerializedAs("_amountLabel")] [SerializeField]
        protected TextMeshProUGUI AmountLabel;

        [FormerlySerializedAs("_highlightEffect")] [SerializeField]
        protected GameObject HighlightEffect;

        [FormerlySerializedAs("_border")] [SerializeField]
        protected Image Border;

        [SerializeField] protected GameObject FragmentTag;

        [FormerlySerializedAs("_button")] [SerializeField]
        private Button _interactionButton;

        public Button InteractionButton => _interactionButton;

        public CanvasGroup CanvasGroup { get; private set; }
        public RectTransform RectTransform { get; private set; }

        public void Construct()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            RectTransform = GetComponent<RectTransform>();
        }

        protected void Display(RewardType rewardType)
        {
            if (rewardType == RewardType.FRAGMENT)
                FragmentTag.SetActive(true);
        }

        protected void ShowDoubleEffect()
        {
            HighlightEffect.SetActive(true);
            Border.gameObject.SetActive(false);
        }
    }
}