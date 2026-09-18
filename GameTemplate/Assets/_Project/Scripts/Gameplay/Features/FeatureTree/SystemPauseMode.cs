using System;

namespace _Project.Scripts.Gameplay.Features.FeatureTree
{
    [Flags]
    public enum SystemPauseMode : byte
    {
        NONE = 0,
        DEFAULT = 1,
        ALWAYS_PAUSABLE = 2,
        NEVER_PAUSABLE = 4
    }
}