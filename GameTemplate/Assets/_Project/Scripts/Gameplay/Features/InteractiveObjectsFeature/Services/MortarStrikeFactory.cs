using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services
{
    public sealed class MortarStrikeFactory : IService
    {
        private readonly ConfigsService _configsService;
        private readonly VFXService _vfxService;
        private readonly CameraService _cameraService;
        private readonly AudioService _audioService;

        public MortarStrikeFactory(
            ConfigsService configsService,
            VFXService vfxService,
            CameraService cameraService,
            AudioService audioService)
        {
            _configsService = configsService;
            _vfxService = vfxService;
            _cameraService = cameraService;
            _audioService = audioService;
        }

        public Entity CreateMortarStrike(Vector3 position)
        {
            World world = World.Default;
            Entity strike = world.CreateEntity();

            const float IMPACT_RADIUS = 3f;

            MortarTelegraphView telegraphPrefab = _configsService.GetMortarTelegraphPrefab();
            MortarTelegraphView view =
                Object.Instantiate(telegraphPrefab, position, telegraphPrefab.transform.rotation);
            view.Entity = strike;
            view.Construct(_vfxService, _cameraService, _audioService);
            view.SetRadius(IMPACT_RADIUS);

            strike.AddComponent<MortarTelegraphTag>();

            strike.AddComponent<EntityViewLink>() = new EntityViewLink
            {
                View = view
            };

            const float TELEGRAPH_DURATION = 1.3f;

            strike.AddComponent<MortarStrikeState>() = new MortarStrikeState
            {
                TelegraphDuration = TELEGRAPH_DURATION,
                ImpactRadius = IMPACT_RADIUS,
                HasImpacted = false
            };

            view.PlayPulseAnimation();

#if DEBUG
            strike.AddComponent<EntityName>().Value = $"MortarStrike_{strike.Id}";
#endif

            return strike;
        }
    }
}