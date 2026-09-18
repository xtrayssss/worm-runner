using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Cysharp.Threading.Tasks;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Android;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MortarStrikeSystem : ISystem
    {
        public World World { get; set; }

        private Filter _mortarStrikes;
        private Filter _crowdMembers;
        private readonly TargetingService _targetingService;
        private Request<RemoveCrowdMembersRequest> _removeMembersRequest;

        public MortarStrikeSystem(TargetingService targetingService)
        {
            _targetingService = targetingService;
        }

        public void OnAwake()
        {
            _mortarStrikes = World.Filter
                .With<MortarTelegraphTag>()
                .With<MortarStrikeState>()
                .With<EntityViewLink>()
                .Build();

            _crowdMembers = World.Filter
                .With<MilitaryCrowdMemberTag>()
                .With<EntityViewLink>()
                .Build();

            _removeMembersRequest = World.GetRequest<RemoveCrowdMembersRequest>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity strike in _mortarStrikes)
            {
                ref MortarStrikeState state = ref strike.GetComponent<MortarStrikeState>();
                ref readonly EntityViewLink viewLink = ref strike.GetComponent<EntityViewLink>();

                if (!state.HasImpacted)
                {
                    if (!state.HasLaunchedProjectile)
                    {
                        state.HasLaunchedProjectile = true;
                        MortarTelegraphView telegraphView = (MortarTelegraphView)viewLink.View;
                        telegraphView
                            .LaunchProjectile(state.TelegraphDuration)
                            .ToYieldInstruction()
                            .ToUniTask()
                            .ContinueWith(() => ExecuteImpact(strike))
                            .Forget();
                    }
                }
            }
        }

        private void ExecuteImpact(Entity strike)
        {
            if (strike.IsNullOrDisposed())
                return;
            
            ref MortarStrikeState state = ref strike.GetComponent<MortarStrikeState>();
            ref readonly EntityViewLink viewLink = ref strike.GetComponent<EntityViewLink>();

            state.HasImpacted = true;

            MortarTelegraphView telegraphView = (MortarTelegraphView)viewLink.View;

            telegraphView.PlayImpactEffect();

            DealDamageInRadius(strike, state);

            telegraphView
                .PlayDestructionAnimation()
                .OnComplete(
                    telegraphView,
                    static view =>
                    {
                        if (view.Entity.IsNullOrDisposed())
                            return;

                        World.Default.RemoveEntity(view.Entity);
                        Object.Destroy(view.gameObject);
                    });
        }

        private void DealDamageInRadius(Entity strike, in MortarStrikeState state)
        {
            List<Entity> victimsInRange =
                _targetingService.GetEntitiesInRange(_crowdMembers, strike.GetEntityPosition(), state.ImpactRadius);

            if (victimsInRange.Count > 0)
            {
                _removeMembersRequest.Publish(new RemoveCrowdMembersRequest
                {
                    SpecificMembers = victimsInRange.ToArray(),
                }, allowNextFrame: true);
            }
        }

        public void Dispose()
        {
        }
    }
}