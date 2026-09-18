using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
    public sealed class DistanceProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Slider _progressSlider;

        private float _targetDistance = 1f;

        private LevelService _levelService;

        public void Construct(LevelService levelService)
        {
            _progressSlider.minValue = 0f;
            _progressSlider.maxValue = 1f;
            _progressSlider.value = 0f;

            _levelService = levelService;
            _levelService.OnLevelChanged += OnLevelChanged;
        }

        private void OnDestroy()
        {
            _levelService.OnLevelChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int levelIndex)
        {
            Show();

            LevelConfig currentLevel = _levelService.CurrentLevel;

            switch (currentLevel.LevelMode)
            {
                case LevelMode.DEFENSE:
                    Hide();
                    break;
            }

            if (gameObject.activeSelf)
                UpdateTargetDistance();
        }

        public void UpdateProgress(float currentDistance)
        {
            float progress = _targetDistance > 0f
                ? Mathf.Clamp01(currentDistance / _targetDistance)
                : 0f;

            _progressSlider.value = progress;
        }

        public void ResetProgress() =>
            UpdateProgress(0);

        private void UpdateTargetDistance() =>
            _targetDistance = Mathf.Max(0f, _levelService.GetCurrentLevelDistance());

        private void Hide() =>
            gameObject.SetActive(false);

        private void Show() =>
            gameObject.SetActive(true);
    }
}