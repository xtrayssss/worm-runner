using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    public sealed class StateBasedUIController : MonoBehaviour
    {
        [SerializeField]
        private List<StateBasedElement> _elements = new List<StateBasedElement>();

        [SerializeField, ReadOnly]
        private GameState _currentState = GameState.NONE;

        public GameState CurrentState => _currentState;
        public IReadOnlyList<StateBasedElement> Elements => _elements;

        public void SetState(GameState newState)
        {
            if (_currentState == newState)
                return;

            _currentState = newState;
            UpdateAllElements();
        }

        public void UpdateAllElements()
        {
            foreach (StateBasedElement element in _elements.Where(static e => e.CanvasGroup != null))
                element.UpdateVisibility(_currentState);
        }

        public IEnumerable<StateBasedElement> GetElementsVisibleInState(GameState state) =>
            _elements.Where(element => element.ShouldBeVisible(state));
    }
}