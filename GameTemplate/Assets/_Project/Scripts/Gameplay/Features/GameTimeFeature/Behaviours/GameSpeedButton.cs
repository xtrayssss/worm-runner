using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.GameTimeFeature.Behaviours
{
    public sealed class GameSpeedButton : MonoBehaviour
    {
        [field: SerializeField] public GameTimeService.SpeedMultiplier SpeedMultiplier { get; private set; }
        [field: SerializeField] public Image ButtonBackground { get; private set; }
        [SerializeField] private Image _adIcon;
        [field: SerializeField] public TextMeshProUGUI SpeedText { get; private set; }
        [SerializeField] private TextMeshProUGUI _unlockText;
        [SerializeField] private Button _button;

        private GameTimeService _gameTimeService;
        private bool _isUnlocked;
        public Button Button => _button;

        public void Construct(GameTimeService gameTimeService, bool requiresAd = false)
        {
            _gameTimeService = gameTimeService;
            _isUnlocked = !requiresAd || SpeedMultiplier == GameTimeService.SpeedMultiplier.NORMAL;

            SpeedText.text = $"x{(int)SpeedMultiplier}";

            UpdateVisualState();

            _gameTimeService.OnSpeedChanged += OnGameTimeChanged;

            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            _gameTimeService.OnSpeedChanged -= OnGameTimeChanged;

            _button.onClick.RemoveListener(OnButtonClicked);
        }

        public void SetUnlocked(bool unlocked)
        {
            _isUnlocked = unlocked;
            UpdateVisualState();
        }

        private void OnButtonClicked()
        {
            if (_gameTimeService.CurrentSpeed == SpeedMultiplier)
                return;

            if (!_isUnlocked)
                return;

            _gameTimeService.SetSpeed(SpeedMultiplier);
        }

        private void OnGameTimeChanged(GameTimeService.SpeedMultiplier newSpeed)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            bool isActive = _gameTimeService.CurrentSpeed == SpeedMultiplier;
            bool shouldShow = ShouldShowButton();

            gameObject.SetActive(shouldShow);

            if (_unlockText != null)
                _unlockText.gameObject.SetActive(!_isUnlocked);

            if (_adIcon != null)
                _adIcon.gameObject.SetActive(!_isUnlocked);

            if (!shouldShow)
                return;

            if (isActive)
                ApplyDefaultColor();
            else
                ApplyGrayscale();
        }

        private bool ShouldShowButton()
        {
            return SpeedMultiplier switch
            {
                GameTimeService.SpeedMultiplier.NORMAL => true,
                GameTimeService.SpeedMultiplier.DOUBLE => true,
                GameTimeService.SpeedMultiplier.QUADRUPLE => _gameTimeService.IsDoubleSpeedUnlocked,
                _ => false
            };
        }

        private void ApplyGrayscale() =>
            ButtonBackground.color = Color.gray;

        private void ApplyDefaultColor() =>
            ButtonBackground.color = Color.white;
    }
}