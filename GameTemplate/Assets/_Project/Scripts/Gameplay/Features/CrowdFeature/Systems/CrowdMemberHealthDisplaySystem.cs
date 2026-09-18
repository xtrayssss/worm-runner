using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.LifeForceFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdMemberHealthDisplaySystem : ISystem
    {
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvent;

        public void OnAwake()
        {
            _damagedEvent = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvent.publishedChanges)
            {
                Entity damageable = damagedEvent.Damageable;

                if (damageable.IsNullOrDisposed() ||
                    !damageable.Has<MilitaryCrowdMemberTag>() ||
                    !damageable.Has<HealthDisplayLink>() ||
                    damageable.Has<ZeroHealthMarker>())
                    continue;

                ref readonly HealthDisplayLink displayLink = ref damageable.GetComponent<HealthDisplayLink>();

                if (!displayLink.Value.gameObject.activeInHierarchy)
                    displayLink.Value.gameObject.SetActive(true);
            }
        }

        public void Dispose()
        {
        }
    }
}