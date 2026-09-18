using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.WindowFeature.Configs
{
    [CreateAssetMenu(fileName = nameof(WindowsConfig),
        menuName = ProjectConfig.PROJECT_NAME + "/Configs/" + nameof(WindowsConfig))]
    public class WindowsConfig : ScriptableObject
    {
        [SerializeField] private List<WindowConfig> _windowConfigs;
        public List<WindowConfig> WindowConfigs => _windowConfigs;
    }
}