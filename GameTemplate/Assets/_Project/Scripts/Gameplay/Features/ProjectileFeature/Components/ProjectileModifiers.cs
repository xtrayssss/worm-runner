using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct ProjectileModifiers : IComponent
    {
        public StatModifierSetup[] OnFireModifiers;
        public StatModifierSetup[] OnHitModifiers;
        public StatModifierSetup[] OnSpawnModifiers;
        public StatModifierSetup[] OnDestroyModifiers;
    }
}