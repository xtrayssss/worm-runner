using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.AlertFeature;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using GamePush;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay
{
    public sealed class SettingsPopup : ModularWindow
    {
        [Serializable]
        public sealed class SettingsToggle
        {
            [field: SerializeField]
            public Toggle Toggle { get; private set; }

            [field: SerializeField]
            public SmoothSliderToggle SmoothSliderToggle { get; private set; }

            [field: SerializeField]
            public Sprite ToggleEnabledSprite { get; private set; }

            [field: SerializeField]
            public Sprite ToggleDisabledSprite { get; private set; }

            [field: SerializeField]
            public Image Icon { get; private set; }

            [field: SerializeField]
            public TextMeshProUGUI StateText { get; private set; }
        }

        [FormerlySerializedAs("_pushAlarmToggle")]
        [Header("Toggles")]
        [SerializeField] private SettingsToggle _alertsToggle;

        [Header("Sound")]
        [SerializeField] private Sprite _soundOnSprite;

        [SerializeField]
        private Sprite _soundOffSprite;

        [SerializeField]
        private Sprite _soundMediumSprite;

        [SerializeField]
        private Slider _soundSlider;

        [SerializeField]
        private Image _soundIcon;

        [Header("Music")]
        [SerializeField] private Sprite _musicOnSprite;

        [SerializeField]
        private Sprite _musicOffSprite;

        [SerializeField]
        private Sprite _musicMediumSprite;

        [SerializeField]
        private Slider _musicSlider;

        [SerializeField]
        private Image _musicIcon;

        [field: SerializeField] public Button CloseButton { get; private set; }

        private bool _alertsEnabled;

        private UnityAction<bool> _alertsToggleAction;
        private AudioService _audioService;

        private AlertService _alertService;
        private SaveLoadService _saveLoadService;

        private SliderPointerHandler _soundSliderHandler;
        private SliderPointerHandler _musicSliderHandler;
        private GameTimeService _gameTimeService;

        private bool AlertsEnabled
        {
            get => _alertsEnabled;
            set
            {
                _alertsEnabled = value;
                //_saveLoadService.PlayerSaveData.AreAlertsEnabled = value;
            }
        }

        public void Construct(
            SaveLoadService saveLoadService,
            AudioService audioService,
            AlertService alertService, 
            GameTimeService gameTimeService)
        {
            _gameTimeService = gameTimeService;
            _audioService = audioService;
            _saveLoadService = saveLoadService;
            _alertService = alertService;

            //AlertsEnabled = _saveLoadService.PlayerSaveData.AreAlertsEnabled;

            _alertsToggle.Toggle.SetIsOnWithoutNotify(AlertsEnabled);

            _alertsToggle.SmoothSliderToggle.Animate(AlertsEnabled, useAnimation: false);
            // _alertsToggle.StateText.text = AlertsEnabled
            //     ? I18N.StringTable_UI_Settings.SETTINGS_TOGGLE_ON
            //     : I18N.StringTable_UI_Settings.SETTINGS_TOGGLE_OFF;

            //UpdateAlertsIcon(AlertsEnabled);

            _soundSlider.SetValueWithoutNotify(_saveLoadService.PlayerSaveData.SfxVolume);
            UpdateSoundIcon(_soundSlider.value);

            // _musicSlider.SetValueWithoutNotify(_saveLoadService.PlayerSaveData.MusicVolume);
            // UpdateMusicIcon(_musicSlider.value);

            SetupSliderPointerHandlers();

            // _alertsToggleAction = value =>
            // {
            //     AlertsEnabled = value;
            //     _alertsToggle.SmoothSliderToggle.Animate(AlertsEnabled);
            //     UpdateAlertsIcon(value);
            //     _audioService.PlaySound(AudioId.Sfx.UI.TOGGLE_SWITCH);
            //
            //     if (value)
            //         _alertService.EnableAlerts();
            //     else
            //         _alertService.DisableAlerts();
            //
            //     // _alertsToggle.StateText.text = value
            //     //     ? I18N.StringTable_UI_Settings.SETTINGS_TOGGLE_ON
            //     //     : I18N.StringTable_UI_Settings.SETTINGS_TOGGLE_OFF;
            //
            //     _saveLoadService.SaveData(syncImmediately: false);
            // };

            _alertsToggle.Toggle.onValueChanged.AddListener(_alertsToggleAction);

            _soundSlider.onValueChanged.AddListener(UpdateSoundSlider);
            //_musicSlider.onValueChanged.AddListener(UpdateMusicSlider);

            _soundSliderHandler.OnSliderReleased += OnSoundSliderReleased;
            //_musicSliderHandler.OnSliderReleased += OnMusicSliderReleased;
        }

        private void OnDestroy()
        {
            //_alertsToggle.Toggle.onValueChanged.RemoveListener(_alertsToggleAction);

            _soundSlider.onValueChanged.RemoveListener(UpdateSoundSlider);
            //_musicSlider.onValueChanged.RemoveListener(UpdateMusicSlider);

            _soundSliderHandler.OnSliderReleased -= OnSoundSliderReleased;
            //_musicSliderHandler.OnSliderReleased -= OnMusicSliderReleased;
        }

        private void SetupSliderPointerHandlers()
        {
            if (!_soundSlider.TryGetComponent<SliderPointerHandler>(out _))
                _soundSliderHandler = _soundSlider.gameObject.AddComponent<SliderPointerHandler>();

            // if (!_musicSlider.TryGetComponent<SliderPointerHandler>(out _))
            //     _musicSliderHandler = _musicSlider.gameObject.AddComponent<SliderPointerHandler>();
        }

        // private void UpdateAlertsIcon(bool value) =>
        //     _alertsToggle.Icon.sprite =
        //         value ? _alertsToggle.ToggleEnabledSprite : _alertsToggle.ToggleDisabledSprite;

        public override UniTask ShowAsync()
        {
            _gameTimeService.Pause();
            
            base.ShowAsync();

            WindowTweener
                .ShowShrinkFadeWindow(
                    ContentRectTransform,
                    useUnscaledTime: true,
                    canvasGroup: SelfCanvasGroup)
                .OnComplete(EnableInteraction);

            return UniTask.CompletedTask;
        }

        public override async UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            CancellationToken linkedToken = CreateLinkedCancellationToken(externCancellationToken);

            base.CloseAsync(linkedToken);

            _saveLoadService.SaveData();

            await WindowTweener
                .HideShrinkFadeWindow(
                    ContentRectTransform,
                    useUnscaledTime: true,
                    canvasGroup: SelfCanvasGroup)
                .OnComplete(
                    this,
                    static popup =>
                    {
                        popup.EnableInteraction();
                        Destroy(popup.gameObject);
                    })
                .AsTask(linkedToken);
            
            _gameTimeService.Unpause();
        }

        private void UpdateSoundSlider(float value)
        {
            _audioService.SetSoundVolume(value);
            UpdateSoundIcon(value);
            _saveLoadService.PlayerSaveData.SfxVolume = value;
        }

        private void OnSoundSliderReleased() =>
            _saveLoadService.SaveData();

        private void UpdateSoundIcon(float value)
        {
            float percentage = value / _soundSlider.maxValue;

            _soundIcon.sprite = percentage switch
            {
                <= 0 => _soundOffSprite,
                < 0.5f => _soundMediumSprite,
                _ => _soundOnSprite
            };
        }

        // private void UpdateMusicSlider(float value)
        // {
        //     _audioService.SetMusicVolume(value);
        //     UpdateMusicIcon(value);
        //
        //     _saveLoadService.PlayerSaveData.MusicVolume = value;
        // }

        // private void OnMusicSliderReleased() =>
        //     _saveLoadService.SaveData(syncImmediately: false);

        // private void UpdateMusicIcon(float value)
        // {
        //     float percentage = value / _musicSlider.maxValue;
        //
        //     _musicIcon.sprite = percentage switch
        //     {
        //         <= 0 => _musicOffSprite,
        //         < 0.5f => _musicMediumSprite,
        //         _ => _musicOnSprite
        //     };
        // }
    }
}