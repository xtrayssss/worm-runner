using System;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature
{
    [Serializable]
    public struct CurrencySpreadAttractAnimationConfig
    {
        public float Duration;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public CurrencyView CurrencyView;
        public CurrencyTarget Target;
    }
}