using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using _Project.Scripts.Gameplay.Features.AlertFeature;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.ConfettiFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Systems;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using _Project.Scripts.Gameplay.Features.InputFeature.Systems;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.LoadingScreenFeature;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Services;
using _Project.Scripts.Gameplay.Features.ToastNotificationFeature.Services;
using _Project.Scripts.Gameplay.Features.TutorialFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
#if GAMEPUSH_ENABLED
using GamePush;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class Bootstrapper : SerializedMonoBehaviour, ICoroutineRunner
    {
        [Header("Initialization Settings")]
        [SerializeField]
        private LoadingCurtain _loadingCurtain;

        [SerializeField]
        private UIRoot _uiRootPrefab;

        [SerializeField]
        private TutorialService _tutorialServicePrefab;

#if DEBUG
        [SerializeField]
        private bool _resetSavesOnStart = true;

#endif

#if UNITY_EDITOR
        [SerializeField]
        private bool _skipAds = true;
#endif

        [ShowInInspector]
        private AllServices _serviceLocator;

        private GameStateMachine _gameStateMachine;
        private bool _isFullyInitialized;
        private SaveLoadService _saveLoadService;

        private InputService _inputService;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            L.Log("[Bootstrapper] Starting initialization process");
        }

        private void Start()
        {
            _loadingCurtain.gameObject.SetActive(true);

            LaunchApplicationAsync().Forget();
        }

        private async UniTaskVoid LaunchApplicationAsync()
        {
            _serviceLocator = AllServices.Instance;

            // // Localization
            // LocalizationService localizationService = new LocalizationService();
            // _serviceLocator.Register(localizationService);
            //
            // await localizationService.InitializeAsync();

            await _loadingCurtain.ShowAsync();

#if GAMEPUSH_ENABLED
            await InitializeGamePush();
#endif
            InitializeCoreServices();

#if DEBUG
            if (_resetSavesOnStart)
                _saveLoadService.ResetSavesToDefault();
#endif

            await InitializeGameServices();
            InitializeGameStateMachine();

            _isFullyInitialized = true;

            L.Log("[Bootstrapper] Initialization completed successfully");

            _gameStateMachine.Enter<BootstrapState>();
        }

#if GAMEPUSH_ENABLED
        private async UniTask InitializeGamePush()
        {
            L.Log("[Bootstrapper] Initializing GamePush");

            if (GP_Init.isReady)
                return;

            await GP_Init.Ready;
        }
#endif

        private void InitializeCoreServices()
        {
            L.Log("[Bootstrapper] Initializing core services");

            _serviceLocator.Register(_loadingCurtain);

            // UIRoot
            UIRoot uiRoot = Instantiate(_uiRootPrefab);
            DontDestroyOnLoad(uiRoot.gameObject);
            _serviceLocator.Register(uiRoot);

            // Configs
            ConfigsService configs = new ConfigsService();
            configs.Initialize();
            _serviceLocator.Register(configs);

            // SceneLoader
            SceneLoader sceneLoader = new SceneLoader();
            _serviceLocator.Register(sceneLoader);

            // StatusFactory
            StatusFactory statusFactory = new StatusFactory();
            _serviceLocator.Register(statusFactory);

            // StatusApplier
            StatusApplier statusApplier = new StatusApplier();
            _serviceLocator.Register(statusApplier);

            // SaveLoad
            ISaveLoadProvider saveLoadProvider = null;
#if UNITY_EDITOR || (!UNITY_EDITOR && !GAMEPUSH_ENABLED)
            saveLoadProvider = new MockSaveLoadProvider();
#elif GAMEPUSH_ENABLED && !UNITY_EDITOR
            saveLoadProvider = new GamePushSaveLoadProvider();
#endif
            _saveLoadService = new SaveLoadService(saveLoadProvider);
            _serviceLocator.Register(_saveLoadService);
            _saveLoadService.Initialize();

            // Localization
            //await localizationService.InitializeAsync();

            L.Log("[Bootstrapper] Core services initialized");
        }

        private async UniTask InitializeGameServices()
        {
            L.Log("[Bootstrapper] Initializing game services");

            ConfigsService configs = _serviceLocator.Get<ConfigsService>();
            UIRoot uiRoot = _serviceLocator.Get<UIRoot>();

            // Targeting Service
            TargetingService targetingService = new TargetingService();
            _serviceLocator.Register(targetingService);

            // Stats Service
            StatsService statsService = new StatsService(configs);
            _serviceLocator.Register(statsService);

            // Currency Animation Service
            CurrencyFlyAnimationService currencyFlyAnimationService = new CurrencyFlyAnimationService();
            _serviceLocator.Register(currencyFlyAnimationService);

            // Audio Service
            AudioService audioService = Instantiate(configs.GetGameAudioPrefab());
            _serviceLocator.Register(audioService);
            audioService.Initialize(_saveLoadService);

            // VFX Service
            GameObject vfxContainer = new GameObject("VFXContainer");
            DontDestroyOnLoad(vfxContainer);
            VFXService vfxService = new VFXService(audioService, vfxContainer.transform);
            _serviceLocator.Register(vfxService);

            // Confetti Service
            ConfettiService confettiService = new ConfettiService(configs.GetConfettiConfigs(), audioService);
            _serviceLocator.Register(confettiService);

            // Game Time Service
            GameTimeService gameTimeService = new GameTimeService();
            _serviceLocator.Register(gameTimeService);

            // Advertisement Service
            IAdvertisementProvider advertisementProvider = null;

#if UNITY_EDITOR || (!UNITY_EDITOR && !GAMEPUSH_ENABLED)
            advertisementProvider = new MockAdvertisementProvider();
#elif GAMEPUSH_ENABLED && !UNITY_EDITOR
            advertisementProvider = new GamePushAdvertisementProvider();
#endif
            AdvertisementService advertisementService =
                new AdvertisementService(audioService, gameTimeService, advertisementProvider);
#if UNITY_EDITOR
            advertisementService.SkipAds = _skipAds;
#endif
            _serviceLocator.Register(advertisementService);

            // Alert Service
            AlertService alertService = new AlertService();
            _serviceLocator.Register(alertService);

            // Currency Service
            CurrencyService currencyService =
                new CurrencyService(configs, currencyFlyAnimationService, _saveLoadService);
            _serviceLocator.Register(currencyService);
            currencyService.Initialize();

            // Reward Service
            RewardService rewardService = new RewardService(configs, currencyService);
            _serviceLocator.Register(rewardService);

            // Window Factory
            WindowFactory windowFactory = new WindowFactory(configs.GetWindowPrefabs());
            _serviceLocator.Register(windowFactory);

            // Window Service
            WindowService windowService = new WindowService(windowFactory);
            _serviceLocator.Register(windowService);

            // Input Service
            InputService inputService = new InputService();
            _serviceLocator.Register(inputService);
            _inputService = inputService;

            // Toast Service
            ToastService toastService = new ToastService(windowService, audioService);
            _serviceLocator.Register(toastService);

            // Projectile Factory
            ProjectileFactory projectileFactory = new ProjectileFactory(configs, vfxService);
            _serviceLocator.Register(projectileFactory);

            // Crowd Evolution System
            CrowdEvolutionSystem crowdEvolutionSystem = new CrowdEvolutionSystem(configs, statsService);
            _serviceLocator.Register(crowdEvolutionSystem);

            // Run Statistics Service
            RunStatisticsService runStatisticsService = new RunStatisticsService();
            _serviceLocator.Register(runStatisticsService);

            // Enhancement Service
            EnhancementService enhancementService = new EnhancementService(
                statsService,
                vfxService,
                configs,
                _saveLoadService);
            _serviceLocator.Register(enhancementService);

            // Crowd Circular Formation Service
            CrowdTriangleFormationService crowdTriangleFormationService = new CrowdTriangleFormationService();
            _serviceLocator.Register(crowdTriangleFormationService);

            // Crowd Factory
            CrowdFactory crowdFactory = new CrowdFactory(
                configs,
                crowdEvolutionSystem,
                enhancementService,
                vfxService,
                audioService);
            _serviceLocator.Register(crowdFactory);

            // Soul Factory
            SoulFactory soulFactory = new SoulFactory(configs);
            _serviceLocator.Register(soulFactory);

            // Gate Visual System
            GateVisualSystem gateVisualSystem = new GateVisualSystem(enhancementService);
            _serviceLocator.Register(gateVisualSystem);

            // Crowd Remove Members System
            CrowdRemoveMembersSystem crowdRemoveMembersSystem = new CrowdRemoveMembersSystem(
                runStatisticsService,
                soulFactory);
            _serviceLocator.Register(crowdRemoveMembersSystem);

            // Camera Service
            CameraService cameraService = new CameraService();
            cameraService.Initialize();
            _serviceLocator.Register(cameraService);

            // Interactive Object Factory
            InteractiveObjectFactory interactiveObjectFactory = new InteractiveObjectFactory(
                configs,
                vfxService,
                crowdFactory,
                gateVisualSystem,
                cameraService,
                audioService,
                uiRoot);
            _serviceLocator.Register(interactiveObjectFactory);

            // Collectible Tracking Service
            CollectibleService collectibleService = new CollectibleService();
            _serviceLocator.Register(collectibleService);

            // Capture Zone Service
            CaptureZoneService captureZoneService = new CaptureZoneService();
            _serviceLocator.Register(captureZoneService);

            // Level Service
            SceneLoader sceneLoader = _serviceLocator.Get<SceneLoader>();
            LevelService levelService = new LevelService(
                configs,
                interactiveObjectFactory,
                _saveLoadService,
                enhancementService,
                crowdFactory,
                collectibleService,
                captureZoneService,
                sceneLoader,
                _loadingCurtain,
                uiRoot);
            levelService.Initialize();
            _serviceLocator.Register(levelService);

            // Mortar Strike Factory
            MortarStrikeFactory mortarStrikeFactory = new MortarStrikeFactory(
                configs,
                vfxService,
                cameraService,
                audioService);
            _serviceLocator.Register(mortarStrikeFactory);

            TutorialService tutorialService = null;

            // if (!_saveLoadService.PlayerSaveData.TutorialProgress.TutorialCompleted)
            // {
            //     tutorialService = Instantiate(_tutorialServicePrefab, transform);
            //     _serviceLocator.Register(tutorialService);
            //     tutorialService.Initialize(windowService, _saveLoadService);
            // }

            await UniTask.CompletedTask;

            L.Log("[Bootstrapper] Game services initialized");
        }

        private void InitializeGameStateMachine()
        {
            L.Log("[Bootstrapper] Initializing state machine");

            StateTransitionHandler transitionHandler = new StateTransitionHandler(this);
            _serviceLocator.Register(transitionHandler);

            RegisterGameStates();

            _gameStateMachine = GameStateMachine.Configure(_serviceLocator, new GameStateMachine.Context());

            L.Log("[Bootstrapper] State machine initialized and entering menu state");
        }

        private void RegisterGameStates()
        {
        }

        private void Update()
        {
            if (_isFullyInitialized)
            {
                _gameStateMachine?.Update();
                _saveLoadService.Update();
            }
        }

        private void FixedUpdate()
        {
            if (_isFullyInitialized)
            {
                _gameStateMachine?.FixedUpdate();
            }
        }

        private void LateUpdate()
        {
            if (_isFullyInitialized)
            {
                _gameStateMachine?.LateUpdate();
            }
        }

        private void OnDestroy()
        {
            if (_gameStateMachine != null)
            {
                _gameStateMachine.Dispose();
                _inputService.Dispose();
                _gameStateMachine = null;
            }
        }
    }
}