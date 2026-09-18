using _Project.Scripts.Gameplay.Features.LifeForceFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class CommonInteractiveObjectView : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _destructionSound;

        [SerializeField]
        private Collider _collider;

        [SerializeField]
        private HealthBar _healthBar;

        [SerializeField]
        private VFXData _destructionVFX;

        [SerializeField]
        private VFXData _buffVfx;

        [SerializeField]
        private Transform _visualRoot;
       
        public VFXData DestructionVFX => _destructionVFX;
        public HealthBar HealthBar => _healthBar;
        public Collider Collider => _collider;
        public AudioSource DestructionSound => _destructionSound;
        public VFXData BuffVfx => _buffVfx;
        public Transform VisualRoot => _visualRoot;
    }
}
