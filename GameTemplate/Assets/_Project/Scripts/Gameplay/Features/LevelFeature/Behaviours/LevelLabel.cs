using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours
{
    public sealed class LevelLabel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _levelText;

        private LevelService _levelService;

        public void Construct(LevelService levelService)
        {
            _levelService = levelService;
            _levelService.OnLevelChanged += HandleLevelChanged;

            HandleLevelChanged(_levelService.CurrentLevelIndex);
        }

        private void OnDestroy()
        {
            _levelService.OnLevelChanged -= HandleLevelChanged;
        }

        private void HandleLevelChanged(int levelIndex)
        {
            _levelText.text = $"{levelIndex + 1}";
        }
    }
}