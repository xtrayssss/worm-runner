using System;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Components
{
    [Serializable]
    public enum DamageSourceType
    {
        UNKNOWN = 0,
        PROJECTILE = 1,
        CROWD_MEMBER = 2,
        MINE = 3,
        CANNON = 4,
    }
}