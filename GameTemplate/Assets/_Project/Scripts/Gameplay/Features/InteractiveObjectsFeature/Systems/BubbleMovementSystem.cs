using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using Cysharp.Threading.Tasks;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BubbleMovementSystem : ISystem
    {
        public World World { get; set; }

        private Filter _bubbles;
        private Filter _crowds;
        private readonly GameStateMachine _gameStateMachine;
        private readonly LevelService _levelService;

        public BubbleMovementSystem(
            GameStateMachine gameStateMachine,
            LevelService levelService)
        {
            _gameStateMachine = gameStateMachine;
            _levelService = levelService;
        }

        public void OnAwake()
        {
            _bubbles = World.Filter
                .With<BubbleTag>()
                .With<BubbleMovement>()
                .With<EntityViewLink>()
                .Without<BubbleActivatedMarker>()
                .Build();

            _crowds = World.Filter
                .With<CrowdTag>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState ||
                _levelService.CurrentLevel.LevelMode != LevelMode.DEFENSE)
                return;

            if (_crowds.IsEmpty())
                return;

            Vector3 crowdPosition = _crowds.First().GetEntityPosition();

            foreach (Entity bubble in _bubbles)
            {
                ref BubbleMovement movement = ref bubble.GetComponent<BubbleMovement>();
                ref readonly EntityViewLink viewLink = ref bubble.GetComponent<EntityViewLink>();
                ref readonly Health health = ref bubble.GetComponent<Health>();

                if (bubble.Has<BubbleSlowdown>())
                {
                    ref BubbleSlowdown slowdown = ref bubble.GetComponent<BubbleSlowdown>();
                    slowdown.SlowdownTimer -= deltaTime;

                    if (slowdown.SlowdownTimer <= 0f)
                    {
                        movement.CurrentMoveSpeed = movement.OriginalMoveSpeed;
                        bubble.RemoveComponent<BubbleSlowdown>();
                    }
                }
                
                if (health.CurrentHealth <= 0f && !movement.IsJumping)
                {
                    movement.IsJumping = true;
                    JumpToCrowdAsync(bubble, crowdPosition).Forget();
                }
                else if (!movement.IsJumping)
                {
                    Vector3 direction = Vector3.back;
                    viewLink.View.transform.position += direction * (movement.CurrentMoveSpeed * deltaTime);
                }
            }
        }

        private async UniTaskVoid JumpToCrowdAsync(Entity bubble, Vector3 targetPosition)
        {
            EntityViewLink viewLink = bubble.GetComponent<EntityViewLink>();
            BubbleView bubbleView = (BubbleView)viewLink.View;

            await bubbleView.PlayJumpToCrowdAsync(targetPosition);
            
            if (!bubble.IsNullOrDisposed())
                bubble.AddComponent<BubbleActivatedMarker>();
        }
        
        public void Dispose()
        {
        }
    }
}
