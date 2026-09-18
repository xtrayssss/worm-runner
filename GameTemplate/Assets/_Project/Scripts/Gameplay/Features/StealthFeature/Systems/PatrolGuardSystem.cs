using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.StealthFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PatrolGuardSystem : ISystem
    {
        public World World { get; set; }

        private Filter _patrollingGuards;
        private Filter _detectedGuards;
        private Filter _crowds;

        public void OnAwake()
        {
            _patrollingGuards = World.Filter
                .With<PatrolGuard>()
                .With<EntityViewLink>()
                .Without<DetectedTargetMarker>()
                .Build();

            _detectedGuards = World.Filter
                .With<PatrolGuard>()
                .With<EntityViewLink>()
                .With<DetectedTargetMarker>()
                .Build();

            _crowds = World.Filter
                .With<CrowdTag>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity guard in _patrollingGuards)
            {
                ref PatrolGuard patrol = ref guard.GetComponent<PatrolGuard>();
                ref readonly EntityViewLink viewLink = ref guard.GetComponent<EntityViewLink>();

                patrol.CurrentRotation += patrol.RotationSpeed * patrol.RotationDirection * deltaTime;

                if (Mathf.Abs(patrol.CurrentRotation) >= patrol.MaxRotationAngle)
                {
                    patrol.RotationDirection *= -1;

                    patrol.CurrentRotation = Mathf.Clamp(
                        patrol.CurrentRotation,
                        -patrol.MaxRotationAngle,
                        patrol.MaxRotationAngle
                    );
                }

                StealthObjectView view = (StealthObjectView)viewLink.View;
                view.transform.localRotation = Quaternion.Euler(
                    view.transform.localEulerAngles.x,
                    patrol.CurrentRotation,
                    view.transform.localEulerAngles.z);
            }

            if (_crowds.IsEmpty())
                return;

            Entity crowd = _crowds.First();
            ref readonly EntityViewLink crowdViewLink = ref crowd.GetComponent<EntityViewLink>();
            Vector3 crowdPosition = crowdViewLink.View.transform.position;

            foreach (Entity guard in _detectedGuards)
            {
                ref readonly EntityViewLink viewLink = ref guard.GetComponent<EntityViewLink>();
                StealthObjectView view = (StealthObjectView)viewLink.View;

                Vector3 guardPosition = view.transform.position;
                Vector3 directionToCrowd = guardPosition - crowdPosition;
                directionToCrowd.y = 0;
                
                if (directionToCrowd.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToCrowd);
                    float targetY = lookRotation.eulerAngles.y;
    
                    Vector3 currentRotation = view.transform.eulerAngles;
                    Quaternion targetRotation = Quaternion.Euler(currentRotation.x, targetY, currentRotation.z);
    
                    view.transform.rotation = Quaternion.Slerp(view.transform.rotation, targetRotation, 0.5f);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}