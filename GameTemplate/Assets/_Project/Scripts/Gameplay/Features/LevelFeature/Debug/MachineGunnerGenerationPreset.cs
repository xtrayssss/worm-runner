#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class MachineGunnerGenerationPreset : ObjectGenerationPreset
    {
        public override InteractiveObjectId ObjectType => InteractiveObjectId.MACHINE_GUNNER_ENEMY;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
            config.EnemyConfig = new EnemyConfig
            {
                MachineGunnerEnemyConfig = new MachineGunnerEnemyConfig(),
            };
        }
    }
}
#endif