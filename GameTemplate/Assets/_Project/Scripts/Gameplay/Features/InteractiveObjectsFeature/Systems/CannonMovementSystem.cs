using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CannonMovementSystem : IFixedSystem
    {
        public World World { get; set; }

        private Filter _cannons;

        public void OnAwake()
        {
            _cannons = World.Filter
                .With<CannonTag>()
                .With<CannonMovementSettings>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity cannon in _cannons)
            {
                ref CannonMovementSettings movementSettings = ref cannon.GetComponent<CannonMovementSettings>();
                ref EntityViewLink viewLink = ref cannon.GetComponent<EntityViewLink>();

                float newX = movementSettings.InitialPosition.x + movementSettings.Amplitude *
                    Mathf.Sin(Time.time * movementSettings.Speed + movementSettings.Phase);

                Vector3 newPosition = viewLink.View.transform.position;
                newPosition.x = newX;
                viewLink.View.transform.position = newPosition;    
            }
        }

        public void Dispose()
        {
        }
    }
}