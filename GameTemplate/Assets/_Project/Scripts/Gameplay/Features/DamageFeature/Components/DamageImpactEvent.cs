using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct DamageImpactEvent : IComponent
    {
        public int ImpactCount;
        public Entity FirstTarget;
        public Vector3 Point;
        public float TotalDamage;
        public HashSet<Entity> Targets;
        public bool IsDirectDamageDealer;
        public StatEffectType StatEffectType;
        public bool IsCritical;
    }
}