using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting.APIUpdating;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    [MovedFrom(sourceAssembly: "Assembly-CSharp",
        sourceNamespace: "Project.Scripts.Gameplay.Features.TutorialFeature",
        sourceClassName: "TouchTrigger", autoUpdateAPI: false)]
    public sealed class TouchTrigger : ITutorialTrigger
    {
        [SerializeField] private GameObject _targetObject;
        private EventTrigger _eventTrigger;
        public event Action OnTriggerActivated;

        public void Initialize()
        {
            _eventTrigger = _targetObject.GetComponent<EventTrigger>();

            if (_eventTrigger == null)
                _eventTrigger = _targetObject.AddComponent<EventTrigger>();
        }

        public void Enable()
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };

            entry.callback.AddListener(data => HandlePointerDown((PointerEventData)data));

            _eventTrigger.triggers.Add(entry);
        }

        public void Disable()
        {
            _eventTrigger.triggers.Clear();
        }

        public void ConvertTargets()
        {
            
        }

        private void HandlePointerDown(PointerEventData eventData)
        {
            OnTriggerActivated?.Invoke();
        }
    }
}