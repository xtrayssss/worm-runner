using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours
{
    public sealed class DetectionIndicatorView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _exclamationMark;

        private Sequence _animationSequence;
        private bool _isVisible;

        private void Awake()
        {
            _exclamationMark.gameObject.SetActive(false);
            _isVisible = false;
        }

        public void Show()
        {
            if (_isVisible)
                return;

            _isVisible = true;
            _exclamationMark.gameObject.SetActive(true);

            if (_animationSequence.isAlive)
                _animationSequence.Stop();

            const float DURATION = 0.3f;

            _animationSequence = Sequence.Create()
                .Chain(Tween.PunchScale(
                    transform,
                    new Vector3(transform.localScale.x * 0.25f, transform.localScale.y * 0.25f, 0),
                    DURATION,
                    frequency: 1,
                    easeBetweenShakes: Ease.OutElastic));
        }

        public void Hide()
        {
            if (!_isVisible)
                return;

            if (_animationSequence.isAlive)
                _animationSequence.Stop();

            _exclamationMark.gameObject.SetActive(false);
            _isVisible = false;
        }

        private void OnDestroy()
        {
            if (_animationSequence.isAlive)
                _animationSequence.Stop();
        }
    }
}