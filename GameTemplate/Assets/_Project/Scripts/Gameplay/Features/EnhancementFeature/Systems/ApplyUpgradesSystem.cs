using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Components;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ApplyUpgradesSystem : ISystem
    {
        public World World { get; set; }

        private readonly EnhancementService _enhancementService;

        private Request<ApplyUpgradeRequest> _applyUpgradeRequest;
        private Event<UpgradeAppliedEvent> _upgradeAppliedEvent;
        private readonly CurrencyService _currencyService;
        private Filter _crowds;

        public ApplyUpgradesSystem(EnhancementService enhancementService, CurrencyService currencyService)
        {
            _enhancementService = enhancementService;
            _currencyService = currencyService;
        }

        public void OnAwake()
        {
            _crowds = World.Filter
                .With<CrowdTag>()
                .With<EntityViewLink>()
                .Build();

            _applyUpgradeRequest = World.GetRequest<ApplyUpgradeRequest>();
            _upgradeAppliedEvent = World.GetEvent<UpgradeAppliedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (ApplyUpgradeRequest request in _applyUpgradeRequest.Consume())
            {
                if (!_currencyService.HasEnough(CurrencyType.MONEY, request.Cost))
                    continue;

                bool wasApplied = ApplyUpgrade(in request);

                if (wasApplied)
                {
                    _currencyService.TrySpendMoney(request.Cost);

                    _upgradeAppliedEvent.NextFrame(new UpgradeAppliedEvent
                    {
                        UpgradeType = request.UpgradeType,
                        Value = request.Value,
                        IntValue = request.IntValue,
                        Target = request.UpgradeSource
                    });
                }
            }
        }

        private bool ApplyUpgrade(in ApplyUpgradeRequest request)
        {
            Entity crowd = _crowds.First();
            ref readonly EntityViewLink crowdViewLink = ref crowd.GetComponent<EntityViewLink>();
            Vector3 crowdPosition = crowdViewLink.View.transform.position;

            return _enhancementService.ApplyUpgrade(in request, crowdPosition);
        }

        public void Dispose()
        {
        }
    }
}