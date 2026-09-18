using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature.Services
{
    [Serializable]
    public sealed class CurrencyFlyAnimationService : IService
    {
        [ShowInInspector] private readonly Dictionary<CurrencyType, Queue<float>> _pendingAnimations =
            new Dictionary<CurrencyType, Queue<float>>();

        public event Action<CurrencyType, float> OnPlayFlyAnimation;

        public CurrencyFlyAnimationService()
        {
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
                _pendingAnimations[type] = new Queue<float>();
        }

        public void QueueAnimation(CurrencyType type, float amount) =>
            _pendingAnimations[type].Enqueue(amount);

        public void PlayInstantAnimation(CurrencyType type, float amount) =>
            OnPlayFlyAnimation?.Invoke(type, amount);

        public void PlayQueuedAnimations(CurrencyType type)
        {
            Queue<float> animations = _pendingAnimations[type];
            float totalAmount = 0;

            while (animations.Count > 0)
                totalAmount += animations.Dequeue();

            if (totalAmount > 0)
                OnPlayFlyAnimation?.Invoke(type, totalAmount);
        }

        public void PlayAllQueuedAnimations()
        {
            foreach ((CurrencyType currencyType, _) in _pendingAnimations)
                PlayQueuedAnimations(currencyType);
        }

        public void ClearQueuedAnimations(CurrencyType type) =>
            _pendingAnimations[type].Clear();
    }
}