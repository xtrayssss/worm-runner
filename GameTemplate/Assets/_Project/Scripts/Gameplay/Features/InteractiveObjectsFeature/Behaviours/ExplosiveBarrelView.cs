using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class ExplosiveBarrelView : InteractiveObjectView
    {
        private CameraService _cameraService;

        public void Construct(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public void PlayExplosionEffect()
        {
            _cameraService.PlayCameraShake(strength: 0.4f, duration: 0.2f);
            PlayDestructionVFX(scale: 1.7f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Entity.IsNullOrDisposed())
                return;

            ref readonly ExplosiveBarrel explosiveBarrel = ref Entity.GetComponent<ExplosiveBarrel>();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosiveBarrel.ExplosionRadius);
        }
#endif
    }
}