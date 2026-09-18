using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Behaviours
{
    public sealed class PipView : MonoBehaviour
    {
        [SerializeField] private Image _pipImage;

        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private Sprite _inactiveSprite;

        public void SetSprite(bool isActive) =>
            _pipImage.sprite = isActive ? _activeSprite : _inactiveSprite;
    }
}