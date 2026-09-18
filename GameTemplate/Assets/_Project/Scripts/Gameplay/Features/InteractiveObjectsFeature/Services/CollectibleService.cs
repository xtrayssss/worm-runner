using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services
{
    [Serializable]
    public sealed class CollectibleService : IService
    {
        private Filter _allCollectibles;

        [ShowInInspector]
        private int _totalCollectibles;

        [ShowInInspector]
        private int _collectedCount;

        [ShowInInspector]
        private readonly Dictionary<CollectibleType, int> _collectedByType = new Dictionary<CollectibleType, int>();

        private readonly World _world;

        [ShowInInspector]
        public bool AllCollected => _totalCollectibles > 0 && _collectedCount >= _totalCollectibles;

        public int CollectedCount => _collectedCount;
        public int TotalCollectibles => _totalCollectibles;
        
        public CollectibleService()
        {
            _world = World.Default;

            _allCollectibles = _world!.Filter
                .With<CollectibleTag>()
                .Build();
        }

        public void Setup()
        {
            _world.Commit();
            _totalCollectibles = _allCollectibles.GetLengthSlow();
        }

        public void AddCollectedCollectible(CollectibleType type)
        {
            _collectedCount++;

            _collectedByType.TryAdd(type, 0);
            _collectedByType[type] += 1;
        }

        public void Reset()
        {
            _collectedCount = 0;
            _totalCollectibles = 0;
            _collectedByType.Clear();
        }
    }
}