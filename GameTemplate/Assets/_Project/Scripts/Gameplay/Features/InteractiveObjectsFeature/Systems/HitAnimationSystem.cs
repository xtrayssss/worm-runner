using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HitAnimationSystem : ISystem
    {
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvents;

        public void OnAwake() =>
            _damagedEvents = World.GetEvent<DamagedEvent>();

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvents.publishedChanges)
            {
                if (damagedEvent.Damageable.IsNullOrDisposed() ||
                    !damagedEvent.Damageable.Has<InteractiveObjectTag>() ||
                    !damagedEvent.Damageable.Has<HitAnimationMarker>())
                    continue;

                Entity interactiveObject = damagedEvent.Damageable;

                ref readonly EntityViewLink viewLink = ref interactiveObject.GetComponent<EntityViewLink>();

                InteractiveObjectView view = (InteractiveObjectView)viewLink.View;

                view.PlayHitAnimation(); 
            }
        }

        public void Dispose()
        {
        }
    }
}