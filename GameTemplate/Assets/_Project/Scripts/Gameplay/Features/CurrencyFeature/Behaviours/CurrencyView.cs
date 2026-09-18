using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours
{
    public sealed class CurrencyView : MonoBehaviour
    {
        [field: SerializeField] public TrailRenderer Trail { get; private set; }
        public RectTransform RectTransform { get; private set; }

        public void Construct() =>
            RectTransform = GetComponent<RectTransform>();
    }
}