using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Configs
{
    [Serializable]
    public class DefenseModeConfig
    {
        [SerializeField] 
        private Wave[] _waves;

        public Wave[] Waves => _waves;
        
        public readonly Vector3 SpawnAreaCenter = new Vector3(0f, 0f, 100f);
    }

    [Serializable]
    public class Wave
    {
        [SerializeField] private EnemySpawnData[] _enemies;
        [SerializeField] private float _delayBeforeStart = 3f;

        public EnemySpawnData[] Enemies => _enemies;
        public float DelayBeforeStart => _delayBeforeStart;
    }

    [Serializable]
    public class EnemySpawnData
    {
        [SerializeField] private InteractiveObjectId _type;
        [SerializeField] private float _spawnTime;

        public InteractiveObjectId Type => _type;
        public float SpawnTime => _spawnTime;
    }
}