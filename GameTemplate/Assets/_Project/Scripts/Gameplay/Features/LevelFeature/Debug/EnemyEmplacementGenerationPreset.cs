#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class EnemyEmplacementGenerationPreset : ObjectGenerationPreset
    {
        public override InteractiveObjectId ObjectType => InteractiveObjectId.ENEMY_EMPLACEMENT;

        public override void ApplyToConfig(InteractiveObjectData config) =>
            config.EnemyEmplacementConfig = new EnemyEmplacementConfig();
    }
}
#endif

