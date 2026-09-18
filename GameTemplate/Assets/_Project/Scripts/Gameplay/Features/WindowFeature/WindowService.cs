using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    public sealed class WindowService : IService
    {
#if DEBUG
        private const string LOG_PREFIX = "[WindowService]";
#endif
        [ShowInInspector]
        private readonly List<BaseWindow> _openWindows = new List<BaseWindow>();

        [ShowInInspector]
        private readonly List<Camera> _uiCameraStack = new List<Camera>();

        private readonly WindowFactory _windowFactory;
        private readonly UniversalAdditionalCameraData _mainCameraSettings;
        public event Action<BaseWindow> OnWindowCreated;

        public WindowService(WindowFactory windowFactory)
        {
            _windowFactory = windowFactory;
            _mainCameraSettings = Camera.main.GetUniversalAdditionalCameraData();
            _uiCameraStack.AddRange(_mainCameraSettings.cameraStack);
        }

        public TWindow OpenPersistentWindow<TWindow>(WindowId windowId, Transform parent = null)
            where TWindow : BaseWindow
        {
            if (IsWindowAlreadyOpen(windowId))
            {
#if DEBUG
                Debug.LogWarning(
                    $"{LOG_PREFIX} Cannot open window {windowId} - instance already exists in active windows list");
#endif
                return null;
            }

            BaseWindow window = CreateAndInitializeWindow<TWindow>(windowId, parent);
            _openWindows.Add(window);
            return (TWindow)window;
        }

        public void RegisterExternalWindow(BaseWindow window)
        {
            if (window == null)
            {
#if DEBUG
                Debug.LogWarning($"{LOG_PREFIX} Cannot register null window");
#endif
                return;
            }

            if (IsWindowAlreadyOpen(window.Id))
            {
#if DEBUG
                Debug.LogWarning(
                    $"{LOG_PREFIX} Cannot register window {window.Id} - instance already exists in active windows list");
#endif
                return;
            }

            ConfigureWindowCamera(window);
            _openWindows.Add(window);
        }

        public void RegisterTutorialPopup(BaseWindow window)
        {
            if (window == null)
            {
#if DEBUG
                Debug.LogWarning($"{LOG_PREFIX} Cannot register null window");
#endif
                return;
            }

            ConfigureWindowCamera(window);
            _openWindows.Add(window);
        }

        public void UnregisterExternalWindow(BaseWindow window)
        {
            if (window == null)
            {
#if DEBUG
                Debug.LogWarning($"{LOG_PREFIX} Cannot unregister null window");
#endif
                return;
            }

            if (!_openWindows.Contains(window))
            {
#if DEBUG
                Debug.LogWarning($"{LOG_PREFIX} Cannot unregister window - not found in active windows list");
#endif
                return;
            }

            RemoveCameraFromStack(window);
            _openWindows.Remove(window);
        }

        public TWindow OpenTemporaryPopup<TWindow>(WindowId windowId, Transform parent = null)
            where TWindow : BaseWindow =>
            (TWindow)CreateAndInitializeWindow<TWindow>(windowId, parent);

        public async UniTask CloseWindow(WindowId windowId, CancellationToken cancellationToken = default) =>
            await CloseWindowByPredicate(window => window.Id == windowId, cancellationToken);

        public async UniTask CloseTemporaryPopup(BaseWindow popupToClose)
        {
            if (popupToClose == null)
            {
#if DEBUG
                Debug.LogWarning(
                    $"{LOG_PREFIX} Cannot close null popup window");
#endif
                return;
            }

            await popupToClose.CloseAsync();
            CleanupWindowResources(popupToClose);
        }

        public async UniTask CloseWindows(params WindowId[] windowIds)
        {
            List<UniTask> closeTasks = new List<UniTask>();

            foreach (WindowId windowId in windowIds)
            {
                if (IsWindowAlreadyOpen(windowId))
                {
#if DEBUG
                    Debug.Log($"{LOG_PREFIX} Closing window {windowId}");
#endif
                    closeTasks.Add(CloseWindow(windowId));
                }
            }

            if (closeTasks.Count > 0)
            {
                await UniTask.WhenAll(closeTasks);
            }
        }

        public void CloseWindowsByCondition(Func<BaseWindow, bool> condition,
            CancellationToken cancellationToken = default) =>
            CloseWindowByPredicate(condition, cancellationToken).Forget();

        public TWindow GetOpenWindow<TWindow>(WindowId windowId) where TWindow : BaseWindow =>
            (TWindow)_openWindows.FirstOrDefault(window => window.Id == windowId);

        public void CloseAllWindows()
        {
            List<BaseWindow> windowsToClose = _openWindows.ToList();

            foreach (BaseWindow window in windowsToClose)
            {
                if (window.gameObject.scene.isLoaded)
                {
                    UnregisterExternalWindow(window);
                }
                else
                {
                    CloseWindow(window.Id).Forget();
                }
            }
        }

        public void CloseWindowsImmediate(params WindowId[] windowIds)
        {
            foreach (WindowId windowId in windowIds)
            {
                if (IsWindowAlreadyOpen(windowId))
                {
#if DEBUG
                    Debug.Log($"{LOG_PREFIX} Closing window {windowId} immediately");
#endif
                    CloseWindow(windowId).Forget();
                }
            }
        }

        public async UniTask CloseAllWindowsExcept(params WindowId[] exceptWindowIds)
        {
            HashSet<WindowId> exceptions = new HashSet<WindowId>(exceptWindowIds);
            List<UniTask> closeTasks = new List<UniTask>();

            foreach (BaseWindow window in _openWindows.ToList())
            {
                if (!exceptions.Contains(window.Id))
                {
#if DEBUG
                    Debug.Log($"{LOG_PREFIX} Closing window {window.Id} (not in exceptions)");
#endif
                    closeTasks.Add(CloseWindow(window.Id));
                }
            }

            if (closeTasks.Count > 0)
            {
                await UniTask.WhenAll(closeTasks);
            }
        }

        public void CloseAllWindowsExceptImmediate(params WindowId[] exceptWindowIds)
        {
            HashSet<WindowId> exceptions = new HashSet<WindowId>(exceptWindowIds);

            foreach (BaseWindow window in _openWindows.ToList())
            {
                if (!exceptions.Contains(window.Id))
                {
#if DEBUG
                    Debug.Log($"{LOG_PREFIX} Closing window {window.Id} immediately (not in exceptions)");
#endif
                    CloseWindow(window.Id).Forget();
                }
            }
        }

        private BaseWindow CreateAndInitializeWindow<TWindow>(WindowId windowId, Transform parent)
            where TWindow : BaseWindow
        {
            TWindow window = _windowFactory.CreateWindow<TWindow>(windowId, parent);
            ConfigureWindowCamera(window);
            OnWindowCreated?.Invoke(window);
            return window;
        }

        private async UniTask CloseWindowByPredicate(Func<BaseWindow, bool> condition,
            CancellationToken cancellationToken)
        {
            BaseWindow windowToClose = _openWindows.FirstOrDefault(condition);

            if (windowToClose == null)
            {
#if DEBUG
                Debug.LogWarning(
                    $"{LOG_PREFIX} Failed to close window - no matching window found in active windows list");
#endif
                return;
            }

            await windowToClose.CloseAsync(cancellationToken);
            CleanupWindowResources(windowToClose);
        }

        private void ConfigureWindowCamera(BaseWindow window)
        {
            if (!IsValidCameraSetup(window))
                return;

            if (IsCameraAlreadyInStack(window.AssociatedCamera))
            {
#if DEBUG
                Debug.LogWarning(
                    $"{LOG_PREFIX} Cannot add camera '{window.AssociatedCamera.name}' - already present in camera stack");
#endif
                return;
            }

            AddCameraToRenderStack(window.AssociatedCamera);
        }

        private void CleanupWindowResources(BaseWindow window)
        {
            RemoveCameraFromStack(window);
            _openWindows.Remove(window);
            Object.Destroy(window.gameObject);
        }

        private void AddCameraToRenderStack(Camera camera)
        {
            _uiCameraStack.Add(camera);

            camera.depth =
                CalculateCameraDepth(
                    priority: camera.TryGetComponent(out CameraPriority cameraPriority)
                        ? cameraPriority.Priority
                        : 0,
                    orderAdded: _uiCameraStack.Count);

            UpdateCameraStackOrder();
            SynchronizeCameraStack();
        }

        private void RemoveCameraFromStack(BaseWindow window)
        {
            if (!IsValidCameraSetup(window)) return;

            _uiCameraStack.Remove(window.AssociatedCamera);
            SynchronizeCameraStack();
        }

        private void UpdateCameraStackOrder()
        {
            _uiCameraStack.Sort(static (first, second) => first.depth.CompareTo(second.depth));
        }

        private void SynchronizeCameraStack()
        {
            _mainCameraSettings.cameraStack.Clear();
            _mainCameraSettings.cameraStack.AddRange(_uiCameraStack);
        }

        private static float CalculateCameraDepth(int priority, int orderAdded) =>
            orderAdded + priority * 1000;

        private bool IsValidCameraSetup(BaseWindow window) =>
            window.AssociatedCamera != null && _mainCameraSettings != null;

        private bool IsCameraAlreadyInStack(Camera camera) =>
            _uiCameraStack.Contains(camera);

        private bool IsWindowAlreadyOpen(WindowId windowId) =>
            _openWindows.Any(window => window.Id == windowId);
    }
}