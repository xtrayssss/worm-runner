using System;
using System.Collections.Generic;
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
    public struct ActiveCollision : IComponent
    {
        public List<CollisionInfo> Collisions;
        public bool IsProcessed;

        [Serializable]
        public struct CollisionInfo
        {
            public EntityView Other;
            public Vector3 Point;
        }
    }
}