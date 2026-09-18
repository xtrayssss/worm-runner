using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.AlertFeature
{
    public sealed class Alert : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private AlertService _alertService;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _alertService = AllServices.Instance.Get<AlertService>();
            _alertService.RegisterAlert(this);

            if (!_alertService.AreAlertsEnabled)
            {
                _alertService.DisableAlert(this);
            }
        }

        public void SetVisible(bool visible)
        {
            float targetAlpha = visible ? 1f : 0f;
            _canvasGroup.alpha = targetAlpha;
        }

        private void OnDestroy() =>
            _alertService?.UnregisterAlert(this);
    }
}