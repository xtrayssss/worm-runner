using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UnapplyFreezeVisualsSystem : ISystem
    {
        public World World { get; set; }
        
        private Filter _unappliedStatuses;

        public void OnAwake()
        {
            _unappliedStatuses = World.Filter
                .With<FreezeStatusTag>()
                .With<StatusRemovedEvent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _unappliedStatuses)
            {
                ref readonly TargetId target = ref status.GetComponent<TargetId>();

                if (target.Value.Has<StatusVisualsReference>())
                {
                    ref readonly StatusVisualsReference statusVisuals =
                        ref target.Value.GetComponent<StatusVisualsReference>();

                    ref readonly StatusRemovedEvent statusRemovedEvent = ref status.GetComponent<StatusRemovedEvent>();
                    statusVisuals.Value.UnapplyFreeze(statusRemovedEvent.RemainingStacks);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
