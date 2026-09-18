using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Behaviours
{
    public sealed class WaveIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private TextMeshProUGUI _enemiesRemainingText;

        public void Display(int wave, int count)
        {
            _waveText.text = $"{wave}/{count}";
        }

        public void DisplayRemainingEnemies(int remaining, Color textColor)
        {
            _enemiesRemainingText.text = remaining.ToString();
            _enemiesRemainingText.color = textColor;
        }

        public void HideRemainingEnemies() =>
            _enemiesRemainingText.gameObject.SetActive(false);

        public void ShowRemainingEnemies() =>
            _enemiesRemainingText.gameObject.SetActive(true);
    }
}