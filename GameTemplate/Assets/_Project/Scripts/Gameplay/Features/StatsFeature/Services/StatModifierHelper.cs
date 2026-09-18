using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Services
{
    public static class StatModifierHelper
    {
        private static readonly HashSet<StatId> INVERSE_BENEFICIAL_STATS = new HashSet<StatId>
        {
            StatId.COOLDOWN,
            StatId.HITS_FOR_CRIT
        };

        public static float CalculateModifierValue(StatId statId, float rawModifierValue, bool isPositiveEnhancement)
        {
            bool isInverseBeneficial = INVERSE_BENEFICIAL_STATS.Contains(statId);
           
            if (isInverseBeneficial)
                return isPositiveEnhancement ? -Mathf.Abs(rawModifierValue) : Mathf.Abs(rawModifierValue);
           
            return isPositiveEnhancement ? Mathf.Abs(rawModifierValue) : -Mathf.Abs(rawModifierValue);
        }
    }
}