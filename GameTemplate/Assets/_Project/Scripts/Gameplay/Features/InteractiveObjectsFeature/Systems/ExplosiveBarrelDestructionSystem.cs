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
    public sealed class ExplosiveBarrelDestructionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _explodedBarrels;

        public void OnAwake()
        {
            _explodedBarrels = World.Filter
                .With<ExplosiveBarrelTag>()
                .With<ExplosiveBarrelDestroyMarker>()
                .With<EntityViewLink>()
                .Without<ExplosiveBarrelDestroyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity barrel in _explodedBarrels)
            {
                ref readonly EntityViewLink viewLink = ref barrel.GetComponent<EntityViewLink>();
                ref ColliderLink colliderLink = ref barrel.GetComponent<ColliderLink>();

                colliderLink.Value.enabled = false;
                barrel.AddComponent<ExplosiveBarrelDestroyingMarker>();

                ExplosiveBarrelView barrelView = (ExplosiveBarrelView)viewLink.View;

                barrelView
                    .PlayDestructionAnimation()
                    .OnComplete(
                        barrelView,
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