using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MineExplosionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _mines;
        private Filter _crowdMembers;
        private readonly TargetingService _targetingService;
        private Request<RemoveCrowdMembersRequest> _removeMembersRequest;

        public MineExplosionSystem(TargetingService targetingService) =>
            _targetingService = targetingService;

        public void OnAwake()
        {
            _mines = World.Filter
                .With<MineTag>()
                .With<MineState>()
                .Without<MineArmedMarker>()
                .Without<MineExplodedMarker>()
                .Build();

            _crowdMembers = World.Filter
                .With<MilitaryCrowdMemberTag>()
                .With<EntityViewLink>()
                .Build();

            _removeMembersRequest = World.GetRequest<RemoveCrowdMembersRequest>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity mine in _mines)
            {
                ref MineState mineState = ref mine.GetComponent<MineState>();

                if (!mineState.IsArmingStarted)
                {
                    mineState.IsArmingStarted =
                        _targetingService.HasTargetsInRange(
                            _crowdMembers,
                            mine.GetEntityPosition(),
                            mineState.ExplosionRadius);
                }

                if (mineState.IsArmingStarted && !mine.Has<MineArmedMarker>())
                {
                    mineState.ArmingTimer -= deltaTime;

                    if (mineState.ArmingTimer <= 0f)
                        ArmMine(mine);
                }

                if (mine.Has<MineArmedMarker>())
                    ExplodeMine(mine);
            }
        }

        private static void ArmMine(Entity mine)
        {
            mine.AddComponent<MineArmedMarker>();

            ref readonly EntityViewLink viewLink = ref mine.GetComponent<EntityViewLink>();
            MineView mineView = (MineView)viewLink.View;
            mineView.SetArmed(true);
        }

        private void ExplodeMine(Entity mine)
        {
            mine.AddComponent<MineExplodedMarker>();

            ref readonly MineState mineState = ref mine.GetComponent<MineState>();

            List<Entity> membersInRange =
                _targetingService.GetEntitiesInRange(
                    _crowdMembers,
                    mine.GetEntityPosition(),
                    mineState.ExplosionRadius);

            if (membersInRange.Count > 0)
            {
                _removeMembersRequest.Publish(new RemoveCrowdMembersRequest
                {
                    SpecificMembers = membersInRange.ToArray()
                }, allowNextFrame: true);
            }

            ref readonly EntityViewLink viewLink = ref mine.GetComponent<EntityViewLink>();
            MineView mineView = (MineView)viewLink.View;
            mineView.PlayExplosionEffect();
        }

        public void Dispose()
        {
        }
    }
}