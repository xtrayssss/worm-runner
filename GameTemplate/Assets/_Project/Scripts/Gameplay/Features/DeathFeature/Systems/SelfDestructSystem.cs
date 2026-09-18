using _Project.Scripts.Gameplay.Features.DeathFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.DeathFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SelfDestructSystem : ISystem
    {
        public World World { get; set; }

        private Filter _destructibles;

        public void OnAwake()
        {
            _destructibles = World.Filter
                .With<SelfDestructTimer>()
                .Without<DestructedMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity destructible in _destructibles)
            {
                ref SelfDestructTimer timer = ref destructible.GetComponent<SelfDestructTimer>();

                timer.Time -= deltaTime;

                if (timer.Time <= 0)
                {
                    destructible.AddComponent<DestructedMarker>();

#if DEBUG
                    Debug.Log($"Entity {destructible.GetNiceName()} marked for destruction due to timer expiration");
#endif
                }
            }
        }

        public void Dispose()
        {
        }
    }
}