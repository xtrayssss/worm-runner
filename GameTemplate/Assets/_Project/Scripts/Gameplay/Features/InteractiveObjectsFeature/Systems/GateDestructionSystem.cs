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
    public sealed class GateDestructionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _destroyedGates;

        public void OnAwake()
        {
            _destroyedGates = World.Filter
                .With<GateTag>()
                .With<GateDestroyMarker>()
                .With<EntityViewLink>()
                .With<ColliderLink>()
                .Without<GateDestroyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity gate in _destroyedGates)
            {
                ref readonly EntityViewLink viewLink = ref gate.GetComponent<EntityViewLink>();

                ref ColliderLink colliderLink = ref gate.GetComponent<ColliderLink>();
                colliderLink.Value.enabled = false;

                gate.AddComponent<GateDestroyingMarker>();

                GateView gateView = (GateView)viewLink.View;
                gateView
                    .PlayDestructionAnimation()
                    .OnComplete(
                        gateView,
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