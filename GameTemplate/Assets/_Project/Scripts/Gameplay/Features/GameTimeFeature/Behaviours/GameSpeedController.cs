using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.GameTimeFeature.Behaviours
{
    public sealed class GameSpeedController : MonoBehaviour
    {
        [SerializeField]
        private GameSpeedButton _normalSpeedButton;

        [SerializeField]
        private GameSpeedButton _doubleSpeedButton;

        [SerializeField]
        private GameSpeedButton _quadrupleSpeedButton;

        private GameTimeService _gameTimeService;
        private AdvertisementService _advertisementService;

        public void Construct(AllServices services)
        {
            _gameTimeService = services.Get<GameTimeService>();
            _advertisementService = services.Get<AdvertisementService>();

            InitializeSpeedButtons();
        }

        private void InitializeSpeedButtons()
        {
            _normalSpeedButton.Construct(_gameTimeService);

            _doubleSpeedButton.Construct(_gameTimeService, requiresAd: true);
            _doubleSpeedButton.SetUnlocked(_gameTimeService.IsDoubleSpeedUnlocked);

            _quadrupleSpeedButton.Construct(_gameTimeService, requiresAd: true);
            _quadrupleSpeedButton.SetUnlocked(_gameTimeService.IsQuadrupleSpeedUnlocked);

            SetupAdSpeedButtonHandlers();
        }

        private void SetupAdSpeedButtonHandlers()
        {
            _doubleSpeedButton.Button.onClick.AddListener(() =>
            {
                if (!_gameTimeService.IsDoubleSpeedUnlocked)
                    ShowAdForDoubleSpeed();
            });

            _quadrupleSpeedButton.Button.onClick.AddListener(() =>
            {
                if (_gameTimeService.IsDoubleSpeedUnlocked && !_gameTimeService.IsQuadrupleSpeedUnlocked)
                    ShowAdForQuadrupleSpeed();
            });
        }

        public void UpdateVisualState()
        {
            _normalSpeedButton.SetUnlocked(true);
            _doubleSpeedButton.SetUnlocked(_gameTimeService.IsDoubleSpeedUnlocked);
            _quadrupleSpeedButton.SetUnlocked(_gameTimeService.IsQuadrupleSpeedUnlocked);
        }

        private void ShowAdForDoubleSpeed()
        {
            _advertisementService.ShowReward(
                onRewarded: _ =>
                {
                    _gameTimeService.UnlockDoubleSpeed();
                    _gameTimeService.SetSpeed(GameTimeService.SpeedMultiplier.DOUBLE);
                    _doubleSpeedButton.SetUnlocked(true);
                    _quadrupleSpeedButton.gameObject.SetActive(true);

#if DEBUG
                    Debug.Log("[GameSpeedController] Double speed unlocked");
#endif
                }
            );
        }

        private void ShowAdForQuadrupleSpeed()
        {
            _advertisementService.ShowReward(
                onRewarded: _ =>
                {
                    _gameTimeService.UnlockQuadrupleSpeed();
                    _gameTimeService.SetSpeed(GameTimeService.SpeedMultiplier.QUADRUPLE);
                    _quadrupleSpeedButton.SetUnlocked(true);

#if DEBUG
                    Debug.Log("[GameSpeedController] Quadruple speed unlocked");
#endif
                }
            );
        }

        private void OnDestroy()
        {
            _doubleSpeedButton.Button.onClick.RemoveAllListeners();
            _quadrupleSpeedButton.Button.onClick.RemoveAllListeners();
        }
    }
}