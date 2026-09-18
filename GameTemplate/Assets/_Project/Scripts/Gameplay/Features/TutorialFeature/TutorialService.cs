using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using _Project.Scripts.Gameplay.Features.TutorialFeature.CutoutMask.Scripts;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public sealed class TutorialService : BaseWindow, IService
    {
        [SerializeField]
        private List<TutorialTemplate> _templates = new List<TutorialTemplate>();

        [SerializeField]
        private RectTransform _pointerContainer;

        private WindowService _windowService;
        private TutorialFadeImage _cutoutMask;
        private SaveLoadService _saveLoadService;
        private TutorialPhaseCallbacks _phaseCallbacks;

        private readonly Dictionary<string, Tutorial> _activeTutorials = new Dictionary<string, Tutorial>();
        private HashSet<string> _completedTemplateIds = new HashSet<string>();
        private Dictionary<string, HashSet<int>> _completedStepsByTemplate = new Dictionary<string, HashSet<int>>();
        private TutorialTemplate _mainTutorialTemplate;
        public event Action<string> OnTutorialCompleted;
        public event Action<string, int> OnTutorialStepCompleted;

        public List<TutorialTemplate> Templates => _templates;
        public RectTransform PointerContainer => _pointerContainer;

        public void Initialize(WindowService windowService, SaveLoadService saveLoadService)
        {
            _windowService = windowService;
            _mainTutorialTemplate = _templates[0];
            base.Initialize(WindowId.TUTORIAL);
            windowService.RegisterExternalWindow(this);
            _cutoutMask = GetComponentInChildren<TutorialFadeImage>(includeInactive: true);
            _saveLoadService = saveLoadService;
            _phaseCallbacks = new TutorialPhaseCallbacks();
            LoadTutorialProgress();
        }

        public void RegisterPhaseCallbacks(TutorialPhase phase, Action onStart = null, Action onEnd = null)
        {
            _phaseCallbacks.RegisterPhaseCallbacks(phase, onStart, onEnd);
        }

        public void UnregisterPhaseCallbacks(TutorialPhase phase, Action onStart = null, Action onEnd = null)
        {
            _phaseCallbacks.UnregisterPhaseCallbacks(phase, onStart, onEnd);
        }

        public bool InitiateTutorialIfNeeded(TutorialPhase phase, string templateId = null)
        {
            TutorialTemplate template = GetTemplateForPhase(phase, templateId);

            if (template == null)
                return false;

            if (IsTutorialCompleted(template.Id))
            {
#if DEBUG
                Debug.Log($"Tutorial already completed: {template.Id}");
#endif
                return false;
            }

            if (!HasTutorialEverStarted())
            {
#if DEBUG
                Debug.Log($"Starting new tutorial from phase: {phase}");
#endif
                StartTutorialFromPhase(phase, template);
                return true;
            }

            if (!IsPhaseCompleted(phase))
            {
#if DEBUG
                Debug.Log($"Resuming tutorial from incomplete phase: {phase}");
#endif
                ContinueTutorialFromPhase(phase, template);
                return true;
            }

#if DEBUG
            Debug.Log($"Phase {phase} already completed, no need to start tutorial");
#endif
            return false;
        }

        public bool RestartPhase(TutorialPhase phase, string templateId = null)
        {
            TutorialTemplate template = GetTemplateForPhase(phase, templateId);
            if (template == null) return false;

            if (template.Steps.All(s => s.Phase != phase))
            {
#if DEBUG
                Debug.LogWarning($"No steps found for phase {phase} in template {template.Id}");
#endif
                return false;
            }

            if (_activeTutorials.TryGetValue(template.Id, out Tutorial existingTutorial))
            {
                existingTutorial.End();
                _activeTutorials.Remove(template.Id);
            }

            Tutorial newTutorial = new Tutorial(template, this);
            _activeTutorials[template.Id] = newTutorial;
            newTutorial.RestartFromPhase(phase);

            return true;
        }

        public bool IsTutorialActive()
        {
            return _activeTutorials.Values.Any(t => t.IsActive);
        }

        public bool IsCurrentActiveStep(string templateId, int stepIndex)
        {
            return _activeTutorials.TryGetValue(templateId, out Tutorial tutorial) &&
                   tutorial.IsCurrentActiveStep(stepIndex);
        }

        public bool IsPhaseActive(TutorialPhase phase)
        {
            return _activeTutorials.Values.Any(t => t.IsPhaseActive(phase));
        }

        public string GetActiveTutorialId()
        {
            return _activeTutorials.Values.FirstOrDefault(t => t.IsActive)?.TemplateId;
        }

        public int GetActiveStepIndex()
        {
            return _activeTutorials.Values.FirstOrDefault(t => t.IsActive)?.CurrentStepIndex ?? -1;
        }

        public bool IsPhaseCompleted(TutorialPhase phase)
        {
            foreach (TutorialTemplate template in _templates)
            {
                var phaseSteps = template.Steps
                    .Select(static (step, index) => new { Step = step, Index = index })
                    .Where(item => item.Step.Phase == phase)
                    .ToList();

                if (phaseSteps.Count == 0)
                    continue;

                return phaseSteps.All(item => IsStepCompleted(template.Id, item.Index));
            }

            return false;
        }

        public bool IsStepCompleted(string templateId, int stepIndex)
        {
            return _completedStepsByTemplate.TryGetValue(templateId, out HashSet<int> completedSteps) &&
                   completedSteps.Contains(stepIndex);
        }

        public bool IsTutorialCompleted([CanBeNull] string templateId = null) => 
            _completedTemplateIds.Contains(templateId ?? _mainTutorialTemplate.Id);

        public void ResetTutorialProgress(string templateId)
        {
            _completedTemplateIds.Remove(templateId);
            _completedStepsByTemplate.Remove(templateId);
        }

        public void ResetAllTutorialProgress()
        {
            _completedTemplateIds.Clear();
            _completedStepsByTemplate.Clear();
        }

        public void MarkStepAsCompleted(string templateId, int stepIndex)
        {
            if (!_completedStepsByTemplate.TryGetValue(templateId, out HashSet<int> completedSteps))
            {
                completedSteps = new HashSet<int>();
                _completedStepsByTemplate[templateId] = completedSteps;
            }

            if (completedSteps.Add(stepIndex))
            {
                OnTutorialStepCompleted?.Invoke(templateId, stepIndex);
                SaveTutorialProgress();
#if DEBUG
                Debug.Log($"Tutorial step completed: {templateId}, Step {stepIndex}");
#endif
            }
        }

        public void MarkTutorialAsCompleted(string templateId)
        {
            if (_completedTemplateIds.Add(templateId))
            {
                OnTutorialCompleted?.Invoke(templateId);
#if DEBUG
                Debug.Log($"Tutorial completed: {templateId}");
#endif
                TutorialTemplate template = _templates.Find(t => t.Id == templateId);

                if (template != null)
                {
                    for (int i = 0; i < template.Steps.Count; i++)
                    {
                        MarkStepAsCompleted(templateId, i);
                    }
                }

                SaveTutorialProgress();
            }

            if (_activeTutorials.TryGetValue(templateId, out Tutorial tutorial))
            {
                tutorial.End();
                _activeTutorials.Remove(templateId);
            }
        }

        public void InvokePhaseStart(TutorialPhase phase)
        {
            _phaseCallbacks.InvokePhaseStart(phase);
        }

        public void InvokePhaseEnd(TutorialPhase phase)
        {
            _phaseCallbacks.InvokePhaseEnd(phase);
        }

        public TutorialStep ConvertTemplateToStep(TutorialStepTemplate template)
        {
            TutorialStep step = new TutorialStep
            {
                UseCutoutMask = template.UseCutoutMask,
                UseCustomPointer = template.UseCustomPointer,
                WidthMultiplier = template.WidthMultiplier,
                HeightMultiplier = template.HeightMultiplier,
                PopupOffset = template.PopupOffset,
                Pointer = template.UseCustomPointer ? template.Pointer : null,
                UseCustomHighlightProperties = template.UseCustomHighlightProperties,
                CustomMaskFadeDuration = template.CustomMaskFadeDuration,
                CustomMaskFadeEase = template.CustomMaskFadeEase,
                CustomMaskEdgeSoftness = template.CustomMaskEdgeSoftness,
                PositionFlags = template.PositionFlags
            };

            if (template.Popup != null)
            {
                step.Popup = template.Popup;
                step.Popup.Construct();
                _windowService.RegisterTutorialPopup(step.Popup);
            }

            step.HighlightTargets = TutorialUtils.ConvertReferencesToTransforms(template.HighlightTargets);
            step.PopupTarget = TutorialUtils.GetReferenceTransform(template.PopupTarget);

            if (template.Trigger != null)
            {
                step.TriggerObject = template.Trigger;
                step.TriggerObject.ConvertTargets();
            }

            return step;
        }

        public void EnableCutoutMask()
        {
            if (!_cutoutMask.gameObject.activeSelf)
                _cutoutMask.gameObject.SetActive(true);
        }

        public void DisableCutoutMask()
        {
            if (_cutoutMask.gameObject.activeSelf)
                _cutoutMask.gameObject.SetActive(false);
        }

        public void ApplyCutoutMaskToTargets(TutorialStep step, List<TutorialHighlight> activeHighlights)
        {
            foreach (RectTransform target in step.HighlightTargets)
            {
                TutorialHighlight highlight = target.GetComponent<TutorialHighlight>() ??
                                              target.gameObject.AddComponent<TutorialHighlight>();
                highlight.Construct(
                    _cutoutMask, Overlay, edgeSoftness: step.MaskEdgeSoftness,
                    step.MaskFadeDuration, step.MaskFadeEase);
                activeHighlights.Add(highlight);
            }
        }

        public void PositionPopupAboveTarget(
            TutorialPopup popup, RectTransform target, float widthMultiplier, float heightMultiplier,
            Vector2 offset, TutorialStepTemplate.PopupPositionFlags flags)
        {
            Vector2 targetSize = target.rect.size;
            Vector2 popupSize = popup.ContentRectTransform.sizeDelta;

            if ((flags & TutorialStepTemplate.PopupPositionFlags.ResizeWidth) != 0)
                popupSize.x = targetSize.x * widthMultiplier;

            if ((flags & TutorialStepTemplate.PopupPositionFlags.ResizeHeight) != 0)
                popupSize.y = targetSize.y * heightMultiplier;

            if ((flags & TutorialStepTemplate.PopupPositionFlags.ResizeWidth) != 0 ||
                (flags & TutorialStepTemplate.PopupPositionFlags.ResizeHeight) != 0)
                popup.ContentRectTransform.sizeDelta = popupSize;

            if ((flags & (TutorialStepTemplate.PopupPositionFlags.PositionX |
                          TutorialStepTemplate.PopupPositionFlags.PositionY)) != 0)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    popup.CanvasRectTransform,
                    RectTransformUtility.WorldToScreenPoint(popup.AssociatedCamera, target.position),
                    popup.AssociatedCamera, out Vector2 targetPosition);

                Vector2 newPosition = popup.RectTransform.anchoredPosition;

                if ((flags & TutorialStepTemplate.PopupPositionFlags.PositionX) != 0)
                    newPosition.x = targetPosition.x;

                if ((flags & TutorialStepTemplate.PopupPositionFlags.PositionY) != 0)
                    newPosition.y = targetPosition.y;

                popup.RectTransform.anchoredPosition = newPosition + offset;
            }
        }

#if DEBUG
        [Button("Jump To Step"), GUIColor(0.3f, 0.8f, 0.3f)]
        public void JumpToStep(string templateId, int stepIndex)
        {
            TutorialTemplate template = _templates.Find(t => t.Id == templateId);

            if (template == null)
            {
                Debug.LogWarning($"No template found with ID: {templateId}");
                return;
            }

            if (!_activeTutorials.TryGetValue(templateId, out Tutorial tutorial))
            {
                tutorial = new Tutorial(template, this);
                _activeTutorials[templateId] = tutorial;
            }

            tutorial.JumpToStep(stepIndex);
        }
#endif

        private bool HasTutorialEverStarted()
        {
            return _completedTemplateIds.Count > 0 || _completedStepsByTemplate.Count > 0 ||
                   _activeTutorials.Count > 0;
        }

        private TutorialTemplate GetTemplateForPhase(TutorialPhase phase, string templateId)
        {
            TutorialTemplate template = null;

            if (!string.IsNullOrEmpty(templateId))
                template = _templates.Find(t => t.Id == templateId);

            template ??= _templates.FirstOrDefault(t => t.Steps.Any(s => s.Phase == phase));

            if (template == null)
            {
#if DEBUG
                Debug.LogWarning($"No template found with steps for phase {phase}");
#endif
            }

            return template;
        }

        private void StartTutorialFromPhase(TutorialPhase phase, TutorialTemplate template)
        {
            if (_activeTutorials.TryGetValue(template.Id, out Tutorial existingTutorial))
            {
                existingTutorial.End();
            }

            Tutorial newTutorial = new Tutorial(template, this);
            _activeTutorials[template.Id] = newTutorial;
            newTutorial.StartFromPhase(phase);
        }

        public void ContinueTutorialFromPhase(TutorialPhase phase, TutorialTemplate template)
        {
            Tutorial activeTutorial = _activeTutorials.Values.FirstOrDefault(static t => t.IsActive);

            if (activeTutorial != null && activeTutorial.TemplateId != template.Id)
            {
                activeTutorial.CompleteCurrentPhase();
                _activeTutorials.Remove(activeTutorial.TemplateId);
            }

            if (!_activeTutorials.TryGetValue(template.Id, out Tutorial tutorial))
            {
                tutorial = new Tutorial(template, this);
                _activeTutorials[template.Id] = tutorial;
            }

            tutorial.ContinueFromPhase(phase);
        }

        private void SaveTutorialProgress()
        {
            var activeTutorial = _activeTutorials.Values.FirstOrDefault(t => t.IsActive);
            bool isTutorialCompleted = activeTutorial != null && IsTutorialCompleted(activeTutorial.TemplateId);

            PlayerSaveData.TutorialProgressData data = new PlayerSaveData.TutorialProgressData
            {
                CompletedTutorials = _completedTemplateIds.ToList(),
                CompletedStepsByTemplate = _completedStepsByTemplate.ToDictionary(
                    static kvp => kvp.Key, static kvp => kvp.Value.ToList()),
                TutorialCompleted = isTutorialCompleted,
            };

            _saveLoadService.PlayerSaveData.TutorialProgress = data;
            _saveLoadService.SaveData(syncImmediately: true);
#if DEBUG
            Debug.Log("Tutorial progress saved");
#endif
        }

        private void LoadTutorialProgress()
        {
            PlayerSaveData.TutorialProgressData savedData = _saveLoadService.PlayerSaveData.TutorialProgress;
            _completedTemplateIds = savedData.CompletedTutorials.ToHashSet();
            _completedStepsByTemplate = savedData.CompletedStepsByTemplate.ToDictionary(
                static kvp => kvp.Key, static kvp => kvp.Value.ToHashSet());

#if DEBUG
            Debug.Log("Tutorial progress loaded");
            Debug.Log($"Completed tutorials: {string.Join(", ", _completedTemplateIds)}");
            foreach (var kvp in _completedStepsByTemplate)
            {
                Debug.Log($"Tutorial {kvp.Key} completed steps: {string.Join(", ", kvp.Value)}");
            }
#endif
        }
    }
}