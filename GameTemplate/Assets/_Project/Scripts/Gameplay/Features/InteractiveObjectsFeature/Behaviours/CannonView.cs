using System;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
#if ODIN_INSPECTOR
    [ShowOdinSerializedPropertiesInInspector]
#endif
    public sealed class CannonView : InteractiveObjectView, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [Header("Cannon Specific")]
        [SerializeField]
        private Transform _shootPoint;

        [NonSerialized, OdinSerialize]
        private VFXData _muzzleFlashVFX;

        private VFXService _vfxService;

        [SerializeField, HideInInspector]
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

        public Transform ShootPoint => _shootPoint;

        public void Construct(VFXService vfxService) =>
            _vfxService = vfxService;

        public void PlayFireEffect()
        {
            if (_muzzleFlashVFX != null)
                _vfxService.PlayVFX(_muzzleFlashVFX, ShootPoint.position, scale: 1.5f);
        }
    }
}