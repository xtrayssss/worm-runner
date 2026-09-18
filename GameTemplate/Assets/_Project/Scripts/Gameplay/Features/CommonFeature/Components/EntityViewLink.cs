using System;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct EntityViewLink : IComponent
    {
        [ReadOnly]
        public EntityView View;
        
        [ReadOnly]
        public EntityView Prefab;
    }
}