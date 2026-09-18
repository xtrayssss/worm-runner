using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [CreateAssetMenu(fileName = nameof(InteractiveObjectConfig),
        menuName = ProjectConfig.PROJECT_NAME + "/Configs/" + nameof(InteractiveObjectConfig))]
    public class InteractiveObjectConfig : SerializedScriptableObject
    {
        [NonSerialized, OdinSerialize]
        [HideLabel]
        [HideReferenceObjectPicker]
        private InteractiveObjectData _interactiveObjectData = new InteractiveObjectData();

        public InteractiveObjectData InteractiveObjectData
        {
            get => _interactiveObjectData;
            set => _interactiveObjectData = value;
        }
    }
}
