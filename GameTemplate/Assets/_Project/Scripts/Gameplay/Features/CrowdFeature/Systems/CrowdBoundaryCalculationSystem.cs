using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdBoundaryCalculationSystem : ISystem
    {
        public World World { get; set; }

        private Filter _crowdMembers;
        private Filter _crowds;

        public void OnAwake()
        {
            _crowdMembers = World.Filter
                .With<CrowdMemberTag>()
                .Build();
            
            _crowds = World.Filter
                .With<CrowdTag>()
                .With<CrowdMovableBounds>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var crowd in _crowds)
            {
                ref EntityViewLink crowdLeadEntityViewLink = ref crowd.GetComponent<EntityViewLink>();

                Bounds bounds = new Bounds(crowdLeadEntityViewLink.View.transform.position, Vector3.zero);

                foreach (Entity member in _crowdMembers)
                {
                    ref EntityViewLink memberEntityViewLink = ref member.GetComponent<EntityViewLink>();
                    bounds.Encapsulate(memberEntityViewLink.View.transform.position);
                }

                ref CrowdMovableBounds movableBounds = ref crowd.GetComponent<CrowdMovableBounds>();
                movableBounds.Value = bounds;
            }
        }

        public void Dispose()
        {
        }
    }
}