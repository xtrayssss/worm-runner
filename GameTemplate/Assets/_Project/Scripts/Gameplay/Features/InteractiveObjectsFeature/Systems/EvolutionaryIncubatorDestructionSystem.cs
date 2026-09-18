using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EvolutionaryIncubatorDestructionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _destroyedIncubators;

        public void OnAwake()
        {
            _destroyedIncubators = World.Filter
                .With<EvolutionaryIncubatorTag>()
                .With<EvolutionaryIncubatorDestroyMarker>()
                .With<EntityViewLink>()
                .With<ColliderLink>()
                .Without<EvolutionaryIncubatorDestroyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity incubator in _destroyedIncubators)
            {
                ref readonly EntityViewLink viewLink = ref incubator.GetComponent<EntityViewLink>();
                ref ColliderLink colliderLink = ref incubator.GetComponent<ColliderLink>();

                colliderLink.Value.enabled = false;

                incubator.AddComponent<EvolutionaryIncubatorDestroyingMarker>();

                EvolutionaryIncubatorView incubatorView = (EvolutionaryIncubatorView)viewLink.View;
                incubatorView
                    .PlayDestructionAnimation()
                    .OnComplete(
                        incubatorView,
                        static view =>
                        {
                            Object.Destroy(view.gameObject);

                            if (!view.Entity.IsNullOrDisposed())
                                World.Default.RemoveEntity(view.Entity);
                        });
            }
        }

        public void Dispose()
        {
        }
    }
}