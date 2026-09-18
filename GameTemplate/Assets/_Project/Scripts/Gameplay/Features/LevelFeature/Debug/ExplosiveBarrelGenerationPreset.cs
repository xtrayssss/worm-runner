#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class ExplosiveBarrelGenerationPreset : ObjectGenerationPreset
    {
        public override InteractiveObjectId ObjectType => InteractiveObjectId.EXPLOSIVE_BARREL;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
        }
    }
}

#endif