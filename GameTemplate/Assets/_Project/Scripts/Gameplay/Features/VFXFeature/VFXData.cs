using System;
using System.Collections;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.VFXFeature
{
    [Serializable]
    public sealed class VFXData
    {
        [SerializeField]
        private GameObject _vfxPrefab;
        
        [SerializeField]
        private float _vfxDuration = 2f;

        [SerializeField]
        private Vector3 _spawnOffset = Vector3.up * 0.5f;

        [SerializeField]
        private bool _followTarget;

        [SerializeField]
        private float _delay;

        [NonSerialized, OdinSerialize]
        private Vector3? _customScale;

        [NonSerialized, OdinSerialize]
        private Vector3? _customRotation;
        
        [SerializeField]
        [BoxGroup("Sound")]
        [ValueDropdown("GetAudioIdDropdown")]
        private string _soundId;
        
        [SerializeField]
        [BoxGroup("Sound")]
        private bool _is3D;
        
        public GameObject VFXPrefab => _vfxPrefab;
        public string SoundId => _soundId;
        public float VFXDuration => _vfxDuration;
        public Vector3 SpawnOffset => _spawnOffset;
        public bool FollowTarget => _followTarget;
        public float Delay => _delay;
        public bool Is3D => _is3D;
        public Vector3? CustomScale => _customScale;
        public Vector3? CustomRotation => _customRotation;

#if UNITY_EDITOR
        private static IEnumerable GetAudioIdDropdown() =>
            AudioService.AudioLibrary.AudioFile.GetAudioIdDropdown();
#endif
    }
}