using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using Cinemachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
    public sealed class CrowdView : EntityView
    {
        [SerializeField]
        private CharacterController _characterController;

        public CinemachineVirtualCamera FollowCamera { get; private set; }
        public CharacterController CharacterController => _characterController;

        public void SetFollowCamera(CinemachineVirtualCamera followCamera) =>
            FollowCamera = followCamera;
    }
}