using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.TutorialFeature.CutoutMask.Scripts;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public sealed class Tutorial
    {
        private readonly TutorialTemplate _template;
        private readonly TutorialService _service;
        private int _activeStepIndex = -1;
        private readonly List<TutorialStep> _steps = new List<TutorialStep>();
        private ITutorialTrigger _activeTrigger;
        private TutorialPointer _activePointer;
        private readonly List<TutorialHighlight> _activeHighlights = new List<TutorialHighlight>(4);

        public string TemplateId => _template.Id;
        public int CurrentStepIndex { get; private set; } = -1;
        public bool IsActive => _activeStepIndex >= 0;
        public TutorialTemplate Template => _template;

        public Tutorial(TutorialTemplate template, TutorialService service)
        {
            _template = template;
            _service = service;
        }

        public void StartFromPhase(TutorialPhase phase)
        {
            int firstStepInPhase = _template.Steps.FindIndex(s => s.Phase == phase);
            if (firstStepInPhase == -1)
            {
#if DEBUG
                Debug.LogWarning($"No steps found for phase {phase} in template {_template.Id}");
#endif
                return;
            }

            _activeStepIndex = firstStepInPhase - 1;
#if DEBUG
            Debug.Log($"Starting tutorial {_template.Id} from phase {phase}, step index {firstStepInPhase}");
#endif
            ShowNextStep();
        }

        public void ContinueFromPhase(TutorialPhase phase)
        {
            int startStepIndex = -1;
            for (int i = 0; i < _template.Steps.Count; i++)
            {
                if (_template.Steps[i].Phase == phase && !_service.IsStepCompleted(_template.Id, i))
                {
                    startStepIndex = i - 1;
                    break;
                }
            }

            if (startStepIndex == -1)
            {
                startStepIndex = _template.Steps.FindIndex(s => s.Phase == phase) - 1;
                
                if (startStepIndex < -1) 
                    startStepIndex = -1;
            }

#if DEBUG
            Debug.Log(
                $"Continuing tutorial {_template.Id} from phase {phase}, starting at step index {startStepIndex + 1}");
#endif
            _activeStepIndex = startStepIndex;
            ShowNextStep();
        }

        public void RestartFromPhase(TutorialPhase phase)
        {
            int firstStepInPhase = _template.Steps.FindIndex(s => s.Phase == phase);
            if (firstStepInPhase == -1)
            {
#if DEBUG
                Debug.LogWarning($"No steps found for phase {phase} in template {_template.Id}");
#endif
                return;
            }

            End();
            _activeStepIndex = firstStepInPhase - 1;
#if DEBUG
            Debug.Log(
                $"Restarting tutorial {_template.Id} phase {phase} from beginning, step index {firstStepInPhase}");
#endif
            ShowNextStep();
        }

        private TutorialPhase GetCurrentPhase()
        {
            if (_activeStepIndex < 0 || _activeStepIndex >= _template.Steps.Count)
                return TutorialPhase.NONE;
            return _template.Steps[_activeStepIndex].Phase;
        }

        public bool IsCurrentActiveStep(int stepIndex)
        {
            return _activeStepIndex == stepIndex;
        }

        public bool IsPhaseActive(TutorialPhase phase)
        {
            TutorialPhase currentPhase = GetCurrentPhase();
            return currentPhase != TutorialPhase.NONE && currentPhase == phase;
        }

        private void ShowNextStep()
        {
            TutorialPhase previousPhase = GetCurrentPhase();

            if (_activeStepIndex >= 0)
            {
                _service.MarkStepAsCompleted(_template.Id, _activeStepIndex);
                HideCurrentStep();
                CleanupTrigger();
            }

            _activeStepIndex++;

            if (_activeStepIndex >= _template.Steps.Count)
            {
                _service.MarkTutorialAsCompleted(_template.Id);
                End();
                return;
            }

            TutorialStepTemplate newStep = _template.Steps[_activeStepIndex];
            if (newStep.Phase != previousPhase)
                _service.InvokePhaseStart(newStep.Phase);

            TutorialStep step = _service.ConvertTemplateToStep(newStep);
            _steps.Add(step);
            CurrentStepIndex = _steps.Count - 1;

            ShowStep(step);
            SetupTrigger();
        }

        private void HandleTriggerActivated()
        {
            CleanupTrigger();
            int nextStepIndex = _activeStepIndex + 1;

            if (nextStepIndex < _template.Steps.Count)
            {
                TutorialPhase currentPhase = _template.Steps[_activeStepIndex].Phase;
                TutorialPhase nextPhase = _template.Steps[nextStepIndex].Phase;

                if (nextPhase != currentPhase)
                {
#if DEBUG
                    Debug.Log(
                        $"Next step belongs to different phase: current={currentPhase}, next={nextPhase}. Ending current tutorial phase.");
#endif
                    CompleteCurrentPhase();
                    return;
                }
            }

            ShowNextStep();
        }

        public void CompleteCurrentPhase()
        {
            TutorialPhase completedPhase = GetCurrentPhase();
            _service.InvokePhaseEnd(completedPhase);

            string templateId = _template.Id;
            for (int i = 0; i < _template.Steps.Count; i++)
            {
                if (_template.Steps[i].Phase == completedPhase)
                {
                    _service.MarkStepAsCompleted(templateId, i);
                }
            }

            HideCurrentStep();
            CleanupTrigger();
            _steps.Clear();
            _activeStepIndex = -1;

#if DEBUG
            Debug.Log($"Completed tutorial phase: {completedPhase}");
#endif
        }

        public void End()
        {
            HideCurrentStep();
            CleanupTrigger();
            _steps.Clear();
            _activeStepIndex = -1;
        }

#if DEBUG
        public void JumpToStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _template.Steps.Count)
            {
                Debug.LogWarning($"Invalid step index: {stepIndex}. Valid range is 0-{_template.Steps.Count - 1}");
                return;
            }

            if (_activeStepIndex >= 0)
            {
                HideCurrentStep();
                CleanupTrigger();
            }

            _activeStepIndex = stepIndex - 1;
            Debug.Log($"Jumping to tutorial step {stepIndex} in {_template.Id}");
            ShowNextStep();
        }
#endif

        private void ShowStep(TutorialStep step)
        {
            _activeHighlights.Clear();

            if (step.Popup != null)
            {
                if (step.PopupTarget != null)
                {
                    _service.PositionPopupAboveTarget(
                        step.Popup, step.PopupTarget, step.WidthMultiplier, step.HeightMultiplier,
                        step.PopupOffset, step.PositionFlags);
                }

                step.Popup.Show();
            }

            if (step.UseCutoutMask && step.HighlightTargets.Length > 0)
            {
                _service.EnableCutoutMask();
                _service.ApplyCutoutMaskToTargets(step, _activeHighlights);
            }

            if (step.UseCustomPointer && step.Pointer != null)
            {
                _activePointer = step.Pointer;
                _activePointer.Show(_service.PointerContainer);
            }
        }

        private void HideCurrentStep()
        {
            if (CurrentStepIndex < 0 || CurrentStepIndex >= _steps.Count) return;

            TutorialStep step = _steps[CurrentStepIndex];

            if (step.Popup != null)
                step.Popup.Close();

            foreach (TutorialHighlight highlight in _activeHighlights)
                Object.DestroyImmediate(highlight);

            _activeHighlights.Clear();

            if (step.UseCutoutMask)
                _service.DisableCutoutMask();

            if (_activePointer != null)
            {
                _activePointer.Hide();
                _activePointer = null;
            }
        }

        private void SetupTrigger()
        {
            if (CurrentStepIndex < 0 || CurrentStepIndex >= _steps.Count) return;

            ITutorialTrigger trigger = _steps[CurrentStepIndex].TriggerObject;
            if (trigger != null)
            {
                _activeTrigger = trigger;
                _activeTrigger.Initialize();
                _activeTrigger.OnTriggerActivated += HandleTriggerActivated;
                _activeTrigger.Enable();
            }
        }

        private void CleanupTrigger()
        {
            if (_activeTrigger != null)
            {
                _activeTrigger.OnTriggerActivated -= HandleTriggerActivated;
                _activeTrigger.Disable();
                _activeTrigger = null;
            }
        }
    }
}