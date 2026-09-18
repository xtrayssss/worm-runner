using _Project.Scripts.Gameplay.Features.LifeForceFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class LockHealthBarRotationSystem : ISystem
    {
        public World World { get; set; }

        private Filter _entities;

        public void OnAwake()
        {
            _entities = World.Filter
                .With<HealthDisplayLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _entities)
            {
                ref var healthBarRef = ref entity.GetComponent<HealthDisplayLink>();
                healthBarRef.Value.transform.rotation = Quaternion.identity;
            }
        }

        public void Dispose()
        {
        }
    }
}