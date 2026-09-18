using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.EntityViewFeature
{
    public class EntityView : MonoBehaviour
    {
        [ShowInInspector, HideInEditorMode] public Entity Entity { get; set; }
    }
}