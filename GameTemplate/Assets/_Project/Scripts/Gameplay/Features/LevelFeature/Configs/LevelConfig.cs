using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Configs
{
    [CreateAssetMenu(fileName = "Level_", menuName = ProjectConfig.PROJECT_NAME + "/Configs/Level/Level Config")]
    public class LevelConfig : SerializedScriptableObject
    {
        [TabGroup("Info")]
        [SerializeField]
        private LevelView _levelPrefab;

        [TabGroup("Info")]
        [SerializeField] 
        private LevelMode _levelMode;

        [TabGroup("Info")]
        [SerializeField]
        [ShowIf("LevelMode", LevelMode.DEFENSE)]
        private DefenseModeConfig _defenseModeConfig;

        [TabGroup("Objects")]
        [NonSerialized, OdinSerialize]
        [HideReferenceObjectPicker]
        [Searchable]
        private List<LevelObjectData> _levelObjects = new List<LevelObjectData>();
        
        [TabGroup("Mortar")]
        [SerializeField]
        private MortarStrikeData[] _mortarStrikes = Array.Empty<MortarStrikeData>();

        public MortarStrikeData[] MortarStrikes => _mortarStrikes;

        public List<LevelObjectData> LevelObjects
        {
            get => _levelObjects;
            set => _levelObjects = value;
        }

        public LevelView LevelPrefab
        {
            get => _levelPrefab;
            set => _levelPrefab = value;
        }
        
        public LevelMode LevelMode => _levelMode;
        
        public DefenseModeConfig DefenseModeConfig => _defenseModeConfig;

#if UNITY_EDITOR
        [TabGroup("Objects")]
        [Button("Enable All"), GUIColor(0.4f, 0.8f, 0.4f)]
        private void EnableAllObjects()
        {
            foreach (LevelObjectData obj in _levelObjects)
                obj.IsActive = true;
        }

        [TabGroup("Objects")]
        [Button("Disable All"), GUIColor(0.8f, 0.4f, 0.4f)]
        private void DisableAllObjects()
        {
            foreach (var obj in _levelObjects)
                obj.IsActive = false;
        }
#endif
    }
}