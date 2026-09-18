using System;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature
{
    [Serializable]
    public struct CurrencySwirlGatherAnimationConfig
    {
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public float Delay;
        public float Duration;
        public float CurveStartIntensity;
        public float CurveEndIntensity;
        public float CurveStartAngle;
        public float CurveEndAngle;
        public CurrencyTarget Target;
        [FormerlySerializedAs("Coin")] public CurrencyView Currency;
    }
}