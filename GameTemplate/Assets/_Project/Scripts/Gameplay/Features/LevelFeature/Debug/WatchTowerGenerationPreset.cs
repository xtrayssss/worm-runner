#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.StealthFeature.Configs;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class WatchTowerGenerationPreset : ObjectGenerationPreset
    {
        public override InteractiveObjectId ObjectType => InteractiveObjectId.WATCH_TOWER;

        public override void ApplyToConfig(InteractiveObjectData config) =>
            config.WatchTowerConfig = new WatchTowerConfig();
    }
}
#endif