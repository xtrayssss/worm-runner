using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct ContactDamage : IComponent
    {
        public float Damage;
        public StatOperation Operation;
    }
}