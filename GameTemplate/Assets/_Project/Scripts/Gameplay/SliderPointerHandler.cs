using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Gameplay
{
    public class SliderPointerHandler : MonoBehaviour, IPointerUpHandler
    {
        public event Action OnSliderReleased;

        public void OnPointerUp(PointerEventData eventData)
        {
            OnSliderReleased?.Invoke();
        }
    }
}