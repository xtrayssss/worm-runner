using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature
{
    public sealed class CurrencySpreadAttractAnimation
    {
        private readonly float _depth;
        private readonly CurrencySpreadAttractAnimationConfig _config;

        public CurrencySpreadAttractAnimation(CurrencySpreadAttractAnimationConfig config)
        {
            _config = config;
            _depth = config.CurrencyView.transform.position.z;
            config.CurrencyView.transform.position =
                new Vector3(_config.StartPosition.x, _config.StartPosition.y, _depth);
        }

        public Sequence Play(float delay)
        {
            return Sequence.Create()
                .Chain(Tween.Delay(delay))
                .Chain(PlayMovement())
                .ChainCallback(this, static effect => Object.Destroy(effect._config.CurrencyView.gameObject))
                .ChainCallback( this, static effect => 
                {
                    effect._config.Target.AnimateCurrencyTarget();
                });
        }

        private Sequence PlayMovement()
        {
            return Sequence.Create()
                .Chain(Tween.Position(
                    _config.CurrencyView.transform,
                    endValue: new Vector3(_config.EndPosition.x, _config.EndPosition.y, _depth),
                    duration: _config.Duration,
                    ease: Ease.InBack));
        }
    }
}