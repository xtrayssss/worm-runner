using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GateEnhancementSystem : ISystem
    {
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvents;

        public void OnAwake()
        {
            _damagedEvents = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvents.publishedChanges)
            {
                Entity target = damagedEvent.Damageable;

                if (target.IsNullOrDisposed() ||
                    !target.Has<GateTag>() ||
                    !target.Has<IsEnhanceableMarker>() ||
                    target.Has<GateDestroyMarker>())
                    continue;

                ref GateState gateState = ref target.GetComponent<GateState>();

                const float ENHANCEMENT_VALUE = 0.5f;

                gateState.EffectValue += ENHANCEMENT_VALUE;
            }
        }

        public void Dispose()
        {
        }
    }
}