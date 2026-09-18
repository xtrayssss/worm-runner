using System.Collections;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
    }
}