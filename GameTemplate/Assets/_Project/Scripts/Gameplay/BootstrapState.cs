using _Project.Scripts.Gameplay.Extensions;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.InputFeature.Systems;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Gameplay
{
    public sealed class BootstrapState : State<GameStateMachine.Context>
    {
        [ShowInInspector]
        private FeatureTree _featureTree;

        private readonly GameStateMachine _gameStateMachine;
        private readonly SaveLoadService _saveLoadService;
        private readonly AudioService _audioService;
        private readonly UIRoot _uiRoot;
        private readonly WindowService _windowsService;
        private readonly World _world;
        private readonly AllServices _services;
        private readonly ProjectileFactory _projectileFactory;
        private readonly InputService _inputService;
        private readonly LevelService _levelService;

        public BootstrapState(AllServices services)
        {
            _gameStateMachine = services.Get<GameStateMachine>();
            _saveLoadService = services.Get<SaveLoadService>();
            _audioService = services.Get<AudioService>();
            _uiRoot = services.Get<UIRoot>();
            _windowsService = services.Get<WindowService>();
            _services = services;
            _world = World.Default;
            _projectileFactory = services.Get<ProjectileFactory>();
            _inputService = services.Get<InputService>();
            _levelService = services.Get<LevelService>();
        }

        public override async UniTask Enter(GameStateMachine.Context ctx)
        {
            _featureTree = _world
                .CreateFeatureTree()
                .AddFeature(new GameFeature(_services))
                .Build(0, _world);

            _world.UpdateByUnity = false;

            GameWindow gameWindow = _windowsService.OpenPersistentWindow<GameWindow>(WindowId.HUD, _uiRoot.transform);
            gameWindow.Construct(AllServices.Instance);

            _uiRoot.Construct(gameWindow);

            _inputService.Initialize();

            _audioService.SetSoundVolume(_saveLoadService.PlayerSaveData.SfxVolume);

            await _levelService.ReloadCurrentLevel(withFadeAnimation: false);

            _gameStateMachine.Fire(GameplayEvent.ENTER_MENU);

            _projectileFactory.InitializePool();

            _uiRoot.GameWindow.ShowAsync().Forget();
        }
    }
}