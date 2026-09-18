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
    public sealed class ApplyHealthRegenFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statOwners;

        public void OnAwake()
        {
            _statOwners = World.Filter
                .With<Stats>()
                .With<HealthRegen>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _statOwners)
            {
                ref HealthRegen healthRegen = ref entity.GetComponent<HealthRegen>();
                healthRegen.Value = GetHealthRegen(entity);
            }
        }

        private float GetHealthRegen(Entity entity)
        {
            ref Stats stats = ref entity.GetComponent<Stats>();
            return stats.Value[StatId.HEALTH_REGEN].CurrentValue;
        }

        public void Dispose()
        {
        }
    }
}