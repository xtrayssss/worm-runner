using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature.Behaviours
{
    public sealed class HealthBar : MonoBehaviour
    {
        private enum DisplayType
        {
            SPRITE = 0,
            SLIDER = 1,
            IMAGE_FILL = 2,
            TEXT = 3
        }

        [SerializeField]
        private DisplayType _displayType;

        [SerializeField]
        [ShowIf(nameof(_displayType), DisplayType.SPRITE)]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        [ShowIf(nameof(_displayType), DisplayType.SLIDER)]
        private Slider _slider;

        [SerializeField]
        [ShowIf(nameof(_displayType), DisplayType.IMAGE_FILL)]
        private Image _image;

        [SerializeField]
        [ShowIf(nameof(_displayType), DisplayType.TEXT)]
        private TMP_Text _text;

        [SerializeField]
        [ShowIf(nameof(_displayType), DisplayType.TEXT)]
        private string _textFormat = "{0:F0}/{1:F0}";

        private float _spriteMaxWidth;

        public void Construct()
        {
            if (_displayType == DisplayType.SPRITE) 
                _spriteMaxWidth = _spriteRenderer.size.x;
        }

        public void UpdateDisplay(float current, float max)
        {
            float normalized = Mathf.Clamp01(current / max);

            switch (_displayType)
            {
                case DisplayType.SPRITE:
                    _spriteRenderer.size = new Vector2(normalized * _spriteMaxWidth, _spriteRenderer.size.y);
                    break;

                case DisplayType.SLIDER:
                    _slider.value = normalized;
                    break;

                case DisplayType.IMAGE_FILL:
                    _image.fillAmount = normalized;
                    break;

                case DisplayType.TEXT:
                    _text.text = string.Format(_textFormat, current, max);
                    break;
            }
        }
    }
}