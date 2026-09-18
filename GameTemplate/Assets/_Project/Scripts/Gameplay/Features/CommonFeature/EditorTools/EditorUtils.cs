#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.EditorTools
{
    public static class EditorUtils
    {
        public static bool IsPrefab(this GameObject gameObject)
        {
            return gameObject.scene.name == gameObject.name ||
                   PrefabUtility.IsPartOfPrefabAsset(gameObject);
        }

        public static bool IsInPrefabMode(this GameObject gameObject) =>
            gameObject.scene.name == gameObject.name;

        public static bool IsInPrefabMode(this Component component) =>
            component.gameObject.IsInPrefabMode();

        public static bool IsPrefabInstance(this GameObject gameObject) =>
            PrefabUtility.IsPartOfPrefabInstance(gameObject);

        public static bool IsPrefabInstance(this Component component) =>
            component.gameObject.IsPrefabInstance();

        public static GameObject GetPrefabAsset(this GameObject gameObject) =>
            PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

        public static GameObject GetPrefabAsset(this Component component) =>
            component.gameObject.GetPrefabAsset();

        public static bool CanEdit(this GameObject gameObject) =>
            !gameObject.IsPrefab();

        public static bool CanEdit(this Component component) =>
            component.gameObject.CanEdit();

        public static void IfNotPrefab(this GameObject gameObject, Action action)
        {
            if (gameObject.CanEdit())
                action?.Invoke();
        }

        public static void IfNotPrefab(this Component component, Action action) =>
            component.gameObject.IfNotPrefab(action);

        public static void IfPrefab(this GameObject gameObject, Action action)
        {
            if (gameObject.IsPrefab())
                action?.Invoke();
        }

        public static void IfPrefab(this Component component, Action action) =>
            component.gameObject.IfPrefab(action);

        public static PrefabInstanceStatus GetPrefabStatus(this GameObject gameObject) =>
            PrefabUtility.GetPrefabInstanceStatus(gameObject);

        public static PrefabInstanceStatus GetPrefabStatus(this Component component) =>
            component.gameObject.GetPrefabStatus();
    }
}
#endif
