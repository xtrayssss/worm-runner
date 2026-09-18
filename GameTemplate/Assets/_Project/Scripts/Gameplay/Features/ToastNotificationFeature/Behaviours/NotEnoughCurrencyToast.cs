using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ToastNotificationFeature.Behaviours
{
    public sealed class NotEnoughCurrencyToast : BaseToastNotification
    {
        public void ShowMessage(CurrencyType currencyType)
        {   
            string currencyName = "ResourceLocalization.GetCurrencyGenitiveName(currencyType)";

            string message = string.Format("I18N.StringTable_Shop.UI_TOAST_INSUFFICIENT_CURRENCY", currencyName);

            message = message.Replace("\r", "");          
            
            ShowMessage(message);

#if DEBUG
            Debug.Log("Not enough " + currencyName);
#endif
        }
    }
}