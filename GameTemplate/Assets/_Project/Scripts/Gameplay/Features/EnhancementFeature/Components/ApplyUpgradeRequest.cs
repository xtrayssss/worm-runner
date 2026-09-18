using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct ApplyUpgradeRequest : IRequestData
    {
        public UpgradeType UpgradeType;
        public float Value;
        public int IntValue;
        public Entity UpgradeSource;
        public int Cost;
    }
}