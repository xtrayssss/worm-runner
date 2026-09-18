using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
    public sealed class FinishTriggerView : InteractiveObjectView
    {
        [SerializeField]
        private VFXData _confetti;

        private VFXService _vfxService;
        private UIRoot _uiRoot;

        public void Construct(VFXService vfxService, UIRoot uiRoot)
        {
            _uiRoot = uiRoot;
            _vfxService = vfxService;
        }

        public void PlayConfettiEffects()
        {
            _vfxService.PlayVFXUI(
                _confetti,
                _uiRoot.GameWindow.ContentRectTransform);
        }
    }
}