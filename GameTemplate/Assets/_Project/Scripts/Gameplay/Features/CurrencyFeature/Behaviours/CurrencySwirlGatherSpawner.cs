using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours
{
    public sealed class CurrencySwirlGatherSpawner : MonoBehaviour
    {
        [FormerlySerializedAs("_coinPrefab")]
        [Header("Configuration")]
        [SerializeField]
        private CurrencyView _currencyPrefab;

        [SerializeField] private Transform _startPoint;
        [SerializeField] private CurrencyTarget _targetPoint;

        [Header("Coin Spawning Parameters")]
        [SerializeField]
        private int _totalValue = 1000;

        [FormerlySerializedAs("_numberOfCoinsRange")] [SerializeField]
        private Vector2 _numberOfCurrenciesRange = new Vector2(0, 300);

        [SerializeField] private Vector2 _delayRange = new Vector2(0, 0);
        [SerializeField] private Vector2 _durationRange = new Vector2(0.6f, 1f);
        [SerializeField] private Vector2 _arcIntensityRange = new Vector2(0, 0.5f);

        private const int VALUE_INCREMENT = 20;
        private const int MAX_CURRENCY_OBJECTS = 30;

        public float CurrencyAnimationDuration { get; private set; }

        private float _numberOfCurrencies;

        [Button]
        public Sequence SpawnCurrencies(float currencyCount = 15)
        {
            Sequence sequence = Sequence.Create(useUnscaledTime: true);
            _numberOfCurrencies = CalculateNumberOfCurrencies(currencyCount);

            for (int i = 0; i < _numberOfCurrencies; i++)
            {
                CurrencySwirlGatherAnimation currencyAnimation = CreateCurrency();
                float spawnDelay = Random.Range(0.03f, 0.04f);

                Sequence currencySequence = sequence
                    .Group(currencyAnimation.PlayAnimation(delay: spawnDelay * i));

                CurrencyAnimationDuration = currencySequence.durationTotal + spawnDelay * i;
            }

            return sequence;
        }

        private float CalculateNumberOfCurrencies(float explicitCurrencyCount)
        {
            if (explicitCurrencyCount > 0)
                return Mathf.Min(explicitCurrencyCount, MAX_CURRENCY_OBJECTS);

            int minCurrencies = Mathf.RoundToInt(Mathf.Min(_numberOfCurrenciesRange.x, _numberOfCurrenciesRange.y));
            int maxCurrencies = Mathf.RoundToInt(Mathf.Max(_numberOfCurrenciesRange.x, _numberOfCurrenciesRange.y));

            int numberOfCurrencies = 0;

            if (_totalValue == 0 || VALUE_INCREMENT == 0 || VALUE_INCREMENT >= _totalValue)
                numberOfCurrencies = minCurrencies;

            return Mathf.Clamp(numberOfCurrencies, minCurrencies, maxCurrencies);
        }

        private CurrencySwirlGatherAnimation CreateCurrency()
        {
            CurrencyView currencyView = Instantiate(
                original: _currencyPrefab,
                parent: _startPoint);

            currencyView.Construct();

            return new CurrencySwirlGatherAnimation(new CurrencySwirlGatherAnimationConfig
            {
                StartPosition = _startPoint.position,
                EndPosition = _targetPoint.transform.position,
                Delay = Random.Range(_delayRange.x, _delayRange.y),
                Duration = Random.Range(_durationRange.x, _durationRange.y),
                CurveStartIntensity = Random.Range(_arcIntensityRange.x, _arcIntensityRange.y),
                CurveEndIntensity = Random.Range(_arcIntensityRange.x, _arcIntensityRange.y),
                CurveStartAngle = Random.Range(-Mathf.PI / 4f, Mathf.PI / 4f),
                CurveEndAngle = Random.Range(-Mathf.PI / 4f, Mathf.PI / 4f),
                Target = _targetPoint,
                Currency = currencyView
            });
        }
    }
}