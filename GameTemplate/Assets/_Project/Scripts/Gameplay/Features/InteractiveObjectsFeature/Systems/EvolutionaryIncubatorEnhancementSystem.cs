using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EvolutionaryIncubatorEnhancementSystem : ISystem
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
                    !target.Has<EvolutionaryIncubatorTag>() ||
                    !target.Has<IsEnhanceableMarker>() ||
                    target.Has<EvolutionaryIncubatorDestroyMarker>())
                    continue;

                ref EvolutionaryIncubatorState incubatorState = ref target.GetComponent<EvolutionaryIncubatorState>();

                float damageAmount = damagedEvent.DamageAmount;
                incubatorState.AccumulatedDamage += damageAmount;

                CheckEvolutionLevelUp(target, ref incubatorState);
            }
        }

        private void CheckEvolutionLevelUp(Entity incubator, ref EvolutionaryIncubatorState incubatorState)
        {
            int maxEvolutionLevel = incubatorState.DamageThresholdsPerLevel.Length;
            
            if (incubatorState.TierLevel >= maxEvolutionLevel - 1)
                return;

            float thresholdForNextLevel = incubatorState.DamageThresholdsPerLevel[incubatorState.TierLevel];

            if (incubatorState.AccumulatedDamage >= thresholdForNextLevel)
            {
                incubatorState.TierLevel++;
                incubatorState.AccumulatedDamage = 0f;

                World.GetEvent<EvolutionLevelUpEvent>().NextFrame(new EvolutionLevelUpEvent
                {
                    EvolutionaryIncubator = incubator,
                });
            }
        }

        public void Dispose()
        {
        }
    }
}