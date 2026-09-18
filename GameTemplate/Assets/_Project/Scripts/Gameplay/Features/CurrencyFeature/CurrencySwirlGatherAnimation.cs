using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature
{
    public sealed class CurrencySwirlGatherAnimation
    {
        private readonly CurrencySwirlGatherAnimationConfig _effectConfig;
        private readonly Vector2 _controlPoint1;
        private readonly Vector2 _controlPoint2;
        private readonly Vector2 _originalScale;
        private readonly Vector2 _initialRandomPosition;
        private readonly float _depth;

        public CurrencySwirlGatherAnimation(CurrencySwirlGatherAnimationConfig config)
        {
            _originalScale = config.Currency.transform.localScale;
            _effectConfig = config;
            _depth = config.Currency.transform.position.z;
            config.Currency.transform.position =
                new Vector3(_effectConfig.StartPosition.x, _effectConfig.StartPosition.y, _depth);
            _initialRandomPosition = GetRandomPositionInCircle(_effectConfig.StartPosition, 1f);

            _controlPoint1 = CalculateControlPoint(_effectConfig.StartPosition, _effectConfig.EndPosition,
                _effectConfig.CurveStartIntensity,
                _effectConfig.CurveStartAngle);
            _controlPoint2 = CalculateControlPoint(_effectConfig.EndPosition, _effectConfig.StartPosition,
                _effectConfig.CurveEndIntensity,
                _effectConfig.CurveEndAngle);

            float perspectiveScale = 1f + (1f - 0) * 0.5f;
            config.Currency.transform.localScale = _originalScale * perspectiveScale;
        }

        public Sequence PlayAnimation(float delay)
        {
            return Sequence.Create()
                .Chain(Tween.Delay(delay))
                .Chain(PlayMoveToRandomPosition())
                .ChainCallback(this, static animation =>
                {
                    if (animation._effectConfig.Currency.Trail != null)
                        animation._effectConfig.Currency.Trail.emitting = false;
                })
                .Chain(PlayIdleAnimation())
                .ChainCallback(this, static animation =>
                {
                    if (animation._effectConfig.Currency.Trail != null)
                        animation._effectConfig.Currency.Trail.emitting = true;
                })
                .Chain(PlayMoveToTargetAnimation())
                .ChainCallback(this, static animation =>
                {
                    animation._effectConfig.Target.AnimateCurrencyTarget();
                    Object.Destroy(animation._effectConfig.Currency.gameObject);
                });
        }

        private Vector2 GetRandomPositionInCircle(Vector2 center, float radius)
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float r = radius * Mathf.Sqrt(Random.Range(0f, 1f));
            return center + new Vector2(r * Mathf.Cos(angle), r * Mathf.Sin(angle));
        }

        private Sequence PlayMoveToRandomPosition()
        {
            return Sequence.Create()
                .Chain(Tween.Position(
                    _effectConfig.Currency.transform,
                    endValue: new Vector3(_initialRandomPosition.x, _initialRandomPosition.y, _depth),
                    duration: 0.3f,
                    ease: Ease.OutCubic));
        }

        private Sequence PlayIdleAnimation()
        {
            float offset = Random.Range(-25f, -30f);
            float duration = Random.Range(0.7f, 0.8f);

            return Sequence.Create().Chain(
                Tween.ShakeLocalPosition(
                    _effectConfig.Currency.transform,
                    duration: duration,
                    strength: new Vector3(offset, offset, 0),
                    easeBetweenShakes: Ease.InOutSine,
                    frequency: 2,
                    enableFalloff: true,
                    asymmetryFactor: 0.5f));
        }

        private Sequence PlayMoveToTargetAnimation()
        {
            return Sequence.Create()
                .Chain(Tween.Custom(
                    startValue: 0f,
                    endValue: 1f,
                    duration: _effectConfig.Duration,
                    target: this,
                    onValueChange: static (animation, value) =>
                    {
                        Vector2 position = CubicBezier(
                            t: value,
                            p0: animation._initialRandomPosition,
                            p1: animation._controlPoint1,
                            p2: animation._controlPoint2,
                            p3: animation._effectConfig.EndPosition);
                        animation._effectConfig.Currency.transform.position =
                            new Vector3(position.x, position.y, animation._depth);
                        float perspectiveScale = 1f + (1f - value) * 0.5f;
                        animation._effectConfig.Currency.transform.localScale =
                            animation._originalScale * perspectiveScale;
                    },
                    ease: Ease.Linear));
        }

        private Vector2 CalculateControlPoint(Vector2 from, Vector2 to, float intensity, float angleOffset)
        {
            float distance = Vector2.Distance(from, to);
            float length = distance * intensity;
            float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) + angleOffset;

            return new Vector2(
                from.x + Mathf.Cos(angle) * length,
                from.y + Mathf.Sin(angle) * length
            );
        }

        private static Vector2 CubicBezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 result = uuu * p0 +
                             3 * uu * t * p1 +
                             3 * u * tt * p2 +
                             ttt * p3;

            return result;
        }
    }
}