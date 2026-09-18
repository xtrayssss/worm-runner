using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public static class TutorialRegistry
    {
        private static readonly Dictionary<string, RectTransform> ELEMENTS = new Dictionary<string, RectTransform>();

        public static IReadOnlyDictionary<string, RectTransform> Elements => ELEMENTS;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Clear() => ELEMENTS.Clear();
#endif

        public static void Register(string key, RectTransform element)
        {
            if (!ELEMENTS.TryAdd(key, element))
            {
#if DEBUG
                Debug.LogWarning($"Element with key '{key}' already registered!");
#endif
            }
        }

        public static void Unregister(string key)
        {
            if (ELEMENTS.ContainsKey(key))
                ELEMENTS.Remove(key);
        }

        public static RectTransform GetElement(string key)
        {
            if (ELEMENTS.TryGetValue(key, out RectTransform element))
                return element;

#if DEBUG
            Debug.LogError($"Element with key '{key}' not found!");
#endif

            return null;
        }
    }
}