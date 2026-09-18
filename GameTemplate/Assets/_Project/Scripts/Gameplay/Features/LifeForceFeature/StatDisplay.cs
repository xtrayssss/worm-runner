using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature
{
    public abstract class StatDisplay : MonoBehaviour
    {
        public virtual void UpdateDisplay(float currentValue, float maxValue)
        {
        }

        public abstract void Show();
        public abstract void Hide();
        public abstract bool IsVisible { get; }
    }
}