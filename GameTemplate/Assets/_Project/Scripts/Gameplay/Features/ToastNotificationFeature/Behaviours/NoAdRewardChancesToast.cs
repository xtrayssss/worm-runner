using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ToastNotificationFeature.Behaviours
{
    public sealed class NoAdRewardChancesToast : BaseToastNotification
    {
        public void ShowNoAdRewardChancesMessage()
        {
            string message = "I18N.StringTable_Shop.AD_REWARD_NO_CHANCES_REMAINING";

            if (string.IsNullOrEmpty(message)) 
                message = "No more ad rewards available until next wave";

            ShowMessage(message);

#if DEBUG
            Debug.Log("No more ad reward chances available");
#endif
        }
    }
}