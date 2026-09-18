using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public record FreezeStatusSetup : StatusSetup
    {
        public FreezeStatusSetup()
        {
            StatusTypeId = StatusTypeId.FREEZE;
            Period = 0f;
            Duration = 3f;
            MaxStacks = 3;
            ModifierSetup = new SpeedModifierSetup
            {
                Operation = StatOperation.MULTIPLY,
                ModifierValue = -0.5f,
                Priority = (int)StatOperation.MULTIPLY,
                AffectsBaseValue = false,
                TargetStatId = StatId.SPEED,
                EffectType = StatEffectType.STATUS
            };
        }
    }
}