using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct  MovementState : IComponent
    {
        public Vector3 HorizontalMovement;
        public Vector3 ForwardMovement;
        public Vector3 HorizontalVelocity;
        public Vector3 ForwardVelocity;
    }
}