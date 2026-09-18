using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct HealedEvent : IEventData
    {
        public Entity Target;
        public float HealAmount;
        public ModifierData Modifier;
        public StatChangeSource HealthChangeSource;

        public struct ModifierData
        {
            public bool IsCritical;
            public StatEffectType StatEffectType;
            public Entity Producer;
            public Vector3 EffectPoint;
        }
    }
}