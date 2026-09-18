using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using _Project.Scripts.Gameplay.Features.AlertFeature;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using Scellecs.Morpeh;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameWindow : ModularWindow
    {
        [SerializeField]
        private CurrencyLabel _moneyLabel;

        [SerializeField]
        private LevelLabel _levelLabel;

        [SerializeField]
        private LevelObjectiveLabel _objectiveLabel;

        [SerializeField]
        private DistanceProgressBar _distanceProgressBar;

        [SerializeField]
        private MenuContent _menuContent;

        [SerializeField]
        private Image _fog;

        [SerializeField]
        private ParticleSystem _fogParticle;

        public Image Fog => _fog;
        public MenuContent MenuContent => _menuContent;
        private CurrencyService _currencyService;
        private LevelService _levelService;
        private CurrencyLabel MoneyLabel => _moneyLabel;
        public DistanceProgressBar DistanceProgressBar => _distanceProgressBar;
        public LevelObjectiveLabel ObjectiveLabel => _objectiveLabel;
        public ParticleSystem FogParticle => _fogParticle;

        public void Construct(AllServices services)
        {
            _currencyService = services.Get<CurrencyService>();
            _levelService = services.Get<LevelService>();

            _menuContent.Construct(services);
            MoneyLabel.Construct(_currencyService, CurrencyType.MONEY);
            _levelLabel.Construct(_levelService);

            CaptureZoneService captureZoneService = services.Get<CaptureZoneService>();
            CollectibleService collectibleService = services.Get<CollectibleService>();
            _objectiveLabel.Construct(_levelService, captureZoneService, collectibleService);

            _distanceProgressBar.Construct(_levelService);
        }

        public override UniTask ShowAsync()
        {
            base.ShowAsync();
            EnableInteraction();
            return UniTask.CompletedTask;
        }

        public override UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            CancellationToken linkedToken = CreateLinkedCancellationToken(externCancellationToken);
            Canvas.enabled = false;
            SetBlockRaycasts(false);

            base.CloseAsync(linkedToken);
            return UniTask.CompletedTask;
        }
    }

    [Serializable]
    public sealed class MenuContent
    {
        [SerializeField]
        private TapToStartButton _tapToStartButton;

        [SerializeField]
        private UpgradeCardsPanel _upgradeCardsPanel;

        [SerializeField]
        private Button _settingsButton;

        [FormerlySerializedAs("_crowdMemberReward")]
        [SerializeField]
        private RewardedAdButton _crowdMemberRewardButton;

        private WindowService _windows;
        private GameStateMachine _gameStateMachine;
        private SaveLoadService _saveLoadService;
        private AudioService _audioService;
        private AlertService _alertService;
        private World _world;
        private EnhancementService _enhancementService;
        private VFXService _vfxService;
        private Filter _crowds;
        private GameTimeService _gameTimeService;

        private const int REWARD_CROWD_MEMBERS_COUNT = 3;

        public RewardedAdButton CrowdMemberRewardButton => _crowdMemberRewardButton;

        public void Construct(AllServices services)
        {
            _gameStateMachine = services.Get<GameStateMachine>();
            _windows = services.Get<WindowService>();
            _saveLoadService = services.Get<SaveLoadService>();
            _audioService = services.Get<AudioService>();
            _alertService = services.Get<AlertService>();
            _enhancementService = services.Get<EnhancementService>();
            _world = World.Default;
            _vfxService = services.Get<VFXService>();
            _gameTimeService = services.Get<GameTimeService>();
            _crowds = _world!.Filter
                .With<CrowdTag>()
                .With<EntityViewLink>()
                .Build();

            CurrencyService currencyService = services.Get<CurrencyService>();
            AdvertisementService advertisementService = services.Get<AdvertisementService>();
            ConfigsService configsService = services.Get<ConfigsService>();

            _upgradeCardsPanel.Construct(configsService, currencyService, _enhancementService);
            _crowdMemberRewardButton.Construct(advertisementService, hide: true);
            _tapToStartButton.Construct();

            _tapToStartButton.Button.onClick.AddListener(StartGameplay);
            _settingsButton.onClick.AddListener(OpenSettingsPopup);
            _crowdMemberRewardButton.OnRewardGranted += GrantCrowdMembersRewardButton;
        }

        private void OnDestroy()
        {
            _tapToStartButton.Button.onClick.RemoveListener(StartGameplay);
            _settingsButton.onClick.RemoveListener(OpenSettingsPopup);
            _crowdMemberRewardButton.OnRewardGranted -= GrantCrowdMembersRewardButton;
        }

        public void Show(LevelMode levelMode)
        {
            switch (levelMode)
            {
                case LevelMode.STEALTH:
                    _upgradeCardsPanel.gameObject.SetActive(false);
                    _crowdMemberRewardButton.gameObject.SetActive(false);
                    break;
                default:
                    _upgradeCardsPanel.gameObject.SetActive(true);
                    _upgradeCardsPanel.Refresh();
                    _crowdMemberRewardButton.gameObject.SetActive(true);
                    break;
            }

            _tapToStartButton.ButtonPulser.PlayPulse();
        }

        public void Hide() =>
            _tapToStartButton.ButtonPulser.StopPulse();

        private void StartGameplay() =>
            _gameStateMachine.Fire(GameplayEvent.ENTER_GAMEPLAY);

        private void OpenSettingsPopup()
        {
            SettingsPopup settingsPopup =
                _windows.OpenPersistentWindow<SettingsPopup>(WindowId.SETTINGS_POPUP);

            settingsPopup.Construct(
                _saveLoadService,
                _audioService,
                _alertService,
                _gameTimeService);

            settingsPopup.CloseButton.onClick.AddListener(() =>
                _windows.CloseWindow(WindowId.SETTINGS_POPUP).Forget());

            settingsPopup.ShowAsync();
        }

        private void GrantCrowdMembersRewardButton()
        {
            if (_crowds.IsEmpty())
                return;

            Request<AddCrowdMembersRequest> addMembersRequest = _world.GetRequest<AddCrowdMembersRequest>();
            EnhancementService.UpgradeData evolutionUpgradeData = _enhancementService.Upgrades[UpgradeType.EVOLUTION];

            addMembersRequest.Publish(new AddCrowdMembersRequest
            {
                Count = REWARD_CROWD_MEMBERS_COUNT,
                EvolutionLevel = (int)evolutionUpgradeData.CurrentValue,
                AnimationType = CrowdMemberAnimationType.IDLE
            }, allowNextFrame: true);

            VFXData upgradeVFX = _enhancementService.UpgradeVFX;

            Vector3 crowdPosition = _crowds.First().GetEntityPosition();

            _vfxService.PlayVFX(upgradeVFX, position: crowdPosition);
        }
    }
}