using System.Threading;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public sealed class TutorialPopup : ModularWindow
    {
        public RectTransform RectTransform;

        public void Construct()
        {
            RectTransform = GetComponent<RectTransform>();
            Initialize(WindowId.TUTORIAL_POPUP);
        }

        public override UniTask ShowAsync()
        {
            base.ShowAsync();

            Canvas.enabled = true;

            WindowTweener
                .ShowShrinkFadeWindow(ContentRectTransform, canvasGroup: SelfCanvasGroup, useUnscaledTime: true)
                .OnComplete(this, static window => window.EnableInteraction());

            return UniTask.CompletedTask;
        }

        public override async UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            CancellationToken linkedToken = CreateLinkedCancellationToken(externCancellationToken);
            
            base.CloseAsync(linkedToken);

            Canvas.enabled = false;

            await WindowTweener.HideShrinkFadeWindow(
                    ContentRectTransform,
                    canvasGroup: SelfCanvasGroup,
                    useUnscaledTime: true)
                .OnComplete(this, static window => window.EnableInteraction())
                .AsTask(linkedToken);
        }
    }
}