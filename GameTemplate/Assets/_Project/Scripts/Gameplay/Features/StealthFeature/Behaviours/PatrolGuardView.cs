using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours
{
    public sealed class PatrolGuardView : StealthObjectView
    {
        private Sequence _idleSequence;

        public void StartIdle()
        {
            const float IDLE_SCALE_AMOUNT = 0.05f;
            const float IDLE_DURATION = 1.9f;
            const Ease IDLE_EASE = Ease.InOutSine;
            const float IDLE_Y_MOVEMENT = 0.08f;

            StopIdle();

            _idleSequence = Sequence.Create(cycles: -1, cycleMode: CycleMode.Yoyo)
                .Group(Tween.Scale(
                    VisualRoot,
                    Vector3.one * (1f + IDLE_SCALE_AMOUNT),
                    IDLE_DURATION / 2f,
                    IDLE_EASE))
                .Group(Tween.PositionY(
                    VisualRoot,
                    VisualRoot.position.y + IDLE_Y_MOVEMENT,
                    IDLE_DURATION / 2f,
                    IDLE_EASE));
        }

        public void StopIdle()
        {
            if (_idleSequence.isAlive)
            {
                _idleSequence.Stop();
                VisualRoot.localScale = Vector3.one;

                VisualRoot.position = new Vector3(
                    VisualRoot.position.x,
                    0,
                    VisualRoot.position.z);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_idleSequence.isAlive)
                _idleSequence.Stop();
        }
    }
}