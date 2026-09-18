using _Project.Scripts.Gameplay.Features.TweenFeature;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    public sealed class ButtonPuncher : MonoBehaviour
    {
        [SerializeField] private float _scaleMultiplier = ButtonTweener.ButtonDefaults.Punch.PressScale.strength.x;
        [SerializeField] private float _animationDuration = ButtonTweener.ButtonDefaults.Punch.PressScale.duration;

        private EventTrigger.Entry _pointerUp;
        private EventTrigger.Entry _pointerDown;

        private void OnEnable()
        {
            EventTrigger eventTrigger =
                gameObject.GetComponent<EventTrigger>() ?? gameObject.AddComponent<EventTrigger>();

            _pointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };

            _pointerDown.callback.AddListener(AnimateButton);
            eventTrigger.triggers.Add(_pointerDown);
        }

        private void OnDisable() =>
            _pointerDown.callback.RemoveAllListeners();

        private void AnimateButton(BaseEventData _)
        {
            ButtonTweener.AnimateButtonPunch(
                transform,
                new ShakeSettings
                {
                    duration = _animationDuration,
                    asymmetry = ButtonTweener.ButtonDefaults.Punch.PressScale.asymmetry,
                    frequency = ButtonTweener.ButtonDefaults.Punch.PressScale.frequency,
                    cycles = ButtonTweener.ButtonDefaults.Punch.PressScale.cycles,
                    strength = Vector3.one * _scaleMultiplier,
                    enableFalloff = ButtonTweener.ButtonDefaults.Punch.PressScale.enableFalloff,
                    useUnscaledTime = true,
                    easeBetweenShakes = ButtonTweener.ButtonDefaults.Punch.PressScale.easeBetweenShakes,
                    falloffEase = ButtonTweener.ButtonDefaults.Punch.PressScale.falloffEase,
                });
        }
    }
}