using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Components
{
    public sealed class EntityReference : MonoBehaviour
    {
        [field: SerializeField] public EntityView EntityView { get; private set; }
    }
}