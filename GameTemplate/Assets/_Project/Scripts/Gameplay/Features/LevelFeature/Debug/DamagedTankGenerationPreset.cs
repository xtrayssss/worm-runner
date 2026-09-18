using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;

#if UNITY_EDITOR
namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class DamagedTankGenerationPreset : ObjectGenerationPreset
    {
        public override InteractiveObjectId ObjectType => InteractiveObjectId.DAMAGED_TANK;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
        }
    }
}
#endif