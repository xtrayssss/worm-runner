using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Components
{
    // Настройки гравитации

    // События изменения популяции толпы

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct CrowdMemberRemovedEvent : IEventData
    {
        public Entity CrowdEntity;
        public Entity RemovedMember;
    }
}