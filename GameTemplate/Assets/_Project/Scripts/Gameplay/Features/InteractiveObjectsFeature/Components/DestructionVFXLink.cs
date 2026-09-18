using System;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct DestructionVFXLink : IComponent
    {
        public VFXData Value;
    }
}