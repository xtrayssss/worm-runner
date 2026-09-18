using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    public class TutorialStepTemplate
    {
        [Flags]
        public enum PopupPositionFlags
        {
            None = 0,
            PositionX = 1 << 0,
            PositionY = 1 << 1,
            ResizeWidth = 1 << 2,
            ResizeHeight = 1 << 3,

            PositionOnly = PositionX | PositionY,
            ResizeOnly = ResizeWidth | ResizeHeight,
            Full = PositionX | PositionY | ResizeWidth | ResizeHeight
        }

#if UNITY_EDITOR
        [HideLabel]
        [FoldoutGroup("$FoldoutTitle")]
        [PropertyOrder(-1)]
        public bool enabled = true;
#endif
        [FoldoutGroup("$FoldoutTitle")]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        public string StepID;

        [FoldoutGroup("$FoldoutTitle")]
        public TutorialPhase Phase = TutorialPhase.NONE;

        [TabGroup("$FoldoutTitle/Settings", "Popup", SdfIconType.Window)]
        [TitleGroup("$FoldoutTitle/Settings/Popup/Main")]
        public TutorialPopup Popup;

        [TabGroup("$FoldoutTitle/Settings", "Popup")]
        [TitleGroup("$FoldoutTitle/Settings/Popup/Positioning")]
        [EnumToggleButtons]
        public PopupPositionFlags PositionFlags = PopupPositionFlags.None;

        [TabGroup("$FoldoutTitle/Settings", "Popup")]
        [TitleGroup("$FoldoutTitle/Settings/Popup/Positioning")]
        [ShowIf("HasAnyPositionFlag")]
        [InlineProperty]
        [HideLabel]
        public ElementReference PopupTarget;

        [TabGroup("$FoldoutTitle/Settings", "Popup")]
        [TitleGroup("$FoldoutTitle/Settings/Popup/Positioning")]
        [ShowIf("ShouldShowWidthMultiplier")]
        [LabelWidth(120)]
        [MinValue(0.1f)]
        public float WidthMultiplier = 1.5f;

        [TabGroup("$FoldoutTitle/Settings", "Popup")]
        [TitleGroup("$FoldoutTitle/Settings/Popup/Positioning")]
        [ShowIf("ShouldShowHeightMultiplier")]
        [LabelWidth(120)]
        [MinValue(0.1f)]
        public float HeightMultiplier = 1.2f;

        [TabGroup("$FoldoutTitle/Settings", "Popup")]
        [TitleGroup("$FoldoutTitle/Settings/Popup/Positioning")]
        [ShowIf("ShouldShowOffset")]
        public Vector2 PopupOffset = new Vector2(0, 200f);

        [TabGroup("$FoldoutTitle/Settings", "Highlight", SdfIconType.Lightning)]
        [TitleGroup("$FoldoutTitle/Settings/Highlight/Main")]
        [ToggleLeft]
        public bool UseCutoutMask;

        [TabGroup("$FoldoutTitle/Settings", "Highlight")]
        [TitleGroup("$FoldoutTitle/Settings/Highlight/Main")]
        [ShowIf("UseCutoutMask")]
        [ListDrawerSettings(NumberOfItemsPerPage = 3)]
        public List<ElementReference> HighlightTargets = new List<ElementReference>();

        [TabGroup("$FoldoutTitle/Settings", "Highlight")]
        [TitleGroup("$FoldoutTitle/Settings/Highlight/Advanced")]
        [ShowIf("UseCutoutMask")]
        [ToggleLeft]
        public bool UseCustomHighlightProperties;

        [TabGroup("$FoldoutTitle/Settings", "Highlight")]
        [TitleGroup("$FoldoutTitle/Settings/Highlight/Advanced")]
        [ShowIf("@UseCutoutMask && UseCustomHighlightProperties")]
        [HorizontalGroup("$FoldoutTitle/Settings/Highlight/Advanced/Timing")]
        [SuffixLabel("seconds")]
        [Range(0.1f, 2f)]
        public float CustomMaskFadeDuration = 0.3f;

        [TabGroup("$FoldoutTitle/Settings", "Highlight")]
        [TitleGroup("$FoldoutTitle/Settings/Highlight/Advanced")]
        [ShowIf("@UseCutoutMask && UseCustomHighlightProperties")]
        [HorizontalGroup("$FoldoutTitle/Settings/Highlight/Advanced/Timing")]
        public Ease CustomMaskFadeEase = Ease.OutCubic;

        [TabGroup("$FoldoutTitle/Settings", "Highlight")]
        [TitleGroup("$FoldoutTitle/Settings/Highlight/Advanced")]
        [ShowIf("@UseCutoutMask && UseCustomHighlightProperties")]
        [HorizontalGroup("$FoldoutTitle/Settings/Highlight/Advanced/Mask")]
        [Range(0.01f, 0.1f)]
        public float CustomMaskEdgeSoftness = 0.03f;

        [TabGroup("$FoldoutTitle/Settings", "Pointer", SdfIconType.Cursor)]
        [TitleGroup("$FoldoutTitle/Settings/Pointer/Main")]
        [ToggleLeft]
        public bool UseCustomPointer;

        [TabGroup("$FoldoutTitle/Settings", "Pointer")]
        [TitleGroup("$FoldoutTitle/Settings/Pointer/Config")]
        [ShowIf("UseCustomPointer")]
        public TutorialPointer Pointer;

        [TabGroup("$FoldoutTitle/Settings", "Trigger", SdfIconType.ToggleOn)]
        [TitleGroup("$FoldoutTitle/Settings/Trigger/Main")]
        [InlineProperty]
        [HideLabel]
        [SerializeReference]
        public ITutorialTrigger Trigger;

        [Serializable]
        public class ElementReference
        {
            public enum SourceType
            {
                Scene,
                Prefab
            }

            [HorizontalGroup("Source", Width = 0.3f)]
            [LabelWidth(50)]
            public SourceType Source;

            [HorizontalGroup("Source")]
            [ShowIf("Source", SourceType.Prefab)]
            [LabelWidth(50)]
            [AssetSelector(Filter = "t:Prefab")]
            public GameObject PrefabAsset;

            [HorizontalGroup("Source")]
            [ValueDropdown("GetElementKeys")]
            public string ElementKey;

#if UNITY_EDITOR
            private IEnumerable<string> GetSceneElements()
            {
                return Object.FindObjectsOfType<TutorialDynamicElement>(true)
                    .Where(static x => !string.IsNullOrEmpty(x.ElementKey))
                    .Select(static x => x.ElementKey);
            }

            private IEnumerable<string> GetElementKeys()
            {
                IEnumerable<string> keys = new List<string>();

                if (Source == SourceType.Scene)
                {
                    keys = GetSceneElements();
                }

                if (PrefabAsset != null)
                {
                    keys = PrefabAsset.GetComponentsInChildren<TutorialDynamicElement>(true)
                        .Where(static x => !string.IsNullOrEmpty(x.ElementKey))
                        .Select(static x => x.ElementKey);
                }

                return keys.Concat(TutorialElementKeys.GetAllKeys());
            }
#endif
        }


#if UNITY_EDITOR
        private string FoldoutTitle(TutorialService root)
        {
            foreach (TutorialTemplate t in root.Templates)
            {
                int index = t.Steps.IndexOf(this);

                if (index != -1)
                {
                    if (Phase != TutorialPhase.NONE)
                        return $"{Phase}-({GetStepName(index)})";

                    return GetStepName(index);
                }
            }

            return "Step";

            string GetStepName(int index)
            {
                return string.IsNullOrEmpty(StepID) ? $"Step {index + 1}" : $"{StepID} (Step {index + 1})";
            }
        }

        private bool HasPositionFlag(PopupPositionFlags flag)
        {
            return (PositionFlags & flag) != 0;
        }

        private bool HasAnyPositionFlag()
        {
            return PositionFlags != PopupPositionFlags.None;
        }

        private bool HasValidTarget()
        {
            return PopupTarget != null && !string.IsNullOrEmpty(PopupTarget.ElementKey);
        }

        private bool ShouldShowWidthMultiplier()
        {
            return HasPositionFlag(PopupPositionFlags.ResizeWidth) && HasValidTarget();
        }

        private bool ShouldShowHeightMultiplier()
        {
            return HasPositionFlag(PopupPositionFlags.ResizeHeight) && HasValidTarget();
        }

        private bool ShouldShowOffset() =>
            HasValidTarget();
#endif
    }
}