using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    public abstract class BaseWindow : SerializedMonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("<Content>k__BackingField")]
        private GameObject _content;

        [SerializeField]
        [FormerlySerializedAs("<Overlay>k__BackingField")]
        private GameObject _overlay;

        [SerializeField]
        [ReadOnly]
        [FormerlySerializedAs("<ContentRectTransform>k__BackingField")]
        private RectTransform _contentRectTransform;

        [SerializeField]
        [ReadOnly]
        [FormerlySerializedAs("<Canvas>k__BackingField")]
        private Canvas _canvas;

        [SerializeField]
        [ReadOnly]
        [FormerlySerializedAs("<CanvasRectTransform>k__BackingField")]
        private RectTransform _canvasRectTransform;

        [SerializeField]
        [ReadOnly]
        [FormerlySerializedAs("<Id>k__BackingField")]
        private WindowId _id;

        [SerializeField]
        [ReadOnly]
        [FormerlySerializedAs("<AssociatedCamera>k__BackingField")]
        private Camera _associatedCamera;

        [SerializeField]
        [ReadOnly]
        [FormerlySerializedAs("<IsOpen>k__BackingField")]
        private bool _isOpen;

        [SerializeField]
        [ReadOnly]
        [FormerlySerializedAs("<IsFullyOpen>k__BackingField")]
        private bool _isFullyOpen;

        [FormerlySerializedAs("StateController")]
        [SerializeField]
        [CanBeNull]
        private StateBasedUIController _stateController;

        public Canvas Canvas => _canvas;
        public RectTransform CanvasRectTransform => _canvasRectTransform;
        public WindowId Id => _id;
        public Camera AssociatedCamera => _associatedCamera;
        public bool IsOpen => _isOpen;
        public bool IsFullyOpen => _isFullyOpen;
        public GameObject Content => _content;
        public GameObject Overlay => _overlay;
        public RectTransform ContentRectTransform => _contentRectTransform;

        [Button]
        public virtual void Initialize(WindowId id)
        {
            _id = id;
            _associatedCamera = GetComponent<Camera>();
            _canvas = GetComponentInChildren<Canvas>();
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
            _canvas.worldCamera = _associatedCamera;
            _contentRectTransform = _content.GetComponent<RectTransform>();
#if DEBUG
            if (_associatedCamera == null)
                Debug.LogWarning($"[BaseWindow] No camera found for window {id}. Rendering will not be controlled!");
#endif
        }

        public virtual UniTask ShowAsync()
        {
            _isOpen = true;

            _canvas.enabled = true;

            return UniTask.CompletedTask;
        }

        public virtual UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            _isOpen = false;
            _isFullyOpen = false;

            return UniTask.CompletedTask;
        }

        [Button]
        public void Show() =>
            ShowAsync().Forget();

        [Button]
        public virtual void Close() =>
            CloseAsync().Forget();

        public void SetGameState(GameState newState) =>
            _stateController.SetState(newState);

        protected CancellationToken CreateLinkedCancellationToken(CancellationToken externalToken = default)
        {
            if (externalToken == CancellationToken.None || !externalToken.CanBeCanceled)
                return this.GetCancellationTokenOnDestroy();

            CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                externalToken,
                this.GetCancellationTokenOnDestroy()
            );

            this.GetCancellationTokenOnDestroy()
                .Register(static cts => ((CancellationTokenSource)cts).Dispose(), linkedCts);

            return linkedCts.Token;
        }
    }
}