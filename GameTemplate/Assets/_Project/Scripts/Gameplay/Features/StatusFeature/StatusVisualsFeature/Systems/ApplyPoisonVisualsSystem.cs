using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ApplyPoisonVisualsSystem : ISystem
    {
        public World World { get; set; }

        private Filter _appliedStatuses;

        public void OnAwake()
        {
            _appliedStatuses = World.Filter
                .With<PoisonStatusTag>()
                .With<StatusAppliedEvent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _appliedStatuses)
            {
                ref readonly TargetId target = ref status.GetComponent<TargetId>();

                if (target.Value.Has<StatusVisualsReference>())
                {
                    ref readonly StatusVisualsReference statusVisuals =
                        ref target.Value.GetComponent<StatusVisualsReference>();

                    statusVisuals.Value.ApplyPoison();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}