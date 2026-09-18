using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record CollectibleConfig : BaseInteractiveObjectConfig<CollectibleConfig>
    {
        [NonSerialized]
        [OdinSerialize]
        [ShowIf("IsStandaloneConfig")]
        private Dictionary<CollectibleType, CollectibleView> _collectibleByType;

        [SerializeField]
        private CollectibleType _type;

        public CollectibleType Type
        {
            get => _type;
            set => _type = value;
        }

        public Dictionary<CollectibleType, CollectibleView> CollectibleByType => BaseConfig._collectibleByType;

#if UNITY_EDITOR
        private bool IsStandaloneConfig(Object root) =>
            root is InteractiveObjectConfig;
#endif
    }
}