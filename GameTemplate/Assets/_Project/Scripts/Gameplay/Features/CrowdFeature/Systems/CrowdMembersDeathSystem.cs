using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdMembersDeathSystem : ISystem
    {
        public World World { get; set; }

        private Request<RemoveCrowdMembersRequest> _removeMembersRequest;
        private Event<DamagedEvent> _damagedEvents;
        
        public void OnAwake()
        {
            _removeMembersRequest = World.GetRequest<RemoveCrowdMembersRequest>();
            _damagedEvents = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvents.publishedChanges)
            {
                if (damagedEvent.Damageable.IsNullOrDisposed() ||
                    !damagedEvent.Damageable.Has<CrowdMemberTag>() ||
                    !damagedEvent.Damageable.Has<ZeroHealthMarker>() ||
                    damagedEvent.Damageable.Has<CrowdMemberDyingMarker>())
                    continue;

                Entity member = damagedEvent.Damageable;

                member.AddComponent<CrowdMemberDyingMarker>();

                ref readonly RigidbodyLink rigidbodyLink = ref member.GetComponent<RigidbodyLink>();
                rigidbodyLink.Value.velocity = Vector3.zero;

                ref ColliderLink colliderLink = ref member.GetComponent<ColliderLink>();
                colliderLink.Value.enabled = false;

                ref EntityViewLink viewLink = ref member.GetComponent<EntityViewLink>();
                viewLink.View.transform.parent = null;

                _removeMembersRequest.Publish(new RemoveCrowdMembersRequest
                {
                    SpecificMember = member,  
                    Count = 1
                }, allowNextFrame: true);
            }
        }

        public void Dispose()
        {
        }
    }
}