using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.LocalizationFeature
{
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField]
        private LocalizedString _localizedString;

        [SerializeField]
        private bool _upperCase;

        private Text _textComponent;
        private TextMeshProUGUI _tmpTextComponent;
        private LocalizationService _localizationService;
        private bool _initialized;

        private void Awake()
        {
            _textComponent = GetComponent<Text>();
            _tmpTextComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            InitializeIfNeeded();
        }

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
            UpdateText().Forget();
        }

        private void OnLocalizationInitialized()
        {
            _localizationService.OnLocalizationInitialized -= OnLocalizationInitialized;

            UpdateText().Forget();
            CompleteInitialization();
        }

        private void CompleteInitialization()
        {
            _localizationService.OnLocaleChanged += OnLocaleChanged;
            _initialized = true;
        }

        private void OnLocaleChanged(Locale locale) =>
            UpdateText().Forget();

        [Button]
        private async UniTaskVoid UpdateText()
        {
            string localizedText = await _localizedString.GetLocalizedStringAsync().Task;

            if (string.IsNullOrEmpty(localizedText))
                return;

            if (_upperCase)
                localizedText = localizedText.ToUpper();

            if (_textComponent != null)
                _textComponent.text = localizedText;

            if (_tmpTextComponent != null)
                _tmpTextComponent.text = localizedText;
        }
    }
}