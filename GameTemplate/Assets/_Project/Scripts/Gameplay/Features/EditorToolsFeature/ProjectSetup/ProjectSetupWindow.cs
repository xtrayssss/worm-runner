using System.IO;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using SingularityGroup.HotReload.Editor;
using SingularityGroup.HotReload.Editor.Cli;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.EditorToolsFeature.ProjectSetup
{
    public class ProjectSetupWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Project Setup")]
        private static void OpenWindow()
        {
            GetWindow<ProjectSetupWindow>("Project Setup");
        }

        [BoxGroup("Project Settings")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Project Name (PlayerSettings)")]
        private string CurrentProductName => PlayerSettings.productName;

        [BoxGroup("Project Settings")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current PROJECT_NAME Constant")]
        private string CurrentProjectNameConstant => ProjectConfig.PROJECT_NAME;

        [Button("Setup Project", ButtonSizes.Large)]
        private void SetupProject()
        {
            SyncProjectName();
            SetupHotReload();
        }

        [Button("Setup Hot Reload", ButtonSizes.Large)]
        private static void SetupHotReload()
        {
            HotReloadPrefs.AllowDisableUnityAutoRefresh = true;
            HotReloadPrefs.AllAssetChanges = true;
            HotReloadPrefs.AutoRecompileUnsupportedChanges = false;
            HotReloadPrefs.AutoRecompileUnsupportedChangesInPlayMode = false;
            HotReloadPrefs.AutoRecompileUnsupportedChangesOnExitPlayMode = false;
            HotReloadPrefs.AutoRecompilePartiallyUnsupportedChanges = false;
            HotReloadPrefs.DisplayNewMonobehaviourMethodsAsPartiallySupported = false;
            HotReloadPrefs.IncludeShaderChanges = false;
        }

        private void SyncProjectName()
        {
            string newProjectName = PlayerSettings.productName;
            
            if (string.IsNullOrEmpty(newProjectName))   
                return;

            if (newProjectName == ProjectConfig.PROJECT_NAME)
                return;

            UpdateProjectConfigFile(newProjectName);
        }

        private void UpdateProjectConfigFile(string newProjectName)
        {
            string filePath = "Assets/_Project/Scripts/Gameplay/Features/CommonFeature/Services/ProjectConfig.cs";
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[ProjectSetupWindow] ProjectConfig.cs file not found at path: {filePath}");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("PROJECT_NAME"))
                {
                    lines[i] = $"        public const string PROJECT_NAME = \"{newProjectName}\";";
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);
            AssetDatabase.Refresh();
        }
    }
}