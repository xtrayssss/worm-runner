using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ApplyCooldownFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _entities;

        public void OnAwake()
        {
            _entities = World.Filter
                .With<Stats>()
                .With<AttackCooldown>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _entities)
            {
                ref AttackCooldown cooldown = ref entity.GetComponent<AttackCooldown>();
                cooldown.Cooldown = GetCooldownValue(entity);
            }
        }

        private float GetCooldownValue(Entity entity)
        {
            ref Stats stats = ref entity.GetComponent<Stats>();
            return stats.Value[StatId.COOLDOWN].CurrentValue;
        }

        public void Dispose()
        {
        }
    }
}