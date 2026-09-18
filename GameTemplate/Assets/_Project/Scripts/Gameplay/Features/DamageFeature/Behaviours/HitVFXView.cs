using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Behaviours
{
    public sealed class HitVFXView : EntityView
    {
        [field: SerializeField] public float Duration { get; private set; }
    }
}