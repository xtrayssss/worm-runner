using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CollisionFeature.Behaviours
{
    public sealed class CollisionSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        [SerializeField] private bool _handleCollisions = true;

        [SerializeField] private bool _handleTriggers;
        [SerializeField] private bool _handleTriggerStay;

        private EntityView _entityView;
        private World _world;

        [ShowInInspector]
        private readonly List<ActiveCollision.CollisionInfo> _collisions =
            new List<ActiveCollision.CollisionInfo>(capacity: 4);

        [ShowInInspector]
        private readonly List<ActiveTrigger.TriggerInfo> _triggers = new List<ActiveTrigger.TriggerInfo>(capacity: 4);

        [ShowInInspector]
        private readonly List<UnifiedCollision.UnifiedCollisionInfo> _unifiedContacts =
            new List<UnifiedCollision.UnifiedCollisionInfo>(capacity: 8);

        private void Start()
        {
            _world = World.Default;
            _entityView = GetComponentInParent<EntityView>();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!_handleCollisions)
                return;

            Entity source = _entityView.Entity;

            if (source.IsNullOrDisposed())
                return;

            if (!source.Has<ActiveCollision>())
                source.AddComponent<ActiveCollision>();

            ref ActiveCollision activeCollision = ref source.GetComponent<ActiveCollision>();

            EntityView otherEntityView = other.gameObject.GetComponentInParent<EntityView>();
            Vector3 contactPoint = other.contacts.Length > 0 ? other.contacts[0].point : other.transform.position;
            Vector3 contactNormal = other.contacts.Length > 0 ? other.contacts[0].normal : Vector3.up;

            AddUnifiedContact(source, otherEntityView, contactPoint, UnifiedCollision.CollisionType.COLLISION);

            _collisions.Add(new ActiveCollision.CollisionInfo
            {
                Other = otherEntityView,
                Point = contactPoint
            });

            activeCollision.Collisions = _collisions;

            _world
                .GetEvent<CollisionEvent>()
                .NextFrame(new CollisionEvent
                {
                    Source = _entityView,
                    Other = otherEntityView,
                    Point = contactPoint,
                    Normal = contactNormal
                });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_handleTriggers)
                return;

            Entity source = _entityView.Entity;

            if (source.IsNullOrDisposed())
                return;

            if (!source.Has<ActiveTrigger>())
                source.AddComponent<ActiveTrigger>();

            ref ActiveTrigger activeTrigger = ref source.GetComponent<ActiveTrigger>();

            EntityView otherEntityView = other.gameObject.GetComponentInParent<EntityView>();

            Vector3 contactPoint = other.bounds.center;

            AddUnifiedContact(source, otherEntityView, contactPoint, UnifiedCollision.CollisionType.TRIGGER);

            _triggers.Add(new ActiveTrigger.TriggerInfo
            {
                Other = otherEntityView,
                Point = contactPoint
            });

            activeTrigger.Triggers = _triggers;

            _world
                .GetEvent<TriggerEnterEvent>()
                .NextFrame(new TriggerEnterEvent
                {
                    Source = _entityView,
                    Other = otherEntityView,
                    Point = contactPoint
                });
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_handleTriggers || !_handleTriggerStay)
                return;

            Entity source = _entityView.Entity;
            if (source.IsNullOrDisposed())
                return;

            EntityView otherEntityView = other.gameObject.GetComponent<EntityView>();

            _world
                .GetEvent<TriggerStayEvent>()
                .NextFrame(new TriggerStayEvent
                {
                    Source = _entityView,
                    Other = otherEntityView,
                    Point = other.bounds.center
                });
        }

        private void OnDisable()
        {
            _collisions.Clear();
            _triggers.Clear();
            _unifiedContacts.Clear();
        }

        private void AddUnifiedContact(
            Entity source,
            EntityView other,
            Vector3 point,
            UnifiedCollision.CollisionType type)
        {
            if (!source.Has<UnifiedCollision>())
                source.AddComponent<UnifiedCollision>();

            ref UnifiedCollision unifiedCollision = ref source.GetComponent<UnifiedCollision>();

            _unifiedContacts.Add(new UnifiedCollision.UnifiedCollisionInfo
            {
                Other = other,
                Point = point,
                Type = type
            });

            unifiedCollision.Contacts = _unifiedContacts;
            unifiedCollision.ContactCount = _unifiedContacts.Count;
        }
    }
}