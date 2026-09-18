using System;
using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public sealed class TutorialPhaseCallbacks
    {
        private readonly Dictionary<TutorialPhase, PhaseCallbacks> _phaseCallbacks =
            new Dictionary<TutorialPhase, PhaseCallbacks>();

        public void RegisterPhaseCallbacks(TutorialPhase phase, Action onStart = null, Action onEnd = null)
        {
            if (!_phaseCallbacks.ContainsKey(phase))
                _phaseCallbacks[phase] = new PhaseCallbacks();

            var callbacks = _phaseCallbacks[phase];

            if (onStart != null)
                callbacks.OnPhaseStart += onStart;

            if (onEnd != null)
                callbacks.OnPhaseEnd += onEnd;
        }

        public void UnregisterPhaseCallbacks(TutorialPhase phase, Action onStart = null, Action onEnd = null)
        {
            if (!_phaseCallbacks.TryGetValue(phase, out var callbacks))
                return;

            if (onStart != null)
                callbacks.OnPhaseStart -= onStart;

            if (onEnd != null)
                callbacks.OnPhaseEnd -= onEnd;
        }

        internal void InvokePhaseStart(TutorialPhase phase)
        {
            if (_phaseCallbacks.TryGetValue(phase, out var callbacks))
            {
                callbacks.OnPhaseStart?.Invoke();
            }
        }

        internal void InvokePhaseEnd(TutorialPhase phase)
        {
            if (_phaseCallbacks.TryGetValue(phase, out var callbacks))
            {
                callbacks.OnPhaseEnd?.Invoke();
            }
        }

        private class PhaseCallbacks
        {
            public Action OnPhaseStart;
            public Action OnPhaseEnd;
        }
    }
}