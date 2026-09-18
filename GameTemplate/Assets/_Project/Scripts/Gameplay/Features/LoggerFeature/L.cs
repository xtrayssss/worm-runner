using System.Diagnostics;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LoggerFeature
{
    public static class L
    {
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log($"[LOG] {message}");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context)
        {
            UnityEngine.Debug.Log($"[LOG] {message}", context);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning($"[WARNING] {message}");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context)
        {
            UnityEngine.Debug.LogWarning($"[WARNING] {message}", context);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError($"[ERROR] {message}");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context)
        {
            UnityEngine.Debug.LogError($"[ERROR] {message}", context);
        }
 
        public static void LogCritical(object message)
        {
            UnityEngine.Debug.LogError($"[CRITICAL] {message}");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat($"[LOG] {format}", args);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition, string message = "Assertion failed")
        {
            if (!condition)
            {
                UnityEngine.Debug.LogError($"[ASSERT] {message}");
            }
        }
    }
}