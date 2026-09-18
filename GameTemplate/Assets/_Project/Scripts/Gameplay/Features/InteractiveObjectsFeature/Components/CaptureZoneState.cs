using System;
using System.Collections.Generic;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components
{
    [Serializable]
    public struct CaptureZoneState : IComponent
    {
        public int RequiredMembers;
        public List<Entity> CapturedMembers;
    }
}