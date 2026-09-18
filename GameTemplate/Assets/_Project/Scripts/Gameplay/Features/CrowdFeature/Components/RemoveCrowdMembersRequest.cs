using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct RemoveCrowdMembersRequest : IRequestData
    {
        public int Count;
        public Entity[] SpecificMembers;
        public Entity SpecificMember;
        public CrowdMemberType MemberType;
        public bool SuppressSoulEffect;
    }
}