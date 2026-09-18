using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct CrowdPopulation : IComponent
    {
        public int MaxMilitaryPopulation;
       
        public int CivilianPopulation;        
        public List<Entity> CivilianMembers;
        
        public int MilitaryPopulation;
        public List<Entity> MilitaryMembers;
    }
}