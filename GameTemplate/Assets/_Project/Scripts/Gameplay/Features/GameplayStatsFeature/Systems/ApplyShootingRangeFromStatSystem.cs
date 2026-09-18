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
    public sealed class ApplyShootingRangeFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statOwners;

        public void OnAwake()
        {
            _statOwners = World.Filter
                .With<Stats>()
                .With<ShootingRange>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity statOwner in _statOwners)
            {
                ref ShootingRange shootingRange = ref statOwner.GetComponent<ShootingRange>();
                shootingRange.CurrentValue = GetValue(statOwner);
            }
        }

        private float GetValue(Entity statOwner)
        {
            ref Stats stats = ref statOwner.GetComponent<Stats>();
            return stats.Value[StatId.SHOOTING_RANGE].CurrentValue;
        }

        public void Dispose()
        {
        }
    }
}