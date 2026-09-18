using System.Text;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.LifeForceFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HealthDisplaySystem : ISystem
    {
        public World World { get; set; }

        private Filter _entitiesWithDisplay;

        public void OnAwake()
        {
            _entitiesWithDisplay = World.Filter
                .With<Health>()
                .With<HealthDisplayLink>()
                .With<HealthDisplaySettings>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _entitiesWithDisplay)
            {
                ref readonly HealthDisplayLink displayLink = ref entity.GetComponent<HealthDisplayLink>();
                ref readonly HealthDisplaySettings settings = ref entity.GetComponent<HealthDisplaySettings>();

                bool shouldUpdate = settings.UpdateMode switch
                {
                    StatDisplayUpdateMode.ALWAYS => true,
                    _ => false
                };

                if (shouldUpdate)
                {
                    ref readonly Health health = ref entity.GetComponent<Health>();
                    displayLink.Value.UpdateDisplay(health.CurrentHealth, health.MaxHealth);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}