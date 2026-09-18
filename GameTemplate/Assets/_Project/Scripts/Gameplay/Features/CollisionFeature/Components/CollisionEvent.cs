using System;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CollisionFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct CollisionEvent : IEventData
    {
        public EntityView Other;
        public EntityView Source;
        public Vector3 Point;
        public Vector3 Normal;
    }
}