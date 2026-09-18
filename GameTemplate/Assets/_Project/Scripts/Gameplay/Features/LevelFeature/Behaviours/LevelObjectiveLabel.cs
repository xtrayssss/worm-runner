using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours
{
    [Serializable]
    public sealed class LevelObjectiveLabel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _objectiveLabel;

        [SerializeField]
        private RectTransform _content;

        [NonSerialized]
        private LevelService _levelService;

        [NonSerialized]
        private CaptureZoneService _captureZoneService;

        [NonSerialized]
        private CollectibleService _collectibleService;

        private readonly UIRoot _uiRoot;

        public void Construct(
            LevelService levelService,
            CaptureZoneService captureZoneService,
            CollectibleService collectibleService)
        {
            _levelService = levelService;
            _captureZoneService = captureZoneService;
            _collectibleService = collectibleService;

            _levelService.OnLevelChanged += OnLevelChanged;
        }

        private void OnDestroy()
        {
            _levelService.OnLevelChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int levelIndex)
        {
            UpdateObjectiveDisplay();
        }

        public void UpdateObjectiveDisplay()
        {
            LevelConfig currentLevel = _levelService.CurrentLevel;

            HideObjectiveDisplay();

            string objectiveText = currentLevel.LevelMode switch
            {
                LevelMode.CAPTURE_ZONES => GetCaptureZoneProgress(),
                LevelMode.COLLECTOR => GetCollectorProgress(),
                _ => null
            };

            if (string.IsNullOrEmpty(objectiveText))
                HideObjectiveDisplay();
            else
                ShowObjectiveDisplay(objectiveText);
        }

        private string GetCaptureZoneProgress()
        {
            int current = _captureZoneService.TotalCapturedMembers;
            int required = _captureZoneService.RequiredMembersForCompletion;

            return "Captured: \n" +
                   $"{current}/{required}";
        }

        private string GetCollectorProgress()
        {
            int current = _collectibleService.CollectedCount;
            int total = _collectibleService.TotalCollectibles;

            return "Collected: \n" +
                   $"{current}/{total}";
        }

        private void ShowObjectiveDisplay(string text)
        {
            _objectiveLabel.text = text;
            _content.gameObject.SetActive(true);
        }

        private void HideObjectiveDisplay()
        {
            _content.gameObject.SetActive(false);
        }
    }
}