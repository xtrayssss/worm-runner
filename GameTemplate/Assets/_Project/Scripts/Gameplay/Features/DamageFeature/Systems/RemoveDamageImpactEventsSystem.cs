using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RemoveDamageImpactEventsSystem : ISystem
    {
        public World World { get; set; }

        private Filter _impactors;

        public void OnAwake()
        {
            _impactors = World.Filter
                .With<DamageImpactEvent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity impactor in _impactors)
                impactor.RemoveComponent<DamageImpactEvent>();
        }

        public void Dispose()
        {
        }
    }
}