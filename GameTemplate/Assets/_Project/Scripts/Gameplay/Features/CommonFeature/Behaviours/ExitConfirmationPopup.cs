using System.Threading;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    public sealed class ExitConfirmationPopup : ModularWindow
    {
        [field: SerializeField] public Button CloseButton { get; private set; }
        [field: SerializeField] public Button ConfirmButton { get; private set; }
        [SerializeField] private TextMeshProUGUI _messageText;

        public delegate void OnConfirmationResponse(bool confirmed);

        private OnConfirmationResponse _onResponseCallback;

        public void Construct(string message, OnConfirmationResponse callback)
        {
            _messageText.text = message;
            _onResponseCallback = callback;

            CloseButton.onClick.AddListener(OnCloseButtonClicked);
            ConfirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        private void OnDestroy()
        {
            CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
            ConfirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        }

        public override UniTask ShowAsync()
        {
            base.ShowAsync();

            WindowTweener
                .ShowFadeScaleSlideWindow(
                    ContentRectTransform,
                    canvasGroup: SelfCanvasGroup,
                    useUnscaledTime: true)
                .OnComplete(this, static window => window.EnableInteraction());

            return UniTask.CompletedTask;
        }

        public override async UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            CancellationToken linkedToken = CreateLinkedCancellationToken(externCancellationToken);
            
            base.CloseAsync(linkedToken);

            await WindowTweener
                .HideFadeScaleSlideWindow(
                    ContentRectTransform,
                    canvasGroup: SelfCanvasGroup,
                    useUnscaledTime: true)
                .OnComplete(this, static window => window.EnableInteraction())
                .AsTask(linkedToken);
        }

        private void OnCloseButtonClicked() =>
            _onResponseCallback?.Invoke(confirmed: false);

        private void OnConfirmButtonClicked() =>
            _onResponseCallback?.Invoke(confirmed: true);
    }
}