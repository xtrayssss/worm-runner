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
    public sealed class ApplyDamageFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statOwners;

        public void OnAwake()
        {
            _statOwners = World.Filter
                .With<Stats>()
                .With<Damage>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity statOwner in _statOwners)
            {
                ref Damage damage = ref statOwner.GetComponent<Damage>();
                damage.CurrentDamage = GetValue(statOwner);
            }
        }

        private float GetValue(Entity statOwner)
        {
            ref Stats stats = ref statOwner.GetComponent<Stats>();
            return stats.Value[StatId.DAMAGE].CurrentValue;
        }

        public void Dispose()
        {
        }
    }
}