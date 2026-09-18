using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GateMovementSystem : ISystem
    {
        public World World { get; set; }

        private Filter _movingGates;
        private Filter _crowds;
        private readonly GameStateMachine _gameStateMachine;
        private readonly LevelService _levelService;

        public GateMovementSystem(
            GameStateMachine gameStateMachine,
            LevelService levelService)
        {
            _gameStateMachine = gameStateMachine;
            _levelService = levelService;
        }

        public void OnAwake()
        {
            _movingGates = World.Filter
                .With<GateTag>()
                .With<GateMovement>()
                .With<EntityViewLink>()
                .Without<GateDestroyMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState ||
                _levelService.CurrentLevel.LevelMode != LevelMode.DEFENSE)
                return;

            foreach (Entity gate in _movingGates)
            {
                ref GateMovement movement = ref gate.GetComponent<GateMovement>();

                ref readonly EntityViewLink viewLink = ref gate.GetComponent<EntityViewLink>();

                Vector3 direction = Vector3.back;
                viewLink.View.transform.position += direction * (movement.MoveSpeed * deltaTime);
            }
        }

        public void Dispose()
        {
        }
    }
}