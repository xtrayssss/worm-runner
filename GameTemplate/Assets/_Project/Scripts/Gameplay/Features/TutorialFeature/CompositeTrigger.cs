using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    [MovedFrom(sourceAssembly: "Assembly-CSharp",
        sourceNamespace: "Project.Scripts.Gameplay.Features.TutorialFeature",
        sourceClassName: "CompositeTrigger", autoUpdateAPI: false)]
    public class CompositeTrigger : ITutorialTrigger
    {
        [SerializeReference] private List<ITutorialTrigger> _triggers = new List<ITutorialTrigger>();

        private int _triggersActivated;
        public event Action OnTriggerActivated;

        public void Initialize()
        {
            _triggersActivated = 0;

            foreach (ITutorialTrigger trigger in _triggers)
            {
                trigger.Initialize();
                trigger.OnTriggerActivated += HandleIndividualTrigger;
            }
        }

        public void Enable()
        {
            if (_triggers.Count == 0)
                return;

            _triggers.First().Enable();
        }

        public void Disable()
        {
            foreach (ITutorialTrigger trigger in _triggers)
            {
                trigger.Disable();
                trigger.OnTriggerActivated -= HandleIndividualTrigger;
            }
        }

        public void ConvertTargets()
        {
            foreach (ITutorialTrigger trigger in _triggers)
                trigger.ConvertTargets();
        }

        private void HandleIndividualTrigger()
        {
            if (_triggersActivated < _triggers.Count)
                _triggers[_triggersActivated].Disable();

            ++_triggersActivated;

            if (_triggersActivated < _triggers.Count)
                _triggers[_triggersActivated].Enable();

            if (_triggersActivated >= _triggers.Count)
                OnTriggerActivated?.Invoke();
        }
    }
}