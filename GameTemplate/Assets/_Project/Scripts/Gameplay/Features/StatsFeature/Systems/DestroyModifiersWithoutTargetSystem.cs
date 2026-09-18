using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DestroyModifiersWithoutTargetSystem : ISystem
    {
        public World World { get; set; }

        private Filter _modifiers;

        public void OnAwake()
        {
            _modifiers = World.Filter
                .With<StatModifierTag>()
                .With<TargetId>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity modifier in _modifiers)
            {
                ref readonly TargetId target = ref modifier.GetComponent<TargetId>();
                
                if (target.Value.IsNullOrDisposed())
                    World.RemoveEntity(modifier);
            }
        }

        public void Dispose()
        {
        }
    }
}