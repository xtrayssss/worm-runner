using System.Globalization;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class SilverCoinsWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _countLabel;
        [field: SerializeField, ReadOnly] public CurrencySwirlGatherSpawner CurrencySwirlGatherSpawner { get; private set; }

        public void Construct() =>
            CurrencySwirlGatherSpawner = GetComponent<CurrencySwirlGatherSpawner>();

        public void DisplayCoins(float amount) => 
            _countLabel.text = ((int)amount).ToString(CultureInfo.InvariantCulture);

        public Sequence PlayMagnetAnimation(float amount) => 
            CurrencySwirlGatherSpawner.SpawnCurrencies(amount);
    }
}