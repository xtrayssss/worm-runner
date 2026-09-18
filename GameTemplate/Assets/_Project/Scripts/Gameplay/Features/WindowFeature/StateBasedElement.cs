using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    [Serializable]
    public class StateBasedElement
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private GameState _visibleInStates;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public GameState VisibleInStates => _visibleInStates;

        public bool ShouldBeVisible(GameState currentState) =>
            (_visibleInStates & currentState) != 0;

        public void UpdateVisibility(GameState currentState)
        {
            bool shouldBeVisible = ShouldBeVisible(currentState);
            CanvasGroup.alpha = shouldBeVisible ? 1f : 0f;
            _canvasGroup.interactable = shouldBeVisible;
            _canvasGroup.blocksRaycasts = shouldBeVisible;
        }
    }
}