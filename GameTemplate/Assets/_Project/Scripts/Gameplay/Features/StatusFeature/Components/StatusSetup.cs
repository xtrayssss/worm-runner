using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public record StatusSetup
    {
        public StatusTypeId StatusTypeId;
        public float Period;
        public float Duration;
        public bool UseStacking;
        public int MaxStacks;
        public bool AutoApply = true;

        [SerializeReference] [HideReferenceObjectPicker] [InlineProperty] [HideLabel]
        public StatModifierSetup ModifierSetup;
    }
}