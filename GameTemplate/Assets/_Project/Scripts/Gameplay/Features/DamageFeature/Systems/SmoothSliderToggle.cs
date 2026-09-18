using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
{
    public class SmoothSliderToggle : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider _toggleSlider;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _handleImage;

        [Header("Animation Settings")]
        [Range(0.1f, 10f)]
        [SerializeField] private float _animationDuration = 0.3f;

        private Sequence _animation;
        private Color _backgroundOriginalColor;

        public void Awake()
        {
            if (_toggleSlider == null)
                _toggleSlider = GetComponent<Slider>();

            _toggleSlider.minValue = 0f;
            _toggleSlider.maxValue = 1f;

            _backgroundOriginalColor = _backgroundImage.color;
        }

        public void Animate(bool isToggledOn = true, bool useAnimation = true)
        {
            _animation.Stop();
            
            if (useAnimation)
            {
                Color targetBackgroundColor = _backgroundOriginalColor;
                targetBackgroundColor.a = isToggledOn ? 1f : 0f; 

                _animation = Sequence.Create(useUnscaledTime:true)
                    .Group(
                        Tween.UISliderValue(
                            target: _toggleSlider,
                            endValue: isToggledOn ? 1f : 0f,
                            duration: _animationDuration,
                            ease: Ease.InOutSine
                        ))
                    .Group(
                        Tween.Color(
                            target: _backgroundImage,
                            endValue: targetBackgroundColor,
                            duration: _animationDuration
                        ));
            }
            else
            {
                _toggleSlider.value = isToggledOn ? 1f : 0f;
                
                Color newBackgroundColor = _backgroundOriginalColor;
                newBackgroundColor.a = isToggledOn ? 1f : 0f;
                _backgroundImage.color = newBackgroundColor;
            }
        }
    }
}