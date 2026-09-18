using System;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    [Flags]
    public enum GameState
    {
        NONE = 0,
        MENU = 1 << 0,
        PREPARATION = 1 << 1,
        GAMEPLAY = 1 << 2,
        PAUSE = 1 << 3,
        VICTORY = 1 << 4,
        DEFEAT = 1 << 5,

        PREPARATION_AND_GAMEPLAY = PREPARATION | GAMEPLAY,
        ALL_STATES = ~NONE
    }
}