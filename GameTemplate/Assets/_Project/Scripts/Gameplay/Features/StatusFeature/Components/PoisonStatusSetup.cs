using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public record PoisonStatusSetup : StatusSetup
    {
        public PoisonStatusSetup()
        {
            StatusTypeId = StatusTypeId.POISON;
            Period = 1f;
            Duration = 3f;
            MaxStacks = 3;
            ModifierSetup = new SimpleStatModifierSetup
            {
                Operation = StatOperation.ADD,
                ModifierValue = -1f,
                Priority = (int)StatOperation.ADD,
                AffectsBaseValue = false,
                TargetStatId = StatId.HEALTH,
                EffectType = StatEffectType.STATUS
            };
        }
    }
}