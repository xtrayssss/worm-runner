using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ConfettiFeature.Configs
{
    [CreateAssetMenu(menuName = ProjectConfig.PROJECT_NAME + "/Configs/Confetti", fileName = "[Confetti] Reward")]
    public sealed class ConfettiConfig : ScriptableObject
    {
        [SerializeField] private ConfettiId _id;
        [SerializeField] private ParticleSystem _prefab;

        public ConfettiId Id => _id;
        public ParticleSystem Prefab => _prefab;
    }
}