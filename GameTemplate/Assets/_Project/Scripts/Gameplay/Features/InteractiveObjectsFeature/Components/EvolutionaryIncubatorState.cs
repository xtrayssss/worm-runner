using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct EvolutionaryIncubatorState : IComponent
    {
        public int TierLevel;
        public int SpawnCount;
        public float AccumulatedDamage;
        public float[] DamageThresholdsPerLevel;
    }
}