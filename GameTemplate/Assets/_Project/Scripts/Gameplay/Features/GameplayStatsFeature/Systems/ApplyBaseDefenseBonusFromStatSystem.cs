using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ApplyBaseDefenseBonusFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _entities;

        public void OnAwake()
        {
            _entities = World.Filter
                .With<Stats>()
                .With<OwnerBaseDefenseBonus>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _entities)
            {
                ref OwnerBaseDefenseBonus ownerBaseDefenseBonus = ref entity.GetComponent<OwnerBaseDefenseBonus>();
                ownerBaseDefenseBonus.Value = GetBaseDefenseBonusValue(entity);
            }
        }

        private float GetBaseDefenseBonusValue(Entity entity)
        {
            ref readonly Stats stats = ref entity.GetComponent<Stats>();
            return stats.Value[StatId.BASE_DEFENSE_BONUS].CurrentValue;
        }

        public void Dispose()
        {
        }
    }

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct OwnerBaseDefenseBonus : IComponent
    {
        public float Value;
    }
}