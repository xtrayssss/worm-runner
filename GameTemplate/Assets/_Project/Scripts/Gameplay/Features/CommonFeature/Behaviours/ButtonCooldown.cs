using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonCooldown : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        [Header("Cooldown Settings")]
        private float _cooldownDuration = 0.5f;

        [SerializeField]
        private bool _resetOnEnable = true;

        [Header("Visual Settings")]
        [SerializeField]
        private bool _changeVisualState = true;

        [SerializeField, ShowIf(nameof(_changeVisualState))]
        private VisualChangeType _visualChangeType = VisualChangeType.AlphaAndColor;

        [SerializeField, ShowIf(nameof(ShouldShowAlphaSettings))]
        [Range(0.1f, 1f)]
        private float _cooldownAlpha = 0.6f;

        [SerializeField, ShowIf(nameof(ShouldShowColorSettings))]
        private Color _cooldownColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("Debug")]
        [SerializeField, ReadOnly]
        private bool _isOnCooldown;

        [SerializeField, ReadOnly]
        private float _remainingCooldownTime;

        private Button _button;
        private CanvasGroup _canvasGroup;
        private Image _buttonImage;
        private bool _wasInteractableBeforeCooldown;
        private float _originalAlpha;
        private Color _originalColor;
        private Coroutine _cooldownCoroutine;

        public event Action OnCooldownStarted;
        public event Action OnCooldownEnded;

        public bool IsOnCooldown => _isOnCooldown;
        public float CooldownDuration => _cooldownDuration;
        public float RemainingCooldownTime => _remainingCooldownTime;

        private enum VisualChangeType
        {
            None,
            AlphaOnly,
            ColorOnly,
            AlphaAndColor
        }

        private bool ShouldShowAlphaSettings => _changeVisualState &&
                                                _visualChangeType
                                                    is VisualChangeType.AlphaOnly
                                                    or VisualChangeType.AlphaAndColor;

        private bool ShouldShowColorSettings => _changeVisualState &&
                                                _visualChangeType
                                                    is VisualChangeType.ColorOnly
                                                    or VisualChangeType.AlphaAndColor;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();

            if (_changeVisualState && _visualChangeType != VisualChangeType.None)
            {
                if (ShouldShowAlphaSettings)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }

                    _originalAlpha = _canvasGroup.alpha;
                }

                if (ShouldShowColorSettings && _buttonImage != null)
                {
                    _originalColor = _buttonImage.color;
                }
            }
        }

        private void OnEnable()
        {
            if (_resetOnEnable && _isOnCooldown)
            {
                StopCooldown();
            }
        }

        private void OnDisable()
        {
            StopCooldown();
        }

        public void TriggerCooldown()
        {
            TriggerCooldown(_cooldownDuration);
        }

        public void TriggerCooldown(float duration)
        {
            if (_isOnCooldown)
            {
                StopCooldown();
            }

            _cooldownCoroutine = StartCoroutine(CooldownCoroutine(duration));
        }

        public void StopCooldown()
        {
            if (_cooldownCoroutine != null)
            {
                StopCoroutine(_cooldownCoroutine);
                _cooldownCoroutine = null;
            }

            if (_isOnCooldown)
            {
                RestoreButtonState();
            }
        }

        public void SetCooldownDuration(float newDuration)
        {
            _cooldownDuration = Mathf.Max(0f, newDuration);
        }

        public bool CanClick()
        {
            return !_isOnCooldown && _button.interactable;
        }

        public float GetCooldownProgress()
        {
            if (!_isOnCooldown) return 1f;

            float totalDuration = _cooldownDuration;
            float elapsed = totalDuration - _remainingCooldownTime;
            return Mathf.Clamp01(elapsed / totalDuration);
        }

        private IEnumerator CooldownCoroutine(float duration)
        {
            StartCooldown();

            _remainingCooldownTime = duration;

            while (_remainingCooldownTime > 0f)
            {
                _remainingCooldownTime -= Time.unscaledDeltaTime;
                yield return null;
            }

            RestoreButtonState();
        }

        private void StartCooldown()
        {
            _isOnCooldown = true;
            _wasInteractableBeforeCooldown = _button.interactable;

            _button.interactable = false;

            ApplyVisualChanges();

            OnCooldownStarted?.Invoke();
        }

        private void RestoreButtonState()
        {
            _isOnCooldown = false;
            _remainingCooldownTime = 0f;

            _button.interactable = _wasInteractableBeforeCooldown;

            RestoreVisualState();

            OnCooldownEnded?.Invoke();
        }

        private void ApplyVisualChanges()
        {
            if (!_changeVisualState || _visualChangeType == VisualChangeType.None) return;

            switch (_visualChangeType)
            {
                case VisualChangeType.AlphaOnly:
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = _cooldownAlpha;
                    }

                    break;

                case VisualChangeType.ColorOnly:
                    if (_buttonImage != null)
                    {
                        _buttonImage.color = _cooldownColor;
                    }

                    break;

                case VisualChangeType.AlphaAndColor:
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = _cooldownAlpha;
                    }

                    if (_buttonImage != null)
                    {
                        _buttonImage.color = _cooldownColor;
                    }

                    break;
            }
        }

        private void RestoreVisualState()
        {
            if (!_changeVisualState || _visualChangeType == VisualChangeType.None) return;

            switch (_visualChangeType)
            {
                case VisualChangeType.AlphaOnly:
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = _originalAlpha;
                    }

                    break;

                case VisualChangeType.ColorOnly:
                    if (_buttonImage != null)
                    {
                        _buttonImage.color = _originalColor;
                    }

                    break;

                case VisualChangeType.AlphaAndColor:
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = _originalAlpha;
                    }

                    if (_buttonImage != null)
                    {
                        _buttonImage.color = _originalColor;
                    }

                    break;
            }
        }
    }
}