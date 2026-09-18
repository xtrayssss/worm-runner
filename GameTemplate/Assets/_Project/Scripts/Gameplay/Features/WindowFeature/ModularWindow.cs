using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    public class ModularWindow : BaseWindow
    {
        [SerializeField, FormerlySerializedAs("<SelfCanvasGroup>k__BackingField")]
        private CanvasGroup _selfCanvasGroup;

        public CanvasGroup SelfCanvasGroup => _selfCanvasGroup;

        public override UniTask ShowAsync()
        {
            base.ShowAsync();

            DisableInteraction();
            SetBlockRaycasts(true);

            return UniTask.CompletedTask;
        }

        public override UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            base.CloseAsync(externCancellationToken);

            DisableInteraction();

            return UniTask.CompletedTask;
        }

        protected void EnableInteraction() =>
            _selfCanvasGroup.interactable = true;

        protected void DisableInteraction() =>
            _selfCanvasGroup.interactable = false;

        protected void SetBlockRaycasts(bool value) =>
            _selfCanvasGroup.blocksRaycasts = value;
    }
}
