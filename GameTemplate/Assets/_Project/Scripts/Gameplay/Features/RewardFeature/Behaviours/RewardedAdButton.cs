using System;
using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours
{
    public sealed class RewardedAdButton : MonoBehaviour
    {
        [SerializeField]
        [ShowIf("@_visualBehavior == VisualBehavior.VISIBILITY_ONLY")]
        private GameObject _buttonContent;

        [SerializeField]
        private VisualBehavior _visualBehavior = VisualBehavior.VISIBILITY_ONLY;

        [SerializeField]
        [ShowIf("@_visualBehavior == VisualBehavior.FULL_VISUAL_UPDATE")]
        private Image _buttonImage;

        [SerializeField]
        [ShowIf("@_visualBehavior == VisualBehavior.FULL_VISUAL_UPDATE")]
        private Sprite _activeSprite;

        [SerializeField]
        [ShowIf("@_visualBehavior == VisualBehavior.FULL_VISUAL_UPDATE")]
        private Sprite _usedSprite;

        [SerializeField]
        private GameObject _adIcon;

        [PropertySpace(3)]
        [SerializeField]
        [ReadOnly]
        private bool _isUsed;

        public bool IsVisible => !_buttonContent.activeInHierarchy;

        [PropertySpace(3)]
        [SerializeField] 
        [ReadOnly] 
        private RewardMode _currentMode = RewardMode.WITH_AD;

        private AdvertisementService _advertisementService;
        private Button _button;

        private enum VisualBehavior
        {
            VISIBILITY_ONLY = 0,
            FULL_VISUAL_UPDATE = 1
        }

        public enum RewardMode
        {
            WITH_AD = 0,
            WITHOUT_AD = 1
        }

        public event Action OnRewardGranted;

        public bool IsUsed => _isUsed;

        public void Construct(AdvertisementService advertisementService, bool hide = false)
        {
            _advertisementService = advertisementService;
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);

            if (!hide) 
                ResetState();
        }

        private void OnDestroy() =>
            _button.onClick.RemoveListener(OnButtonClicked);

        public void SetRewardMode(RewardMode mode)
        {
            _currentMode = mode;
            UpdateAdIconVisibility();

#if DEBUG
            Debug.Log($"[RewardedAdButton] Mode changed to: {mode}");
#endif
        }

        public void ResetState()
        {
            _isUsed = false;
            _currentMode = RewardMode.WITH_AD;
            UpdateButtonVisibility();
        }

        private void OnButtonClicked()
        {
            if (_isUsed)
                return;

            switch (_currentMode)
            {
                case RewardMode.WITHOUT_AD:
                    GrantRewardDirectly();
                    break;

                case RewardMode.WITH_AD:
                    ShowRewardedAd();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GrantRewardDirectly()
        {
            OnRewardGranted?.Invoke();
            _isUsed = true;
            UpdateButtonVisibility();

#if DEBUG
            Debug.Log("[RewardedAdButton] Reward granted directly (no ad)");
#endif
        }

        private void ShowRewardedAd()
        {
            _advertisementService.ShowReward(
                onRewarded: OnAdRewarded,
                onRewardedStart: OnAdStarted,
                onRewardedClose: OnAdClosed
            );
        }

        private void OnAdStarted()
        {
        }

        private void OnAdRewarded(string _) =>
            OnRewardGranted?.Invoke();

        private void OnAdClosed(bool success)
        {
            if (success)
                _isUsed = true;

            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        {
            switch (_visualBehavior)
            {
                case VisualBehavior.VISIBILITY_ONLY:
                    UpdateButtonContentVisibility();
                    UpdateAdIconVisibility();
                    break;
                case VisualBehavior.FULL_VISUAL_UPDATE:
                    UpdateInteractability();
                    UpdateSprite();
                    UpdateAdIconVisibility();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_visualBehavior), _visualBehavior, null);
            }
        }

        private void UpdateButtonContentVisibility() => 
            _buttonContent.SetActive(!_isUsed);

        private void UpdateInteractability() =>
            _button.interactable = !_isUsed;

        private void UpdateSprite() =>
            _buttonImage.sprite = _isUsed ? _usedSprite : _activeSprite;

        private void UpdateAdIconVisibility()
        {
            if (_adIcon == null)
                return;

            _adIcon.SetActive(!_isUsed && _currentMode == RewardMode.WITH_AD);
        }
    }
}