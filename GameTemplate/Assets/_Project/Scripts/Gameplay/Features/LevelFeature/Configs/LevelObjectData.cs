using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Configs
{
    [Serializable]
    public class LevelObjectData
    {
        [HorizontalGroup("Header", Width = 30)]
        [HideLabel]
        [SerializeField]
        private bool _isActive = true;

        [HorizontalGroup("Header")]
        [FoldoutGroup("Header/$ObjectTitle", Expanded = false)]
        [SerializeField]
        private Vector3 _position;

        [FoldoutGroup("Header/$ObjectTitle")]
        [NonSerialized, OdinSerialize]
        [HideReferenceObjectPicker]
        [HideLabel]
        private InteractiveObjectData _customData = new InteractiveObjectData();

#if UNITY_EDITOR
        [HideInInspector, ReadOnly]
        public int RoadIndex;

        [HideInInspector]
        public InteractiveObjectView View;

        private string ObjectTitle => _customData.Title;
#endif
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public InteractiveObjectData CustomData
        {
            get => _customData;
            set => _customData = value;
        }

        public InteractiveObjectId ObjectType
        {
            get => _customData.Id;
            set => _customData.Id = value;
        }

        public ref Vector3 Position => ref _position;
    }
}