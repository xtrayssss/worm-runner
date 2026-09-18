using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class EnemyEmplacementView : InteractiveObjectView
    {
        [Header("Enemy Emplacement")]
        [SerializeField] private Transform[] _spawnPoints;
        
        public Transform[] SpawnPoints => _spawnPoints;
    }
}