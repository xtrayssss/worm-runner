using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Gameplay.Features.InputFeature.Systems
{
    [Serializable]
    public sealed class InputService : IService
    {
        [ShowInInspector] public Vector2 MoveDirection { get; private set; }

        private InputAction _moveAction;

        public void Initialize()
        {
            PlayerControls playerInput = new PlayerControls();
            _moveAction = playerInput.Game.Move;

            _moveAction.Enable();

            _moveAction.performed += context => MoveDirection = context.ReadValue<Vector2>();
            _moveAction.canceled += _ => MoveDirection = Vector2.zero;
        }

        public void Dispose()
        {
            _moveAction.Disable();
        }
    }
}