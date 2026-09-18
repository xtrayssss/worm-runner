using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct MineState : IComponent
    {
        public float ArmingTimer;
        public float ExplosionRadius;
        public bool IsArmingStarted;

#if UNITY_EDITOR
        public bool ShowDetectionRadius;
#endif
    }
}