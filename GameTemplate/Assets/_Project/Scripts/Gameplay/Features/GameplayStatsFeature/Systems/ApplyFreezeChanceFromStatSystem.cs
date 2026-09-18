using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ApplyFreezeChanceFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _entities;

        public void OnAwake()
        {
            _entities = World.Filter
                .With<Stats>()
                .With<FreezeStatusChance>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _entities)
            {
                ref FreezeStatusChance criticalHit = ref entity.GetComponent<FreezeStatusChance>();
                criticalHit.Chance = GetFreezeChance(entity);
            }
        }

        private float GetFreezeChance(Entity entity)
        {
            ref Stats stats = ref entity.GetComponent<Stats>();
            return stats.Value[StatId.FREEZE_CHANCE].CurrentValue;
        }

        public void Dispose()
        {
        }
    }
}