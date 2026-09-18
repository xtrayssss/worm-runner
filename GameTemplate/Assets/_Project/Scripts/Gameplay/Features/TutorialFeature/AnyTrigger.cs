using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    public class AnyTrigger : ITutorialTrigger
    {
        [SerializeReference] private List<ITutorialTrigger> _triggers = new List<ITutorialTrigger>();

        public event Action OnTriggerActivated;

        public void Initialize()
        {
            foreach (ITutorialTrigger trigger in _triggers)
            {
                trigger.Initialize();
                trigger.OnTriggerActivated += HandleIndividualTrigger;
            }
        }

        public void Enable()
        {
            foreach (ITutorialTrigger trigger in _triggers)
            {
                trigger.Enable();
            }
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
            foreach (ITutorialTrigger trigger in _triggers)
            {
                trigger.Disable();
                trigger.OnTriggerActivated -= HandleIndividualTrigger;
            }

            OnTriggerActivated?.Invoke();
        }
    }
}