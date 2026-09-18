using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BubbleSlowdownSystem : ISystem
    {
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvent;

        private const float SLOWDOWN_DURATION = 0.5f;
        private const float SLOWDOWN_MULTIPLIER = 0.3f;

        public void OnAwake()
        {
            _damagedEvent = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvent.publishedChanges)
            {
                Entity bubble = damagedEvent.Damageable;

                if (bubble.IsNullOrDisposed() ||
                    !bubble.Has<BubbleTag>() ||
                    !bubble.Has<BubbleMovement>())
                    continue;

                ref BubbleMovement movement = ref bubble.GetComponent<BubbleMovement>();

                if (bubble.Has<BubbleSlowdown>())
                {
                    ref BubbleSlowdown slowdown = ref bubble.GetComponent<BubbleSlowdown>();
                    slowdown.SlowdownTimer = SLOWDOWN_DURATION;
                }
                else
                {
                    bubble.AddComponent<BubbleSlowdown>() = new BubbleSlowdown
                    {
                        SlowdownTimer = SLOWDOWN_DURATION
                    };

                    movement.CurrentMoveSpeed *= SLOWDOWN_MULTIPLIER;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}