using System;
using PrimeTween;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    public class TutorialPointer
    {
        [SerializeField]
        private RectTransform _pointerPrefab;

        [SerializeField]
        private PointerAnimationType _animationType = PointerAnimationType.BOUNCE;

        [SerializeField]
        private TutorialStepTemplate.ElementReference _startPoint;

        [SerializeField]
        private TutorialStepTemplate.ElementReference _endPoint;

        [SerializeField]
        private float _animationDuration = 1.2f;

        [SerializeField]
        private float _bounceHeight = 0.3f;

        [SerializeField]
        private Vector2 _scaleRange = new Vector2(0.9f, 1.1f);

        [SerializeField]
        private float _rotationAngle = 5f;

        private RectTransform _pointerInstance;
        private Sequence _animationSequence;
        private CanvasGroup _canvasGroup;

        private RectTransform _startTransform;
        private RectTransform _endTransform;

        public enum PointerAnimationType
        {
            None = 0,
            NATURAL_CLICK = 1,
            TAP_AND_DRAG = 2,
            BOUNCE = 3,
            SCALE = 4
        }

        public void Show(RectTransform parent)
        {
            if (_pointerPrefab == null || string.IsNullOrEmpty(_startPoint.ElementKey))
                return;

            _startTransform = TutorialUtils.GetReferenceTransform(_startPoint);
            _endTransform = TutorialUtils.GetReferenceTransform(_endPoint);

            _pointerInstance = Object.Instantiate(_pointerPrefab, parent);
            _pointerInstance.position = _startTransform.position;
            _canvasGroup = _pointerInstance.GetComponent<CanvasGroup>() ??
                           _pointerInstance.gameObject.AddComponent<CanvasGroup>();

            switch (_animationType)
            {
                case PointerAnimationType.NATURAL_CLICK:
                    PlayNaturalClickAnimation();
                    break;
                case PointerAnimationType.TAP_AND_DRAG:
                    PlayTapAndDragAnimation();
                    break;
                case PointerAnimationType.BOUNCE:
                    PlayBounceAnimation();
                    break;
                case PointerAnimationType.SCALE:
                    PlayScaleAnimation();
                    break;
            }
        }

        private void PlayNaturalClickAnimation()
        {
            Vector3 startPos = _pointerInstance.position;
            Quaternion startRot = _pointerInstance.rotation;
            Vector3 startScale = _pointerInstance.localScale;
            float startAlpha = _canvasGroup?.alpha ?? 1f;

            Vector3 upPos = startPos + Vector3.up * _bounceHeight;
            Vector3 downPos = startPos + Vector3.down * _bounceHeight * 0.5f;
            Quaternion upRot = Quaternion.Euler(0, 0, -_rotationAngle);
            Quaternion downRot = Quaternion.Euler(0, 0, _rotationAngle * 0.5f);
            Vector3 upScale = startScale * _scaleRange.y;
            Vector3 downScale = startScale * _scaleRange.x;
            float upAlpha = 1f;
            float downAlpha = 0.95f;

            float liftDuration = _animationDuration * 0.3f;
            float pressDuration = _animationDuration * 0.4f;
            float recoverDuration = _animationDuration * 0.3f;

            Sequence liftPhase = Sequence.Create()
                .Group(Tween.Position(_pointerInstance, startPos, upPos, liftDuration, Ease.OutQuad))
                .Group(Tween.Rotation(_pointerInstance, startRot, upRot, liftDuration))
                .Group(Tween.Scale(_pointerInstance, startScale, upScale, liftDuration))
                .Group(Tween.Alpha(_canvasGroup, startAlpha, upAlpha, liftDuration));

            Sequence pressPhase = Sequence.Create()
                .Group(Tween.Position(_pointerInstance, upPos, downPos, pressDuration, Ease.InQuad))
                .Group(Tween.Rotation(_pointerInstance, upRot, downRot, pressDuration))
                .Group(Tween.Scale(_pointerInstance, upScale, downScale, pressDuration))
                .Group(Tween.Alpha(_canvasGroup, upAlpha, downAlpha, pressDuration));

            Sequence recoverPhase = Sequence.Create()
                .Group(Tween.Position(_pointerInstance, downPos, startPos, recoverDuration, Ease.OutBack))
                .Group(Tween.Rotation(_pointerInstance, downRot, startRot, recoverDuration))
                .Group(Tween.Scale(_pointerInstance, downScale, startScale, recoverDuration))
                .Group(Tween.Alpha(_canvasGroup, downAlpha, startAlpha, recoverDuration));

            _animationSequence = Sequence.Create()
                .Chain(liftPhase)
                .Chain(pressPhase)
                .Chain(recoverPhase)
                .SetCycles(-1);
        }

        private void PlayTapAndDragAnimation()
        {
            if (_endTransform == null) return;

            Vector3 startPos = _pointerInstance.position;
            Vector3 endPos = _endTransform.position;
            Vector3 dragOffset = endPos - startPos;

            float phase1Duration = _animationDuration * 0.2f;
            float phase2Duration = _animationDuration * 0.25f;
            float phase3Duration = _animationDuration * 0.25f;
            float phase4Duration = _animationDuration * 0.3f;

            Sequence phase1 = Sequence.Create()
                .Group(Tween.Position(_pointerInstance, startPos, startPos, phase1Duration))
                .Group(Tween.Scale(_pointerInstance, Vector3.one, Vector3.one, phase1Duration))
                .Group(Tween.Rotation(_pointerInstance, Quaternion.identity, Quaternion.identity, phase1Duration));

            Sequence phase2 = Sequence.Create()
                .Group(Tween.Position(_pointerInstance, startPos, startPos + Vector3.up * -0.15f, phase2Duration,
                    Ease.OutQuad))
                .Group(Tween.Scale(_pointerInstance, Vector3.one, Vector3.one * 0.9f, phase2Duration))
                .Group(Tween.Rotation(_pointerInstance, Quaternion.identity, Quaternion.Euler(0, 0, -5f),
                    phase2Duration));

            Sequence phase3 = Sequence.Create()
                .Group(Tween.Position(_pointerInstance,
                    startPos + Vector3.up * -0.15f,
                    startPos + dragOffset + Vector3.up * -0.15f,
                    phase3Duration, Ease.OutQuart))
                .Group(Tween.Scale(_pointerInstance, Vector3.one * 0.9f, Vector3.one * 0.9f, phase3Duration))
                .Group(Tween.Rotation(_pointerInstance,
                    Quaternion.Euler(0, 0, -5f),
                    Quaternion.Euler(0, 0, 5f),
                    phase3Duration));

            Sequence phase4 = Sequence.Create()
                .Group(Tween.Position(_pointerInstance,
                    startPos + dragOffset + Vector3.up * -0.15f,
                    startPos + dragOffset,
                    phase4Duration, Ease.OutBack))
                .Group(Tween.Scale(_pointerInstance, Vector3.one * 0.9f, Vector3.one, phase4Duration))
                .Group(Tween.Rotation(_pointerInstance,
                    Quaternion.Euler(0, 0, 5f),
                    Quaternion.identity,
                    phase4Duration));

            _animationSequence = Sequence.Create()
                .Chain(phase1)
                .Chain(phase2)
                .Chain(phase3)
                .Chain(phase4)
                .SetCycles(-1);

            _pointerInstance.localScale = Vector3.one * (1f / _pointerInstance.lossyScale.x);
        }

        private void PlayBounceAnimation()
        {
            var startPos = _pointerInstance.position;
            var upPos = startPos + Vector3.up * _bounceHeight;

            _animationSequence = Sequence.Create(cycles: -1, CycleMode.Yoyo)
                .Chain(Tween.Position(_pointerInstance, startPos, upPos, _animationDuration * 0.5f, Ease.OutQuad))
                .Chain(Tween.Position(_pointerInstance, upPos, startPos, _animationDuration * 0.5f, Ease.InQuad));
        }

        private void PlayScaleAnimation()
        {
            _animationSequence = Sequence.Create(cycles: -1, CycleMode.Yoyo)
                .Chain(Tween.Scale(_pointerInstance,
                    Vector3.one * _scaleRange.x,
                    Vector3.one * _scaleRange.y,
                    _animationDuration,
                    Ease.InOutBack));
        }

        public void Hide()
        {
            _animationSequence.Stop();
            if (_pointerInstance != null)
            {
                Object.Destroy(_pointerInstance.gameObject);
            }
        }
    }
}