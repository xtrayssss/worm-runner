using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    [RequireComponent(typeof(ButtonAnimator))]
    public sealed class ScrollAwareButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerUpHandler
    {
        private ScrollRect _scrollRect;
        private ButtonAnimator _buttonAnimator;
        private bool _isScrolling;
        private Vector2 _pointerStartPosition;
        [SerializeField] private float _dragThreshold = 5f;

        private void Awake()
        {
            _buttonAnimator = GetComponent<ButtonAnimator>();
            if (_buttonAnimator == null)
            {
#if DEBUG
                Debug.LogError("ButtonAnimator component is required on this GameObject!");
#endif
                enabled = false;
                return;
            }

            _scrollRect = GetComponentInParent<ScrollRect>();
            if (_scrollRect == null)
            {
#if DEBUG
                Debug.LogWarning("ScrollRect not found in parent hierarchy. ScrollAwareButton will be disabled.");
#endif
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _isScrolling = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_scrollRect == null) return;

            Vector2 delta = eventData.position - _pointerStartPosition;
            float absDeltaX = Mathf.Abs(delta.x);
            float absDeltaY = Mathf.Abs(delta.y);

            if (absDeltaY > absDeltaX && absDeltaY > _dragThreshold)
            {
                _isScrolling = true;
                _buttonAnimator.StopAnimation();
                _buttonAnimator.ResetScale();
                _scrollRect.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isScrolling && _scrollRect != null)
            {
                _scrollRect.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isScrolling && _scrollRect != null)
            {
                _scrollRect.OnEndDrag(eventData);
                _isScrolling = false;
                _buttonAnimator.StopAnimation();
                _buttonAnimator.ResetScale();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isScrolling)
            {
                eventData.eligibleForClick = false;
                eventData.pointerPress = null;
                _buttonAnimator.StopAnimation();
                _buttonAnimator.ResetScale();
            }
        }

        public void OnPointerDownStarted(Vector2 pointerPosition) =>
            _pointerStartPosition = pointerPosition;
    }
}