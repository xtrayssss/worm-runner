using System;
using System.Collections;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public class StateTransitionHandler : IService
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private static StateTransitionHandler _instance;
        private Action _onFrameEndAction;

        public StateTransitionHandler(ICoroutineRunner coroutineRunner) => 
            _coroutineRunner = coroutineRunner;

        public void ScheduleFrameEndAction(Action action) => 
            _coroutineRunner.StartCoroutine(ExecuteAtFrameEnd(action));

        private static IEnumerator ExecuteAtFrameEnd(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }
    }
}