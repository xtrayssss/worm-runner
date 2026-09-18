using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Configs
{
    [CreateAssetMenu(fileName = nameof(UpgradeConfig),
        menuName = ProjectConfig.PROJECT_NAME + "/Configs/" + nameof(UpgradeConfig))]
    public class UpgradeConfig : ScriptableObject
    {
        public UpgradeType Type;

        public string Title;

        public Sprite Icon;

        public Color CardColor = Color.white;

        [ShowIf(nameof(Type), UpgradeType.ATTACK)]
        public float AttackBonus = 10f;

        [ShowIf(nameof(Type), UpgradeType.POPULATION)]
        public int MembersToAdd = 1;

        [ShowIf(nameof(Type), UpgradeType.INCOME)]
        public float IncomeMultiplier = 1.5f;

        [SerializeField]
        private int _cost;

        public float GetValue()
        {
            return Type switch
            {
                UpgradeType.ATTACK => AttackBonus,
                UpgradeType.INCOME => IncomeMultiplier,
                _ => 0f
            };
        }

        public int GetIntValue()
        {
            return Type switch
            {
                UpgradeType.POPULATION => MembersToAdd,
                _ => 0
            };
        }

        public int GetCost(int level)
        {
            level = Mathf.Max(0, level - 1);

            return (int)(_cost + level * 25f);
        }
    }
}