using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;

namespace _Project.Scripts.Gameplay.Features.TweenFeature
{
    public static class TweenExtensions
    {
        public static UniTask AsTask(this Tween tween, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();

            tween.OnComplete(tcs, static tcs => tcs.TrySetResult(), warnIfTargetDestroyed: false);
            cancellationToken.Register(static tcs => ((UniTaskCompletionSource)tcs).TrySetCanceled(), tcs);

            return tcs.Task;
        }

        public static UniTask AsTask(this Sequence tween, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();

            tween.ChainCallback(tcs, static tcs => tcs.TrySetResult(), warnIfTargetDestroyed: false);
            cancellationToken.Register(static tcs => ((UniTaskCompletionSource)tcs).TrySetCanceled(), tcs);

            return tcs.Task;
        }

        public static Sequence Jump(
            [NotNull] Transform target,
            Vector3 endValue,
            float duration,
            float height,
            int numJumps = 1,
            Ease jumpUpEase = Ease.OutQuad,
            Ease jumpDownEase = Ease.InQuad,
            Ease horizontalEase = Ease.Linear)
        {
            Assert.IsTrue(height > 0f);
            Assert.IsTrue(numJumps >= 1, nameof(numJumps) + " should be >= 1.");

            var jumpsSequence = Sequence.Create();
            var iniPosY = target.position.y;
            var deltaJump = (endValue.y - iniPosY) / numJumps;
            var jumpDuration = duration / (numJumps * 2);

            for (int i = 0; i < numJumps; i++)
            {
                var from = iniPosY + i * deltaJump;
                var to = iniPosY + (i + 1) * deltaJump;

                jumpsSequence
                    .Chain(Tween.PositionY(target, Mathf.Max(from, to) + height, jumpDuration, jumpUpEase))
                    .Chain(Tween.PositionY(target, to, jumpDuration, jumpDownEase));
            }

            var result = Sequence.Create()
                .Group(jumpsSequence);

            if (!Mathf.Approximately(target.position.x, endValue.x))
            {
                result.Group(Tween.PositionX(target, endValue.x, duration, horizontalEase));
            }

            if (!Mathf.Approximately(target.position.z, endValue.z))
            {
                result.Group(Tween.PositionZ(target, endValue.z, duration, horizontalEase));
            }

            return result;
        }
    }
}