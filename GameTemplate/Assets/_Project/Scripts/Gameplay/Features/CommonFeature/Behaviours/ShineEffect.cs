using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class ShineEffect : MonoBehaviour
    {
        [Header("Shine Settings")]
        [SerializeField] private RectTransform _shineGlow;

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 1.5f;

        [SerializeField] private float _delayBetweenShines = 3f;
        [SerializeField] private bool _playOnAwake = true;
        [SerializeField] private bool _loopAnimation = true;

        [Header("Movement Settings")]
        [SerializeField] private Vector2 _endOffset = new Vector2(200f, 0f);

        private Sequence _shineSequence;
        private Vector2 _startOffset;

        private void Awake()
        {
            _startOffset = _shineGlow.anchoredPosition;

            SetupInitialState();
        }

        private void Start()
        {
            if (_playOnAwake)
                PlayShine();
        }

        private void OnDestroy() =>
            StopShine();

        [Button]
        private void PlayShine()
        {
            StopShine();
            SetupInitialState();

            _shineSequence = Sequence.Create(useUnscaledTime: true, cycles: _loopAnimation ? -1 : 1)
                .Chain(Tween.UIAnchoredPosition(_shineGlow, _endOffset, _animationDuration, Ease.InOutSine))
                .Chain(Tween.Delay(_delayBetweenShines));
        }

        public void StopShine()
        {
            if (_shineSequence.isAlive)
                _shineSequence.Stop();

            SetupInitialState();
        }

        private void SetupInitialState() =>
            _shineGlow.anchoredPosition = _startOffset;
    }
}