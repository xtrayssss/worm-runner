using System;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public class EnemyGroupConfig
    {
        [SerializeField]
        private EnemyFormationType _formationType;

        public EnemyFormationType FormationType
        {
            get => _formationType;
            set => _formationType = value;
        }
    }
}