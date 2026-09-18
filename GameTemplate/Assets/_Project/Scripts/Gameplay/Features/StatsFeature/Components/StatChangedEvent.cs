using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct StatChangedEvent : IComponent
    {
        public StatId StatId;
        public Entity Target;
        public Entity Producer;
        public float OldValue;
        public float NewValue;
        public Entity Modifier;
        public StatChangeSource Source;
        public bool SuppressDamageEvent;
    }
}