using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
#if ODIN_INSPECTOR
    [ShowOdinSerializedPropertiesInInspector]
#endif
    public sealed class EnemyGroupView : InteractiveObjectView, ISerializationCallbackReceiver,
        ISupportsPrefabSerialization
    {
        [Header("Enemy Group")]
        [NonSerialized]
        [OdinSerialize]
        private Dictionary<EnemyFormationType, Transform[]> _spawnPoints =
            new Dictionary<EnemyFormationType, Transform[]>();

        [SerializeField]
        [HideInInspector]
        private SerializationData _serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData
        {
            get => _serializationData;
            set => _serializationData = value;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (this.SafeIsUnityNull())
                return;

            UnitySerializationUtility.DeserializeUnityObject(this, ref _serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (this.SafeIsUnityNull())
                return;

            UnitySerializationUtility.SerializeUnityObject(this, ref _serializationData);
        }


        public Dictionary<EnemyFormationType, Transform[]> SpawnPoints => _spawnPoints;
    }
}