using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    public class WindowFactory : IService
    {
        private readonly Dictionary<WindowId, BaseWindow> _windowPrefabs;

        public WindowFactory(Dictionary<WindowId, BaseWindow> windowPrefabs)
        {
            _windowPrefabs = windowPrefabs;
        }

        public TWindow CreateWindow<TWindow>(WindowId windowId, Transform parent) where TWindow : BaseWindow
        {
            TWindow window = (TWindow)Object.Instantiate(_windowPrefabs[windowId], parent);

            window.Initialize(windowId);

            return window;
        }
    }
}