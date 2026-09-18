using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours
{
    public sealed class CurrencySpreadAttractSpawner : MonoBehaviour
    {
        [FormerlySerializedAs("_coinPrefab")]
        [Header("Configuration")]
        [SerializeField] private CurrencyView _currencyPrefab;

        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private CurrencyTarget _targetPoint;

        [Header("Scatter Parameters")]
        [SerializeField] private float _scatterRadius = 1.5f;

        [SerializeField] private Vector2 _delayRange = new Vector2(0f, 0.1f);
        [SerializeField] private Vector2 _durationRange = new Vector2(0.4f, 0.7f);
        [SerializeField] private Ease _ease = Ease.InBack;

        private const int MAX_CURRENCY_OBJECTS = 30;

        [Button]
        public Sequence SpreadCurrency(float totalCurrency)
        {
            Sequence sequence = Sequence.Create(useUnscaledTime: true);

            float currencyCount = Mathf.Min(totalCurrency, MAX_CURRENCY_OBJECTS);

            for (int i = 0; i < currencyCount; i++)
            {
                CurrencySpreadAttractAnimation currencyEffect = CreateCurrency();
                float delay = Random.Range(_delayRange.x, _delayRange.y);
                sequence.Group(currencyEffect.Play(delay));
            }

            return sequence;
        }

        private CurrencySpreadAttractAnimation CreateCurrency()
        {
            CurrencyView currencyView = Instantiate(_currencyPrefab, _spawnPoint);
            currencyView.Construct();
            
            if (currencyView.Trail != null) 
                currencyView.Trail.emitting = false;

            CurrencySpreadAttractAnimation currencyEffect = new CurrencySpreadAttractAnimation(
                new CurrencySpreadAttractAnimationConfig
                {
                    StartPosition = new Vector3(
                        _spawnPoint.position.x + Random.Range(-0.5f, 0.5f),
                        _spawnPoint.position.y + Random.Range(-0.5f, 0.5f),
                        0f),
                    Duration = Random.Range(_durationRange.x, _durationRange.y),
                    EndPosition = _targetPoint.transform.position,
                    CurrencyView = currencyView,
                    Target =  _targetPoint
                });

            return currencyEffect;
        }
    }
}