using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Configs
{
    [CreateAssetMenu(fileName = nameof(CrowdConfig),
        menuName = ProjectConfig.PROJECT_NAME + "/Configs/" + nameof(CrowdConfig))]
    public class CrowdConfig : SerializedScriptableObject
    {
        [FormerlySerializedAs("_crowdLeadPrefab")]
        [SerializeField]
        [Required]
        [AssetsOnly]
        private CrowdView _crowdPrefab;

        [SerializeField]
        [Required]
        [AssetsOnly]
        private CinemachineVirtualCamera _followCameraPrefab;

        [SerializeField]
        private int _maxPopulation = 15;

        [SerializeField]
        private float _horizontalMoveSpeed = 3f;

        [SerializeField]
        private float _forwardMoveSpeed = 3f;

        [SerializeField]
        private float _horizontalSmoothingTime = 0.13f;

        [SerializeField]
        private float _forwardSmoothingTime = 0.17f;

        [FormerlySerializedAs("_crowdMemberPrefab")]
        [SerializeField, Required, AssetsOnly]
        private MilitaryCrowdMemberView _militaryMemberPrefab;
        
        [SerializeField, Required, AssetsOnly]
        private CivilianCrowdMemberView _civilianMemberPrefab;

        [SerializeField]
        private float _shootingCooldown = 1f;

        [SerializeField]
        private float _damage = 10f;

        [SerializeField]
        private int _pipsPerTier = 5;
        
        [SerializeField]
        private float _shootingRange = 10f;

        [SerializeField, ListDrawerSettings(
             DraggableItems = false,
             HideAddButton = false,
             HideRemoveButton = false,
             NumberOfItemsPerPage = 5
         )]
        private CrowdMemberEvolutionConfig[] _evolutions;
        
        public float ShootingRange => _shootingRange;
        public float Damage => _damage;
        public float ShootingCooldown => _shootingCooldown;
        public int MaxEvolutionLevel => _evolutions.Length;
        public int PipsPerTier => _pipsPerTier;
        public CrowdMemberEvolutionConfig[] Evolutions => _evolutions;
        public CinemachineVirtualCamera FollowCameraPrefab => _followCameraPrefab;
        public CrowdView CrowdPrefab => _crowdPrefab;
        public float HorizontalMoveSpeed => _horizontalMoveSpeed;
        public float ForwardMoveSpeed => _forwardMoveSpeed;
        public float HorizontalSmoothingTime => _horizontalSmoothingTime;
        public MilitaryCrowdMemberView MilitaryMemberPrefab => _militaryMemberPrefab;
        public float ForwardSmoothingTime => _forwardSmoothingTime;
        public int MaxPopulation => _maxPopulation;
        public CivilianCrowdMemberView CivilianMemberPrefab => _civilianMemberPrefab;

        public CrowdMemberEvolutionConfig GetEvolution(int evolutionLevel)
        {
            if (Evolutions == null || Evolutions.Length == 0)
                return null;

            int index = Mathf.Clamp(evolutionLevel - 1, 0, Evolutions.Length - 1);
            return Evolutions[index];
        }

        public int GetTierFromEvolution(int evolutionLevel) =>
            (evolutionLevel - 1)/ _pipsPerTier;
        
        public bool IsTierUpEvolution(int evolutionLevel) =>
            (evolutionLevel - 1) % _pipsPerTier == 0;
    }
}