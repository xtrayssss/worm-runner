using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using Cinemachine;
using PrimeTween;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Services
{
    [Serializable]
    public sealed class CameraService : IService
    {
        private readonly World _world = World.Default;

        private readonly Dictionary<CinemachineVirtualCamera, CameraState> _cameraStates =
            new Dictionary<CinemachineVirtualCamera, CameraState>();

        private Filter _crowds;
        public Camera MainCamera { get; private set; }

        public void Initialize()
        {
            MainCamera = Camera.main;

            _crowds = _world.Filter
                .With<CrowdTag>()
                .With<EntityViewLink>()
                .Build();

            SaveInitialCameraStates();
        }

        [Button]
        public void PlayCameraShake(
            float strength = 0.5f,
            float duration = 0.5f,
            int frequency = 10)
        {
            foreach (Entity crowd in _crowds)
            {
                if (!crowd.Has<EntityViewLink>())
                    continue;

                ref readonly EntityViewLink entityViewLink = ref crowd.GetComponent<EntityViewLink>();
                
                if (entityViewLink.View is not CrowdView crowdView)
                    continue;

                Tween.ShakeLocalRotation(
                    crowdView.FollowCamera.transform,
                    Vector3.one * strength,
                    duration,
                    frequency);
            }
        }

        public Sequence PlayFinishFovEffect(float fovIncrease = 5f, float duration = 1f, Ease ease = Ease.OutQuart)
        {
            Sequence sequence = Sequence.Create();

            foreach (Entity crowd in _crowds)
            {
                if (!crowd.Has<EntityViewLink>())
                    continue;

                ref readonly EntityViewLink entityViewLink = ref crowd.GetComponent<EntityViewLink>();

                if (entityViewLink.View is not CrowdView crowdView)
                    continue;

                CinemachineVirtualCamera camera = crowdView.FollowCamera;

                EnsureCameraStateExists(camera);

                CameraState state = _cameraStates[camera];
                float targetFov = state.OriginalFov + fovIncrease;

                Tween fovTween = Tween.Custom(
                    camera,
                    startValue: camera.m_Lens.FieldOfView,
                    endValue: targetFov,
                    duration: duration,
                    onValueChange: static (cam, fov) => cam.m_Lens.FieldOfView = fov,
                    ease: ease
                );

                sequence.Chain(fovTween);

                state.IsFovEffectActive = true;
                state.CurrentFovTween = sequence;
            }

            return sequence;
        }

        public void RestoreFovToOriginal(float duration = 0.8f, Ease ease = Ease.InOutQuad, float delay = 0f)
        {
            Sequence sequence = Sequence.Create();

            if (delay > 0f)
                sequence.ChainDelay(delay);

            foreach (Entity crowd in _crowds)
            {
                if (!crowd.Has<EntityViewLink>())
                    continue;

                ref readonly EntityViewLink entityViewLink = ref crowd.GetComponent<EntityViewLink>();
                if (entityViewLink.View is not CrowdView crowdView)
                    continue;

                CinemachineVirtualCamera camera = crowdView.FollowCamera;

                if (!_cameraStates.TryGetValue(camera, out CameraState state))
                    continue;

                if (!state.IsFovEffectActive)
                    continue;

                if (state.CurrentFovTween.isAlive)
                    state.CurrentFovTween.Stop();

                Tween restoreTween = Tween.Custom(
                    camera,
                    startValue: camera.m_Lens.FieldOfView,
                    endValue: state.OriginalFov,
                    duration: duration,
                    onValueChange: static (cam, fov) => cam.m_Lens.FieldOfView = fov,
                    ease: ease
                );

                sequence.Chain(restoreTween);

                state.IsFovEffectActive = false;
                state.CurrentFovTween = sequence;
            }
        }

        private void SaveInitialCameraStates()
        {
            foreach (Entity crowd in _crowds)
            {
                if (!crowd.Has<EntityViewLink>())
                    continue;

                ref readonly EntityViewLink entityViewLink = ref crowd.GetComponent<EntityViewLink>();
                if (entityViewLink.View is not CrowdView crowdView)
                    continue;

                CinemachineVirtualCamera camera = crowdView.FollowCamera;

                EnsureCameraStateExists(camera);
            }
        }

        private void EnsureCameraStateExists(CinemachineVirtualCamera camera)
        {
            if (!_cameraStates.ContainsKey(camera))
            {
                _cameraStates[camera] = new CameraState
                {
                    OriginalFov = camera.m_Lens.FieldOfView,
                    IsFovEffectActive = false
                };
            }
        }

        private class CameraState
        {
            public float OriginalFov;
            public bool IsFovEffectActive;
            public Sequence CurrentFovTween;
        }
    }
}