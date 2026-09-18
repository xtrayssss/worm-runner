using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;

namespace _Project.Scripts.Gameplay.Features.StatsFeature
{
    [Serializable]
    public class Stat
    {
        public float BaseValue;
        public float CurrentValue;
        public float MinValue;
        public float MaxValue;
        public List<StatModifier> ActiveCurrentModifiers = new List<StatModifier>();
        public List<StatModifier> ActiveBaseModifiers = new List<StatModifier>();
        public float CachedModifiedBaseValue;
        public bool UseBaseValueAsMaximum;
    }
}