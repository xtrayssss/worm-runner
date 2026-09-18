using System;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    public record BaseInteractiveObjectConfig<TConfig> where TConfig : class
    {
        [NonSerialized]
        protected TConfig BaseConfig;

        public virtual void SetBaseConfig(TConfig baseConfig) =>
            BaseConfig = baseConfig;
    }
}