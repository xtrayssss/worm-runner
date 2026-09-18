using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.WindowFeature
{
    public sealed class CameraPriority : MonoBehaviour
    {
        [field: SerializeField]
        public int Priority { get; private set; }
    }
}