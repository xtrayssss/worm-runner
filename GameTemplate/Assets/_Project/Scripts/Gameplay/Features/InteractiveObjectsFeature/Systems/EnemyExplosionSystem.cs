using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using Scellecs.Morpeh;
using Sirenix.Serialization;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    public sealed class EnemyExplosionSystem : ISystem
    {
        public World World { get; set; }

        private readonly LevelService _levelService;
        private readonly StatsService _statsService;
        private readonly GameStateMachine _gameStateMachine;
        private readonly TargetingService _targetingService;

        private Filter _kamikazeEnemies;
        private Filter _crowdMembers;

        private readonly List<Entity> _membersInRangeBuffer = new List<Entity>(capacity: 8);

        public EnemyExplosionSystem(
            LevelService levelService,
            StatsService statsService,
            GameStateMachine gameStateMachine,
            TargetingService targetingService)
        {
            _levelService = levelService;
            _statsService = statsService;
            _gameStateMachine = gameStateMachine;
            _targetingService = targetingService;
        }

        public void OnAwake()
        {
            _kamikazeEnemies = World.Filter
                .With<EnemyTag>()
                .With<KamikazeEnemy>()
                .With<EntityViewLink>()
                .Without<EnemyDestroyingMarker>()
                .Build();

            _crowdMembers = World.Filter
                .With<CrowdMemberTag>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState ||
                _levelService.CurrentLevel.LevelMode != LevelMode.DEFENSE)
                return;

            foreach (Entity kamikaze in _kamikazeEnemies)
            {
                ref readonly EntityViewLink viewLink = ref kamikaze.GetComponent<EntityViewLink>();
                ref readonly KamikazeEnemy enemyData = ref kamikaze.GetComponent<KamikazeEnemy>();

                _targetingService.GetEntitiesInRangeNonAlloc(
                    _crowdMembers,
                    sourcePosition: viewLink.View.transform.position + enemyData.ExplosionOffset,
                    range: enemyData.ExplosionRadius,
                    results: _membersInRangeBuffer);

                if (_membersInRangeBuffer.Count == 0)
                    continue;

                foreach (Entity member in _membersInRangeBuffer)
                {
                    _statsService.CreateEffectModifier(
                        target: member,
                        StatId.HEALTH,
                        StatOperation.ADD,
                        modifierValue: enemyData.ExplosionDamage,
                        producer: kamikaze,
                        position: viewLink.View.transform.position
                    );
                }

                DestroyEnemy(kamikaze, viewLink);
            }
        }

        private void DestroyEnemy(Entity kamikaze, EntityViewLink viewLink)
        {
            _statsService.CreateEffectModifier(
                target: kamikaze,
                StatId.HEALTH,
                StatOperation.ADD,
                modifierValue: -int.MaxValue,
                position: viewLink.View.transform.position
            );
        }

        public void Dispose()
        {
        }
    }
}