using System;
using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    public class TutorialStep
    {
        public bool UseCutoutMask;
        public ITutorialTrigger TriggerObject;
        public TutorialPopup Popup;
        public bool UseCustomPointer;
        public TutorialPointer Pointer;
        public RectTransform PopupTarget;
        public RectTransform[] HighlightTargets;
        public float WidthMultiplier;
        public float HeightMultiplier;
        public Vector2 PopupOffset;
        public bool UseCustomHighlightProperties;
        public float CustomMaskFadeDuration = 0.3f;
        public Ease CustomMaskFadeEase = Ease.OutCubic;
        public float CustomMaskEdgeSoftness = 0.03f;
        public TutorialStepTemplate.PopupPositionFlags PositionFlags;

        public float MaskFadeDuration => UseCustomHighlightProperties ? CustomMaskFadeDuration : 0.3f;
        public Ease MaskFadeEase => UseCustomHighlightProperties ? CustomMaskFadeEase : Ease.OutCubic;
        public float MaskEdgeSoftness => UseCustomHighlightProperties ? CustomMaskEdgeSoftness : 0.012f;
    }
}