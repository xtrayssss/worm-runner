using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.DeathFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public class DestroyHitVFXSystem : ISystem
    {
        private Filter _effects;
        public World World { get; set; }

        public void OnAwake()
        {
            _effects = World.Filter
                .With<HitVFXTag>()
                .With<DestructedMarker>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity effect in _effects)
            {
                ref EntityViewLink entityViewLink = ref effect.GetComponent<EntityViewLink>();
                
                Object.Destroy(entityViewLink.View.gameObject);
                
                World.RemoveEntity(effect);
            }
        }

        public void Dispose()
        {
            
        }
    }
}