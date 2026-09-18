using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Components;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using GamePush;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.GameTimeFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PauseSystem : ISystem
    {
        private readonly GameTimeService _gameTimeService;
        public World World { get; set; }

        private readonly List<FeatureTree.FeatureTree> _systemGroups = new List<FeatureTree.FeatureTree>();

        private Request<PauseRequest> _pauseRequest;
        private Request<UnpauseRequest> _unpauseRequest;

        public PauseSystem(GameTimeService gameTimeService, params FeatureTree.FeatureTree[] systemGroups)
        {
            _gameTimeService = gameTimeService;
            _systemGroups.AddRange(systemGroups);
        }

        public void OnAwake()
        {
            _pauseRequest = World.GetRequest<PauseRequest>();
            _unpauseRequest = World.GetRequest<UnpauseRequest>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (UnpauseRequest _ in _unpauseRequest.Consume())
            {
                Time.timeScale = (float)_gameTimeService.CurrentSpeed;

                foreach (FeatureTree.FeatureTree systemGroup in _systemGroups) 
                    systemGroup.Unpause();
                
#if GAMEPUSH_ENABLED
                GP_Game.GameplayStart();
#endif


#if DEBUG
                Debug.Log($"Game unpaused. Time scale restored to: {_gameTimeService.CurrentSpeed}");
#endif
            }

            foreach (PauseRequest _ in _pauseRequest.Consume())
            {
                Time.timeScale = 0;

                foreach (FeatureTree.FeatureTree systemGroup in _systemGroups) 
                    systemGroup.Pause();
                
#if GAMEPUSH_ENABLED
                GP_Game.GameplayStop();
#endif                    

#if DEBUG
                Debug.Log($"Game paused. Previous time scale was: {_gameTimeService.CurrentSpeed}");
#endif
            }
        }

        public void Dispose()
        {
        }
    }
}