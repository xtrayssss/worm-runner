using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
#if ODIN_INSPECTOR
    [ShowOdinSerializedPropertiesInInspector]
#endif
    public sealed class CommonCrowdMemberView : MonoBehaviour, ISerializationCallbackReceiver,
        ISupportsPrefabSerialization
    {
        [SerializeField]
        private Transform _visualRoot;

        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private Collider _collider;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private CharacterAnimator _animator;

        [SerializeField]
        private NestedFadeGroup.NestedFadeGroup _fadeGroup;

        [SerializeField]
        [HideInInspector]
        private SerializationData _serializationData;

        public Transform VisualRoot => _visualRoot;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public Collider Collider => _collider;
        public Rigidbody Rigidbody => _rigidbody;
        public CharacterAnimator Animator => _animator;
        public NestedFadeGroup.NestedFadeGroup FadeGroup => _fadeGroup;

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
    }
}