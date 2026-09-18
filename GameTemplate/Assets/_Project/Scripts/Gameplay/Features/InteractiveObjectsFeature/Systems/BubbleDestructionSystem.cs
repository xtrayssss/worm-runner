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
    public sealed class BubbleDestructionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _destroyedBubbles;

        public void OnAwake()
        {
            _destroyedBubbles = World.Filter
                .With<BubbleTag>()
                .With<BubbleDestroyMarker>()
                .With<EntityViewLink>()
                .Without<BubbleDestroyingMaker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity bubble in _destroyedBubbles)
            {
                ref readonly EntityViewLink viewLink = ref bubble.GetComponent<EntityViewLink>();

                BubbleView bubbleView = (BubbleView)viewLink.View;

                bubbleView
                    .PlayDestructionAnimation()
                    .OnComplete(bubbleView, static view =>
                    {
                        Object.Destroy(view.gameObject);

                        if (!view.Entity.IsNullOrDisposed())
                            World.Default.RemoveEntity(view.Entity);
                    });

                bubble.AddComponent<BubbleDestroyingMaker>();
            }
        }

        public void Dispose()
        {
        }
    }
}