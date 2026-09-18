using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MortarStrikeSchedulerSystem : ISystem
    {
        public World World { get; set; }

        private readonly MortarStrikeFactory _mortarFactory;
        private readonly LevelService _levelService;

        public MortarStrikeSchedulerSystem(MortarStrikeFactory mortarFactory, LevelService levelService)
        {
            _mortarFactory = mortarFactory;
            _levelService = levelService;
        }

        public void OnAwake()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            LevelConfig currentLevel = _levelService.CurrentLevel;

            if (currentLevel == null)
                return;

            if (currentLevel.MortarStrikes.Length <= 0)
                return;

            for (int index = 0; index < currentLevel.MortarStrikes.Length; index++)
            {
                if (_levelService.StrikesTriggered[index])
                    continue;

                MortarStrikeData strikeData = currentLevel.MortarStrikes[index];

                if (_levelService.LevelTime < strikeData.TriggerTime)
                    continue;

                _levelService.StrikesTriggered[index] = true;

                Filter crowds = World.Filter.With<CrowdTag>().Build();
                Vector3 crowdPosition = crowds.First()
                    .GetEntityPosition();

                Vector3 spawnPosition = new Vector3(
                    crowdPosition.x + Random.Range(-RoadSegment.LANE_WIDTH * 3f, RoadSegment.LANE_WIDTH * 3f),
                    0.01f,
                    crowdPosition.z + 8f
                );

                _mortarFactory.CreateMortarStrike(spawnPosition);
            }
        }

        public void Dispose()
        {
        }
    }
}