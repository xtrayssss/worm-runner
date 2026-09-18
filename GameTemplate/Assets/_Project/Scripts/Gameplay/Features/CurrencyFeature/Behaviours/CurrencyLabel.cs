using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours
{
    public sealed class CurrencyLabel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _currencyText;

        [SerializeField] private Transform _animationTarget;

        [Header("Text Settings")]
        [SerializeField] private string _currencyFormat = "{0:N0}";

        private Tween _punchTween;
        private CurrencyService _currencyService;
        private CurrencyType _currencyType;
        private Vector3 _originalScale;

        public void Construct(CurrencyService currencyService, CurrencyType currencyType)
        {
            _currencyType = currencyType;
            _currencyService = currencyService;
            _originalScale = _animationTarget.localScale;
        }

        private void Start()
        {
            _currencyService.OnCurrencyChanged += OnCurrencyChanged;

            int initialAmount = _currencyService.GetCurrency(_currencyType);
            SetCurrency(initialAmount, false);
        }

        private void OnDestroy()
        {
            _currencyService.OnCurrencyChanged -= OnCurrencyChanged;
            StopAllAnimations();
        }

        private void OnCurrencyChanged(CurrencyService.CurrencyUpdateInfo currencyUpdateInfo)
        {
            if (currencyUpdateInfo.Delta == 0 || currencyUpdateInfo.CurrencyType != _currencyType)
                return;

            SetCurrency((int)currencyUpdateInfo.NewValue);
        }

        private void SetCurrency(int amount, bool animate = true)
        {
            if (!animate)
            {
                UpdateDisplayText(amount);
                return;
            }

            PlayPunchAnimation();

            UpdateDisplayText(amount);
        }

        private void PlayPunchAnimation()
        {
            StopPunchAnimation();

            _animationTarget.localScale = _originalScale;
            
            Vector3 punchScale = _originalScale * 1.1f;

            Tween.PunchScale(
                _animationTarget,
                settings: new ShakeSettings
                {
                    duration = 0.17f,
                    asymmetry = 0.5f,
                    frequency = 1,
                    cycles = 1,
                    strength = punchScale,
                    enableFalloff = true,
                    easeBetweenShakes = Ease.OutQuart
                });
        }

        private void UpdateDisplayText(int amount) =>
            _currencyText.text = string.Format(_currencyFormat, amount);

        private void StopAllAnimations()
        {
            StopPunchAnimation();
        }

        private void StopPunchAnimation()
        {
            if (_punchTween.isAlive)
                _punchTween.Stop();
        }
    }
}