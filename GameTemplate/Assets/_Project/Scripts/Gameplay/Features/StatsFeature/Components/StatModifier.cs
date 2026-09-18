using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct StatModifier
    {
        public float Value;
        public StatId TargetStatId;
        public bool AffectsBaseValue;
        public StatOperation Operation;
        public int Priority;
        public int Id;
        public Entity ModifierEntity;
    }
}