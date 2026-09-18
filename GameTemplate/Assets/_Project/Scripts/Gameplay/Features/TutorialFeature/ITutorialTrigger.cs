using System;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public interface ITutorialTrigger
    {
        event Action OnTriggerActivated;
        void Initialize();
        void Enable();
        void Disable();
        void ConvertTargets();
    }
}