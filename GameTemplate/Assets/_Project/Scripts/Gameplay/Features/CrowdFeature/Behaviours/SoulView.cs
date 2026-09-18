using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
    public sealed class SoulView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _soulSprite;

        private void Awake()
        {
            if (_soulSprite == null)
                _soulSprite = GetComponent<SpriteRenderer>();
        }

        public void Construct(Color color)
        {
            _soulSprite.color = new Color(color.r, color.g, color.b, 0f);
            transform.localScale = Vector3.zero;
        }

        public void PlaySoulAnimation()
        {
            const float FADE_IN_DURATION = 0.25f;
            const float SCALE_IN_DURATION = 0.25f;
            const float FINAL_SCALE = 1f;

            const float FLY_HEIGHT = 3f;
            const float FLY_DURATION = 0.65f;

            const float FADE_START_RATIO = 0.65f;
            const float FADE_OUT_DURATION = FLY_DURATION - FLY_DURATION * FADE_START_RATIO;

            Vector3 endPos = transform.position + Vector3.up * FLY_HEIGHT;

            Sequence soulSequence = Sequence.Create();
            
            Color currentColor = _soulSprite.color;
            Color targetColorWithAlpha = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);

            Tween fadeInTween = Tween.Color(
                _soulSprite,
                targetColorWithAlpha,
                FADE_IN_DURATION,
                ease: Ease.InQuad
            );

            Tween scaleInTween = Tween.Scale(
                transform,
                Vector3.one * FINAL_SCALE,
                SCALE_IN_DURATION,
                ease: Ease.InQuad
            );

            Tween flyTween = Tween.PositionY(
                transform,
                endPos.y,
                FLY_DURATION,
                ease: Ease.OutSine
            );

            float fadeStartTime = FLY_DURATION * FADE_START_RATIO;

            Tween fadeOutTween = Tween.Color(
                _soulSprite,
                new Color(currentColor.r, currentColor.g, currentColor.b, 0f),
                FADE_OUT_DURATION,
                ease: Ease.InQuad,
                startDelay: fadeStartTime
            );

            soulSequence
                .Group(fadeInTween)
                .Group(scaleInTween)
                .Chain(flyTween)
                .Group(fadeOutTween)
                .OnComplete(target: this, static soulView => Destroy(soulView.gameObject));
        }
    }
}