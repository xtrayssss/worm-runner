namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature
{
    public enum EffectType
    {
        NONE = -1,
        STAT_MODIFICATION = 0,
        POPULATION_ADD = 1,
        POPULATION_MULTIPLY = 2,
        POPULATION_SUBTRACT = 3,
        EVOLUTION_UPGRADE = 5,
        EVOLUTION_DOWNGRADE = 6
    }

    public static class EffectTypeExtensions
    {
        private static readonly EffectType[] POPULATION_EFFECTS =
        {
            EffectType.POPULATION_ADD,
            EffectType.POPULATION_MULTIPLY,
            EffectType.POPULATION_SUBTRACT
        };

        private static readonly EffectType[] EVOLUTION_EFFECTS =
        {
            EffectType.EVOLUTION_UPGRADE,
            EffectType.EVOLUTION_DOWNGRADE
        };

        public static bool IsPopulationEffect(this EffectType effectType) =>
            System.Array.Exists(POPULATION_EFFECTS, e => e == effectType);

        public static bool IsEvolutionEffect(this EffectType effectType) =>
            System.Array.Exists(EVOLUTION_EFFECTS, e => e == effectType);
    }
}