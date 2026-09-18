using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    [MovedFrom(sourceAssembly: "Assembly-CSharp",
        sourceNamespace: "Project.Scripts.Gameplay.Features.TutorialFeature",
        sourceClassName: "ButtonTrigger", autoUpdateAPI: false)]
    public sealed class ButtonTrigger : ITutorialTrigger
    {
        [SerializeField]
        private TutorialStepTemplate.ElementReference _buttonKey;

        [SerializeField]
        [HideInEditorMode]
        private Button _targetButton;

        public event Action OnTriggerActivated;

        public void Initialize()
        {
        }

        public void ConvertTargets() =>
            _targetButton = TutorialUtils.GetReferenceTransform(_buttonKey)?.GetComponent<Button>();

        public void Enable() =>
            _targetButton.onClick.AddListener(HandleButtonClick);

        public void Disable() =>
            _targetButton.onClick.RemoveListener(HandleButtonClick);

        private void HandleButtonClick() =>
            OnTriggerActivated?.Invoke();
    }
}