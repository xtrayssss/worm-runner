using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
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
    public sealed class EnemyMovementSystem : ISystem
    {
        public World World { get; set; }
        private Filter _movingEnemies;
        private Filter _crowds;

        private readonly LevelService _levelService;
        private readonly GameStateMachine _gameStateMachine;

        public EnemyMovementSystem(
            LevelService levelService,
            GameStateMachine gameStateMachine)
        {
            _levelService = levelService;
            _gameStateMachine = gameStateMachine;
        }

        public void OnAwake()
        {
            _movingEnemies = World.Filter
                .With<EnemyTag>()
                .With<EnemyMovement>()
                .With<EntityViewLink>()
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

            foreach (Entity enemy in _movingEnemies)
            {
                ref EnemyMovement movement = ref enemy.GetComponent<EnemyMovement>();
                ref readonly EntityViewLink enemyViewLink = ref enemy.GetComponent<EntityViewLink>();

                Vector3 direction = Vector3.back;

                if (enemy.Has<KamikazeEnemy>())
                {
                    direction = (crowdPosition - enemyViewLink.View.transform.position).normalized;
                    direction.y = 0;

                    if (!movement.RandomOffset.HasValue)
                    {
                        float randomOffset = (Random.value - 0.5f) * 0.2f;
                        movement.RandomOffset = randomOffset;
                    }

                    direction.x += movement.RandomOffset.Value;
                    direction.Normalize();
                }
                else if (enemy.Has<ShooterEnemy>())
                {
                    ref readonly ShooterEnemy shooterData = ref enemy.GetComponent<ShooterEnemy>();
                    Vector3 currentPosition = enemyViewLink.View.transform.position;
                    float distance = Vector3.Distance(currentPosition, crowdPosition);

                    if (distance <= shooterData.StopDistance)
                        continue;
                }

                float forwardMoveSpeed = movement.MoveSpeed;
                float horizontalMoveSpeed = movement.MoveSpeed * 2f;

                Vector3 horizontalMovementRaw = new Vector3(direction.x, 0f, 0f) * horizontalMoveSpeed;
                Vector3 forwardMovementRaw = new Vector3(0f, 0f, direction.z) * forwardMoveSpeed;

                enemyViewLink.View.transform.position += (horizontalMovementRaw + forwardMovementRaw) * deltaTime;
            }
        }

        public void Dispose()
        {
        }
    }
}