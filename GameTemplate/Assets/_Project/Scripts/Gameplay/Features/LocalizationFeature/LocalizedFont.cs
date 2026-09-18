using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Scripts.Gameplay.Features.LocalizationFeature
{
    public sealed class LocalizedFont : MonoBehaviour
    {
        [SerializeField] private LocalizedAsset<TMP_FontAsset> _localizedFont;

        private TextMeshProUGUI _tmpTextComponent;
        private LocalizationService _localizationService;
        private bool _initialized;

        private void Awake()
        {
            _tmpTextComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable() => 
            InitializeIfNeeded();

        private void OnDisable() => 
            _localizationService.OnLocaleChanged -= OnLocaleChanged;

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
            UpdateFont().Forget();
        }

        private void OnLocalizationInitialized()
        {
            _localizationService.OnLocalizationInitialized -= OnLocalizationInitialized;

            UpdateFont().Forget();
            CompleteInitialization();
        }

        private void CompleteInitialization()
        {
            _localizationService.OnLocaleChanged += OnLocaleChanged;
            _initialized = true;
        }

        private void OnLocaleChanged(Locale locale) =>
            UpdateFont().Forget();

        [Button]
        private async UniTaskVoid UpdateFont()
        {
            TMP_FontAsset fontAsset = await _localizedFont.LoadAssetAsync().Task;

            if (fontAsset == null)
                return;

            if (_tmpTextComponent != null)
                _tmpTextComponent.font = fontAsset;
        }
    }
}