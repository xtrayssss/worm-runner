using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.StealthFeature.Configs;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class PatrolGuardGenerationPreset : ObjectGenerationPreset
    {
        public override InteractiveObjectId ObjectType => InteractiveObjectId.PATROL_GUARD;

        public override void ApplyToConfig(InteractiveObjectData config) => 
            config.PatrolGuardConfig = new PatrolGuardConfig();
    }
}