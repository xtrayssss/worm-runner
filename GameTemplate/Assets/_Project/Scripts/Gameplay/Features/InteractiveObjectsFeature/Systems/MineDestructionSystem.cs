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
    public sealed class MineDestructionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _explodedMines;

        public void OnAwake()
        {
            _explodedMines = World.Filter
                .With<MineTag>()
                .With<MineExplodedMarker>()
                .With<EntityViewLink>()
                .Without<MineDestroyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity mine in _explodedMines)
            {
                ref readonly EntityViewLink viewLink = ref mine.GetComponent<EntityViewLink>();
                
                mine.AddComponent<MineDestroyingMarker>();
                
                MineView mineView = (MineView)viewLink.View;
                mineView
                    .PlayDestructionAnimation()
                    .OnComplete(mineView, static view =>
                    {
                        Object.Destroy(view.gameObject);

                        if (view.Entity.IsNullOrDisposed())
                            return;

                        World.Default.RemoveEntity(view.Entity);
                    });
            }
        }

        public void Dispose()
        {
        }
    }
}