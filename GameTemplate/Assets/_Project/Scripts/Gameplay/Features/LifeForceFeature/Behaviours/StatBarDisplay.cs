using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature.Behaviours
{
    public class StatBarDisplay : StatDisplay
    {
        [FormerlySerializedAs("_healthSlider")] [SerializeField]
        private Slider _statSlider;

        [SerializeField] private bool _smoothTransition = true;
        [SerializeField] private float _transitionSpeed = 5f;

        private float _targetValue;
        private bool _isVisible = true;

        public override bool IsVisible => _isVisible;

        public override void UpdateDisplay(float currentValue, float maxValue)
        {
            _targetValue = currentValue / maxValue;
            
            if (!_smoothTransition)
            {
                ApplyStatValue(_targetValue);
            }
        }

        private void Update()
        {
            if (_smoothTransition && _isVisible)
            {
                float currentValue = _statSlider.value;
                float newValue = Mathf.MoveTowards(currentValue, _targetValue, _transitionSpeed * Time.deltaTime);
                ApplyStatValue(newValue);
            }
        }

        private void ApplyStatValue(float value) => 
            _statSlider.value = value;

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