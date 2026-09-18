using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ToastNotificationFeature.Behaviours
{
    public sealed class NoEquippedArsenalToast : BaseToastNotification
    {
        public void ShowNoEquippedArsenalMessage()
        {
            string message = "I18N.StringTable_UI_Battle.NO_EQUIPPED_ARSENAL";
            
            if (string.IsNullOrEmpty(message)) 
                message = "Equip at least one arsenal to enter battle";
                
            ShowMessage(message);

#if DEBUG
            Debug.Log("Cannot enter battle without equipped arsenal");
#endif
        }
    }
}