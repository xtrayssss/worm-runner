#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class MineGenerationPreset : ObjectGenerationPreset
    {
        public override InteractiveObjectId ObjectType => InteractiveObjectId.MINE;

        public override void ApplyToConfig(InteractiveObjectData config) => 
            config.MineConfig = new MineConfig();
    }
}
#endif