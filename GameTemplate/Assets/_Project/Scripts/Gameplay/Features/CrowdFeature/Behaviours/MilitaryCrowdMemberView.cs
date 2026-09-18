using System;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.LifeForceFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
#if ODIN_INSPECTOR
    [ShowOdinSerializedPropertiesInInspector]
#endif

    public sealed class MilitaryCrowdMemberView : CrowdMemberView, ISerializationCallbackReceiver,
        ISupportsPrefabSerialization
    {
        [SerializeField]
        private Transform _shootPoint;

        [NonSerialized]
        [OdinSerialize]
        private VFXData _muzzleFlashVFX;

        [SerializeField]
        private HealthBar _healthBar;

        [SerializeField]
        [HideInInspector]
        private SerializationData _serializationData;

        private VFXService _vfxService;
        private AudioService _audioService;

        SerializationData ISupportsPrefabSerialization.SerializationData
        {
            get => _serializationData;
            set => _serializationData = value;
        }

        public Transform ShootPoint => _shootPoint;
        public HealthBar HealthBar => _healthBar;

        public void Construct(VFXService vfxService, AudioService audioService)
        {
            _audioService = audioService;
            _vfxService = vfxService;
            HealthBar.Construct();
        }

        public void PlayShootEffect()
        {
            if (_muzzleFlashVFX != null)
            {
                _vfxService.PlayVFX(_muzzleFlashVFX, ShootPoint.position);

                _audioService.PlaySound3D(
                    AudioId.Sfx.Gameplay.RIFLE_SHOT,
                    position: _shootPoint.position,
                    randomization: new AudioService.AudioLibrary.SoundRandomizationSettings
                    {
                        PitchRange = new Vector2(0.9f, 1.1f),
                        VolumeRange = new Vector2(0.6f, 0.7f)
                    });
            }
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
    }
}