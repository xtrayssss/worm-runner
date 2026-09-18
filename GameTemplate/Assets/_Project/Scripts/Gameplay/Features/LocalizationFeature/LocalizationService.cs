using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Cysharp.Threading.Tasks;
using GamePush;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project.Scripts.Gameplay.Features.LocalizationFeature
{
    public sealed class LocalizationService : IService
    {
        public event Action<Locale> OnLocaleChanged;
        public event Action OnLocalizationInitialized;

        [ShowInInspector]
        private LocalizationSettings _localizationSettings;

        [ShowInInspector]
        private Locale _currentLocale;

        [ShowInInspector]
        private readonly Dictionary<LocaleIdentifier, Locale> _availableLocales =
            new Dictionary<LocaleIdentifier, Locale>();

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync()
        {
            if (IsInitialized)
                return;

            AsyncOperationHandle<LocalizationSettings> initOperation = LocalizationSettings.InitializationOperation;

#if UNITY_EDITOR
            _localizationSettings = initOperation.WaitForCompletion();
#else
    _localizationSettings = await initOperation.Task;
#endif
            if (_localizationSettings == null)
            {
#if DEBUG
                Debug.LogError("[LocalizationService] Initialization failed: LocalizationSettings is null");
#endif
                return;
            }

            List<Locale> locales = _localizationSettings.GetAvailableLocales().Locales;

            foreach (Locale locale in locales)
            {
                _availableLocales.Add(locale.Identifier, locale);
#if DEBUG
                Debug.Log($"[LocalizationService] Added locale: {locale.Identifier} ({locale.LocaleName})");
#endif
            }

            IsInitialized = true;

            await SetLanguageFromGamePush();

            OnLocalizationInitialized?.Invoke();
        }

        private async UniTask SetLanguageFromGamePush()
        {
            string language = GetInitialLanguageCode();

            await ChangeLanguage(language);
        }

        private string GetInitialLanguageCode()
        {
#if GAMEPUSH_ENABLED
            return GP_Language.CurrentISO();
#else
            string systemLanguage = Application.systemLanguage.ToString();
            string languageCode = MapSystemLanguageToLocaleCode();
            return languageCode;

            string MapSystemLanguageToLocaleCode()
            {
                return systemLanguage switch
                {
                    "English" => "en",
                    "Russian" => "ru",
                    "Spanish" => "es",
                    "French" => "fr",
                    "German" => "de",
                    _ => throw new ArgumentOutOfRangeException(nameof(systemLanguage), systemLanguage, null)
                };
            }
#endif
        }

        [Button]
        public async UniTask ChangeLanguage(string localeCode)
        {
            Locale locale = _availableLocales.GetValueOrDefault(localeCode);

            _localizationSettings.SetSelectedLocale(locale);
            _currentLocale = await _localizationSettings.GetSelectedLocaleAsync().Task;

            await LocalizationSettings.InitializationOperation;

            OnLocaleChanged?.Invoke(_currentLocale);
#if DEBUG
            Debug.Log($"[LocalizationService] Locale changed to: {_currentLocale.LocaleName}");
#endif
        }

        public async UniTask<string> GetLocalizedStringAsync(string tableName, string key)
        {
            LocalizedStringDatabase stringDatabase = _localizationSettings.GetStringDatabase();
            AsyncOperationHandle<string> localizedStringOperation =
                stringDatabase.GetLocalizedStringAsync(tableName, key, _currentLocale);

            string localizedString = await localizedStringOperation.Task;
            return string.IsNullOrEmpty(localizedString) ? key : localizedString;
        }

        public async UniTask<string> GetLocalizedStringAsync(string tableName, string key, params object[] args)
        {
            AsyncOperationHandle<string> localizedStringOperation = _localizationSettings
                .GetStringDatabase()
                .GetLocalizedStringAsync(tableName, key, args);

            string localizedString = await localizedStringOperation.Task;

            return string.IsNullOrEmpty(localizedString) ? key : localizedString;
        }

        public string GetLocalizedString(string tableName, string key, params object[] args) =>
            _localizationSettings
                .GetStringDatabase()
                .GetLocalizedString(tableName, key, args);

        public List<Locale> GetAvailableLocales() =>
            new List<Locale>(_availableLocales.Values);

        public Locale GetCurrentLocale() =>
            _currentLocale;
    }
}