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
    public sealed class CollectibleDestructionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _collectedItems;

        public void OnAwake()
        {
            _collectedItems = World.Filter
                .With<CollectibleTag>()
                .With<CollectibleDestroyMarker>()
                .With<EntityViewLink>()
                .Without<CollectibleDestroyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity collectible in _collectedItems)
            {
                ref readonly EntityViewLink viewLink = ref collectible.GetComponent<EntityViewLink>();

                ref ColliderLink colliderLink = ref collectible.GetComponent<ColliderLink>();
                colliderLink.Value.enabled = false;

                CollectibleView collectibleView = (CollectibleView)viewLink.View;
                
                collectibleView
                    .PlayDestructionVFX(scale: 2.3f)
                    .PlayDestructionAnimation()
                    .OnComplete(collectibleView, static view =>
                    {
                        if (!view.Entity.IsNullOrDisposed()) 
                            World.Default.RemoveEntity(view.Entity);
                        
                        Object.Destroy(view.gameObject);
                    });
                
                collectible.AddComponent<CollectibleDestroyingMarker>();
            }
        }

        public void Dispose()
        {
        }
    }
}