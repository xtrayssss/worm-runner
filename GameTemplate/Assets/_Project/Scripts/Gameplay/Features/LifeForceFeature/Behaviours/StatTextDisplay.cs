using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature.Behaviours
{
    public class StatTextDisplay : StatDisplay
    {
        [FormerlySerializedAs("_healthText")] [SerializeField]
        private TMP_Text _statText;

        private bool _isVisible = true;
        protected virtual string Format => "F2";
        public override bool IsVisible => _isVisible;

        public override void UpdateDisplay(float currentValue, float maxValue)
        {
            _statText.text = currentValue.ToString(Format);
        }

        public override void Show()
        {
            _isVisible = true;
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            _isVisible = false;
            gameObject.SetActive(false);
        }
    }
}