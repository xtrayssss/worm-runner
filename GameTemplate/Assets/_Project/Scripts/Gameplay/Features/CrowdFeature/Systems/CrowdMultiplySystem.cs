using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdMultiplySystem : ISystem
    {
        public World World { get; set; }

        private Request<MultiplyCrowdRequest> _multiplyCrowdRequest;
        private Request<AddCrowdMembersRequest> _addMembersRequest;
        private Request<RemoveCrowdMembersRequest> _removeMembersRequest;
        private Filter _crowds;

        public void OnAwake()
        {
            _multiplyCrowdRequest = World.GetRequest<MultiplyCrowdRequest>();
            _addMembersRequest = World.GetRequest<AddCrowdMembersRequest>();
            _removeMembersRequest = World.GetRequest<RemoveCrowdMembersRequest>();
            _crowds = World.Filter
                .With<CrowdTag>()
                .With<CrowdPopulation>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (MultiplyCrowdRequest request in _multiplyCrowdRequest.Consume())
            {
                Entity crowd = _crowds.First();

                ref readonly CrowdPopulation crowdPopulation = ref crowd.GetComponent<CrowdPopulation>();

                int targetPopulation = Mathf.RoundToInt(crowdPopulation.MilitaryPopulation * request.Ratio);
                int delta = targetPopulation - crowdPopulation.MilitaryPopulation;

                if (delta > 0)
                {
                    _addMembersRequest.Publish(new AddCrowdMembersRequest
                    {
                        Count = delta,
                        AnimationType = request.AnimationType
                    });
                }
                else if (delta < 0)
                {
                    _removeMembersRequest.Publish(new RemoveCrowdMembersRequest
                    {
                        Count = -delta
                    });
                }
            }
        }

        public void Dispose()
        {
        }
    }
}