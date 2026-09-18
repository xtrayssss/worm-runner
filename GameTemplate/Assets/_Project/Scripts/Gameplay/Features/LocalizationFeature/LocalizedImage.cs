using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.LocalizationFeature
{
    public sealed class LocalizedImage : MonoBehaviour
    {
        [SerializeField]
        private LocalizedAsset<Texture2D> _localizedSprite;

        private Image _imageComponent;
        private LocalizationService _localizationService;
        private bool _initialized;

        private void Awake()
        {
            _imageComponent = GetComponent<Image>();
        }

        private void OnEnable() =>
            InitializeIfNeeded();

        private void OnDisable()
        {
            if (_initialized)
            {
                _localizationService.OnLocaleChanged -= OnLocaleChanged;
            }
        }

        private void InitializeIfNeeded()
        {
            if (_initialized)
                return;

            _localizationService = AllServices.Instance.Get<LocalizationService>();

            if (!_localizationService.IsInitialized)
            {
                _localizationService.OnLocalizationInitialized += OnLocalizationInitialized;
                return;
            }

            CompleteInitialization();
            UpdateImage().Forget();
        }

        private void OnLocalizationInitialized()
        {
            _localizationService.OnLocalizationInitialized -= OnLocalizationInitialized;

            UpdateImage().Forget();
            CompleteInitialization();
        }

        private void CompleteInitialization()
        {
            _localizationService.OnLocaleChanged += OnLocaleChanged;
            _initialized = true;
        }

        private void OnLocaleChanged(Locale locale) =>
            UpdateImage().Forget();

        [Button]
        private async UniTaskVoid UpdateImage()
        {
            Texture2D texture = await _localizedSprite.LoadAssetAsync().Task;

            if (texture == null)
                return;

            if (_imageComponent != null)
                _imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
        }
    }
}