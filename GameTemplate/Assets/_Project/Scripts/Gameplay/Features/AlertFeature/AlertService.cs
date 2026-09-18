using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;

namespace _Project.Scripts.Gameplay.Features.AlertFeature
{
    public sealed class AlertService : IService
    {
        private readonly List<Alert> _alerts = new List<Alert>();

        public bool AreAlertsEnabled { get; private set; } = true;

        public void RegisterAlert(Alert alert) =>
            _alerts.Add(alert);

        public void UnregisterAlert(Alert alert) =>
            _alerts.Remove(alert);

        public void EnableAlerts()
        {
            AreAlertsEnabled = true;
            
            foreach (Alert alert in _alerts)
            {
                alert.SetVisible(true);
            }
        }

        public void DisableAlerts()
        {
            AreAlertsEnabled = false;
            
            foreach (Alert alert in _alerts)
            {
                alert.SetVisible(false);
            }
        }

        public void DisableAlert(Alert alert) => 
            alert.SetVisible(false);
    }
}