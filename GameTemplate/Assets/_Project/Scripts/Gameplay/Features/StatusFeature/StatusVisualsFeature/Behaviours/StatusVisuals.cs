using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Behaviours
{
    public sealed class StatusVisuals : MonoBehaviour
    {
        [Serializable]
        public class Effect
        {
            [SerializeField] private GameObject _view;

            public GameObject View => _view;
            public bool IsBound { get; private set; }

            public void SetBound(bool bound) =>
                IsBound = bound;
        }

        [SerializeField] private Effect[] _freezeEffectLevels;
        [SerializeField] private Effect _poisonEffect;
        [field: SerializeField] public string SlotNameForEffects { get; private set; } = "Head";
        private int _activeFreezeLevelIndex = -1;

        public GameObject ApplyFreeze(int stackCount)
        {
            int levelIndex = Mathf.Clamp(stackCount - 1, 0, _freezeEffectLevels.Length - 1);

            if (_activeFreezeLevelIndex == levelIndex)
                return _freezeEffectLevels[levelIndex].View;

            if (_activeFreezeLevelIndex >= 0 && _activeFreezeLevelIndex < _freezeEffectLevels.Length)
                _freezeEffectLevels[_activeFreezeLevelIndex].View.SetActive(false);

            _freezeEffectLevels[levelIndex].View.SetActive(true);
            _activeFreezeLevelIndex = levelIndex;

            return _freezeEffectLevels[levelIndex].View;
        }

        public void UnapplyFreeze(int remainingStacks)
        {
            if (remainingStacks > 0)
            {
                ApplyFreeze(remainingStacks);
            }
            else
            {
                if (_activeFreezeLevelIndex >= 0 && _activeFreezeLevelIndex < _freezeEffectLevels.Length)
                    _freezeEffectLevels[_activeFreezeLevelIndex].View.SetActive(false);

                _activeFreezeLevelIndex = -1;
            }
        }

        public void ApplyPoison() =>
            _poisonEffect.View.SetActive(true);

        public void UnapplyPoison() =>
            _poisonEffect.View.SetActive(false);

        public Effect GetActiveFreeEffect() =>
            _freezeEffectLevels[_activeFreezeLevelIndex];

        public Effect GetActivePoisonEffect() =>
            _poisonEffect;
    }
}