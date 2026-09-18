using _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ApplyBaseHealthBonusFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _entities;

        public void OnAwake()
        {
            _entities = World.Filter
                .With<Stats>()
                .With<OwnerBaseHealthBonus>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _entities)
            {
                ref OwnerBaseHealthBonus ownerBaseHealthBonus = ref entity.GetComponent<OwnerBaseHealthBonus>();
                ownerBaseHealthBonus.Value = GetBaseHealthBonusValue(entity);
            }
        }

        private float GetBaseHealthBonusValue(Entity entity)
        {
            ref readonly Stats stats = ref entity.GetComponent<Stats>();
            return stats.Value[StatId.BASE_HEALTH_BONUS].CurrentValue;
        }

        public void Dispose()
        {
        }
    }
}