using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.StealthFeature.Components;
using PrimeTween;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WatchTowerSystem : ISystem
    {
        public World World { get; set; }

        private Filter _towers;

        public void OnAwake()
        {
            _towers = World.Filter
                .With<WatchTower>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity tower in _towers)
            {
                ref WatchTower watchTower = ref tower.GetComponent<WatchTower>();

                if (tower.Has<DetectedTargetMarker>())
                {
                    if (watchTower.ConeMovementTween.isAlive)
                        watchTower.ConeMovementTween.Stop();

                    continue;
                }

                ref readonly EntityViewLink viewLink = ref tower.GetComponent<EntityViewLink>();
                WatchTowerView view = (WatchTowerView)viewLink.View;

                if (!watchTower.ConeMovementTween.isAlive)
                {
                    Vector3 localStartDir = view.StartPoint.localPosition.normalized;
                    Vector3 localEndDir = view.EndPoint.localPosition.normalized;
                
                    float startAngle = Mathf.Atan2(localStartDir.x, localStartDir.z) * Mathf.Rad2Deg;
                    float endAngle = Mathf.Atan2(localEndDir.x, localEndDir.z) * Mathf.Rad2Deg;

                    watchTower.ConeMovementTween = Tween.Custom(
                        target: view.CommonStealthObjectView.ConePivot,
                        startValue: startAngle,
                        endValue: endAngle,
                        duration: watchTower.SweepDuration,
                        onValueChange: static (pivot, angle) =>
                        {
                            pivot.localRotation = Quaternion.Euler(pivot.transform.localEulerAngles.x, angle, pivot.transform.localEulerAngles.z);
                        },
                        ease: Ease.InOutSine,
                        cycles: -1,
                        cycleMode: CycleMode.Yoyo
                    );
                }
            }
        }

        public void Dispose()
        {
        }
    }
}