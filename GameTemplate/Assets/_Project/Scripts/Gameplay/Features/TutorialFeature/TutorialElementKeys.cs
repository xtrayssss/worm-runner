using System.Collections.Generic;
using System.Linq;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public static class TutorialElementKeys
    {
        public const string UPGRADE_CARD_POPULATION = "UpgradeCardPopulation";
        public const string UPGRADE_CARD_ATTACK = "UpgradeCardAttack";
        public const string UPGRADE_CARD_EVOLUTION = "UpgradeCardEvolution";
        public const string UPGRADE_CARD_INCOME = "UpgradeCardIncome";

        public static IEnumerable<string> GetAllKeys()
        {
            return typeof(TutorialElementKeys)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(static f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(static f => (string)f.GetValue(null))
                .OrderBy(static key => key);
        }
    }
}