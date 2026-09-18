using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.GameTimeFeature.Services
{
    public sealed class GameTimeService : IService
    {
        public enum SpeedMultiplier
        {
            NORMAL = 1,
            DOUBLE = 2,
            QUADRUPLE = 4
        }

        public SpeedMultiplier CurrentSpeed { get; private set; } = SpeedMultiplier.NORMAL;

        public bool IsDoubleSpeedUnlocked { get; private set; }
        public bool IsQuadrupleSpeedUnlocked { get; private set; }
        public bool IsPaused { private set; get; }
        public event Action<SpeedMultiplier> OnSpeedChanged;

        public void SetSpeed(SpeedMultiplier speed)
        {
            if ((speed == SpeedMultiplier.DOUBLE && !IsDoubleSpeedUnlocked) ||
                (speed == SpeedMultiplier.QUADRUPLE && !IsQuadrupleSpeedUnlocked))
            {
#if DEBUG
                Debug.LogWarning($"Attempted to set speed to {speed}x but it's not unlocked yet.");
#endif
                return;
            }

            if (CurrentSpeed != speed)
            {
                CurrentSpeed = speed;
                Time.timeScale = (float)speed;
                OnSpeedChanged?.Invoke(CurrentSpeed);
                IsPaused = false;
#if DEBUG
                Debug.Log($"Game speed changed to {speed}x");
#endif
            }
        }

        public void UnlockDoubleSpeed()
        {
            if (!IsDoubleSpeedUnlocked)
            {
                IsDoubleSpeedUnlocked = true;
#if DEBUG
                Debug.Log("Double speed (2x) unlocked");
#endif
            }
        }

        public void UnlockQuadrupleSpeed()
        {
            if (IsDoubleSpeedUnlocked && !IsQuadrupleSpeedUnlocked)
            {
                IsQuadrupleSpeedUnlocked = true;
#if DEBUG
                Debug.Log("Quadruple speed (4x) unlocked");
#endif
            }
            else if (!IsDoubleSpeedUnlocked)
            {
#if DEBUG

                Debug.LogWarning("Cannot unlock 4x speed before 2x speed is unlocked");
#endif
            }
        }

        public void Pause()
        {
            IsPaused = true;

            World.Default
                .GetRequest<PauseRequest>()
                .Publish(new PauseRequest(), allowNextFrame: true);
        }

        public void Unpause()
        {
            IsPaused = false;

            World.Default
                .GetRequest<UnpauseRequest>()
                .Publish(new UnpauseRequest(), allowNextFrame: true);
        }

        public void Reset()
        {
            IsDoubleSpeedUnlocked = false;
            IsQuadrupleSpeedUnlocked = false;
            ResetSpeed();
        }

        public void ResetSpeed()
        {
            IsPaused = false;

            CurrentSpeed = SpeedMultiplier.NORMAL;

            Time.timeScale = 1f;

            OnSpeedChanged?.Invoke(CurrentSpeed);

#if DEBUG
            Debug.Log("Speed reset to 1x");
#endif
        }
    }
}