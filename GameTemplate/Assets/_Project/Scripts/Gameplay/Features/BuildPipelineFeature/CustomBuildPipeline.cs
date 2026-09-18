using System.Collections.Generic;
using System.IO;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using Kamgam.ExcludeFromBuild;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.BuildPipelineFeature
{
    public sealed class CustomBuildPipeline
    {
        private const string DEV_BUILD_SYMBOL = "DEVELOPMENT_BUILD";

        [MenuItem("Build/Build Dev (WebGL)")]
        public static void BuildDevWebGL()
        {
            BuildWebGL(isDevelopment: true);
        }

        [MenuItem("Build/Build Release (WebGL)")]
        public static void BuildReleaseWebGL()
        {
            BuildWebGL(isDevelopment: false);
        }

        private static void BuildWebGL(bool isDevelopment)
        {
            //StartExcludeFromBuildTest();

            try
            {
                string buildPath = GetBuildPath(isDevelopment);
                const BuildTarget BUILD_TARGET = BuildTarget.WebGL;

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = GetEnabledScenes(),
                    locationPathName = buildPath,
                    target = BUILD_TARGET,
                    options = GetBuildOptions(isDevelopment)
                };

                ConfigureWebGLSettings(isDevelopment);

                L.Log($"Starting {(isDevelopment ? "Development" : "Release")} build...");
                L.Log($"Build path: {buildPath}");
                L.Log($"Decompression Fallback: {PlayerSettings.WebGL.decompressionFallback}");
                L.Log($"Compression Format: {PlayerSettings.WebGL.compressionFormat}");

                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    L.Log($"Build succeeded: {summary.totalSize} bytes");
                    L.Log($"Build time: {summary.totalTime}");
                    EditorUtility.RevealInFinder(buildPath);
                }
                else if (summary.result == BuildResult.Failed)
                {
                    L.LogError("Build failed!");
                }
            }
            finally
            {
                //StopExcludeFromBuildTest();
            }
        }

        private static void ConfigureWebGLSettings(bool isDevelopment)
        {
            if (isDevelopment)
            {
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            }
            else
            {
                PlayerSettings.WebGL.decompressionFallback = false;
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            }
        }

        private static BuildOptions GetBuildOptions(bool isDevelopment)
        {
            BuildOptions options = BuildOptions.None;

            if (isDevelopment)
            {
                options |= BuildOptions.Development;
                //options |= BuildOptions.AllowDebugging;
            }
            else
            {
                options |= BuildOptions.StrictMode;
            }

            return options;
        }

        private static string GetBuildPath(bool isDevelopment)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            string buildsFolder = Path.Combine(projectRoot, "Builds");

            if (!Directory.Exists(buildsFolder))
                Directory.CreateDirectory(buildsFolder);

            string buildType = isDevelopment ? "Dev" : "Release";
            string timestamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
            string buildFolderName = $"{buildType}_{timestamp}";

            string buildPath = Path.Combine(buildsFolder, buildFolderName);

            return buildPath;
        }

        private static string[] GetEnabledScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            List<string> enabledScenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.enabled)
                    enabledScenes.Add(scene.path);
            }

            return enabledScenes.ToArray();
        }

        private static void StartExcludeFromBuildTest() =>
            ExcludeFromBuildWindow.GetOrOpen().StartTest();

        private static void StopExcludeFromBuildTest() =>
            ExcludeFromBuildWindow.GetOrOpen().StopTest();
    }
}