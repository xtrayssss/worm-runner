using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours
{
    public sealed class WatchTowerView : StealthObjectView
    {
        [SerializeField]
        private Transform _startPoint;

        [SerializeField]
        private Transform _endPoint;

        [SerializeField]
        private Transform _controlPoint;

        public Transform StartPoint => _startPoint;
        public Transform EndPoint => _endPoint;
        public Transform ControlPoint => _controlPoint;
    }
}