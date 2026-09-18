using _Project.Scripts.Gameplay.Features.LifeForceFeature.Behaviours;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class EvolutionProgressDisplay : MonoBehaviour
    {
        [SerializeField]
        private HealthBar _progressBar;

        [SerializeField]
        private TMP_Text _levelText;
        
        public void Construct() => 
            _progressBar.Construct();

        public void UpdateDisplay(float currentValue, float maxValue) =>
            _progressBar.UpdateDisplay(currentValue, maxValue);

        public void UpdateLevel(int level) => 
            _levelText.text = $"Level {level}";

        public void ResetProgress() =>
            _progressBar.UpdateDisplay(0, 1);
    }
}