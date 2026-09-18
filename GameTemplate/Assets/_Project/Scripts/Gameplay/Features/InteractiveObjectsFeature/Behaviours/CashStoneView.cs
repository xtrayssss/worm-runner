using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class CashStoneView : InteractiveObjectView
    {
        [Header("Cash Stone Specific")]
        [SerializeField] private ParticleSystem _rewardEffect;
        [SerializeField] private AudioSource _rewardSound;

        public ParticleSystem RewardEffect => _rewardEffect;
        public AudioSource RewardSound => _rewardSound;
    }
}