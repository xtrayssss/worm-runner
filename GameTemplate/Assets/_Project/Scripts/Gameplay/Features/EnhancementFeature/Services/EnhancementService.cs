using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Components;
using _Project.Scripts.Gameplay.Features.GameFeature.Configs;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Services
{
    [Serializable]
    public sealed class EnhancementService : IService
    {
        private readonly StatsService _statsService;
        private readonly VFXService _vfxService;
        private readonly Filter _crowdMembers;

        [ShowInInspector]
        private readonly List<EffectData> _effects = new List<EffectData>(capacity: 8);

        [ShowInInspector]
        public Dictionary<UpgradeType, UpgradeData> Upgrades { get; private set; }

        private VFXData _upgradeVFX;

        private World _world;
        private readonly ConfigsService _configsService;
        private readonly SaveLoadService _saveLoadService;

        public VFXData UpgradeVFX => _upgradeVFX;

        public EnhancementService(
            StatsService statsService,
            VFXService vfxService,
            ConfigsService configsService,
            SaveLoadService saveLoadService)
        {
            _statsService = statsService;
            _vfxService = vfxService;
            _world = World.Default;

            _configsService = configsService;
            _saveLoadService = saveLoadService;

            _crowdMembers = _world!.Filter
                .With<MilitaryCrowdMemberTag>()
                .With<Stats>()
                .Build();

            GameConfig gameConfig = configsService.GetGameConfig();
            _upgradeVFX = gameConfig.UpgradeVFX;

            LoadFromSave();
        }

        [Serializable]
        public class EffectData
        {
            public EffectType Type;
            public StatId TargetStat;
            public StatOperation Operation;
            public float Value;
        }

        [Serializable]
        public sealed class UpgradeData
        {
            [ShowInInspector]
            public int Level { get; set; } = 1;

            [ShowInInspector]
            public float CurrentValue { get; set; } = 1f;
        }

        public bool ApplyUpgrade(in ApplyUpgradeRequest request, Vector3 vfxPosition)
        {
            Upgrades[request.UpgradeType].Level += 1;

            bool wasApplied = request.UpgradeType switch
            {
                UpgradeType.ATTACK => ApplyAttackUpgrade(request.Value, request.UpgradeSource, vfxPosition),
                UpgradeType.POPULATION => ApplyAddMemberUpgrade(request.IntValue, vfxPosition),
                UpgradeType.EVOLUTION => ApplyEvolutionUpgrade(vfxPosition),
                UpgradeType.INCOME => ApplyIncomeUpgrade(request.Value, vfxPosition),
                _ => throw new ArgumentOutOfRangeException(nameof(request.UpgradeType), request.UpgradeType, null)
            };

            if (wasApplied)
                SaveToFile();

            return wasApplied;
        }

        public void ApplyGlobalEffectsToMember(Entity member)
        {
            foreach (KeyValuePair<UpgradeType, UpgradeData> upgrade in Upgrades)
            {
                if (upgrade.Key == UpgradeType.ATTACK)
                {
                    ApplyStatModifierToMember(
                        member,
                        new EffectData
                        {
                            Type = EffectType.STAT_MODIFICATION,
                            Operation = StatOperation.MULTIPLY,
                            Value = upgrade.Value.CurrentValue,
                            TargetStat = StatId.DAMAGE
                        },
                        affectsBaseValue: true);
                }
            }

            foreach (EffectData effect in _effects)
            {
                if (effect.Type == EffectType.STAT_MODIFICATION)
                    ApplyStatModifierToMember(member, effect, affectsBaseValue: false);
            }
        }

        public void ApplyEffect(
            Entity source,
            Entity crowd,
            float effectValue,
            Vector3 vfxPosition,
            EffectType effectType)
        {
            EffectData effectData = CreateEffectData(source, effectType, effectValue);

            if (ShouldStoreGlobalEffect(effectType))
                _effects.Add(effectData);

            _vfxService.PlayVFX(_upgradeVFX, vfxPosition);

            ApplyEffectByType(source, crowd, effectData);
        }

        public void ResetEffects() =>
            _effects.Clear();

        public void ResetUpgrades(params UpgradeType[] upgradeTypes)
        {
            foreach (UpgradeType type in upgradeTypes)
                ResetUpgrade(type);

            SaveToFile();
        }

        public void ResetUpgrade(UpgradeType upgradeType)
        {
            if (!Upgrades.TryGetValue(upgradeType, out UpgradeData data))
                return;

            data.Level = 1;
            data.CurrentValue = upgradeType == UpgradeType.ATTACK ? 0f : 1f;
        }

        public static string GetEffectDisplayName(EffectType effectType, StatId statId)
        {
            return effectType switch
            {
                EffectType.STAT_MODIFICATION => GetStatDisplayName(statId),
                EffectType.POPULATION_ADD => "Population+",
                EffectType.POPULATION_MULTIPLY => "Population×",
                EffectType.POPULATION_SUBTRACT => "Population-",
                EffectType.EVOLUTION_UPGRADE => "Evolution+",
                EffectType.EVOLUTION_DOWNGRADE => "Evolution-",
                _ => "Unknown Effect"
            };
        }

        public static string GetStatDisplayName(StatId statId)
        {
            return statId switch
            {
                StatId.DAMAGE => "Damage",
                StatId.COOLDOWN => "Fire Rate",
                StatId.HEALTH => "Health",
                StatId.SPEED => "Movement Speed",
                StatId.SHOOTING_RANGE => "Fire Range",
                _ => statId.ToString()
            };
        }

        private void ApplyEffectByType(Entity source, Entity crowd, EffectData effectData)
        {
            switch (effectData.Type)
            {
                case EffectType.STAT_MODIFICATION:
                    ApplyStatModificationEffect(source, effectData);
                    break;

                case EffectType.POPULATION_ADD:
                    ApplyPopulationAddEffect(crowd, GetPopulationModificationValue(source));
                    break;

                case EffectType.POPULATION_MULTIPLY:
                    ApplyPopulationMultiplyEffect(crowd, GetPopulationModificationValue(source));
                    break;

                case EffectType.POPULATION_SUBTRACT:
                    ApplyPopulationSubtractEffect(crowd, GetPopulationModificationValue(source));
                    break;

                case EffectType.EVOLUTION_UPGRADE:
                    ApplyEvolutionChangeEffect(1);
                    break;

                case EffectType.EVOLUTION_DOWNGRADE:
                    ApplyEvolutionChangeEffect(-1);
                    break;
            }
        }

        private bool ApplyAttackUpgrade(float attackBonus, Entity upgradeSource, Vector3 vfxPosition)
        {
            Upgrades[UpgradeType.ATTACK].CurrentValue += attackBonus;

            _vfxService.PlayVFX(_upgradeVFX, vfxPosition);

            foreach (Entity member in _crowdMembers)
            {
                ApplyStatModifierToMember(
                    member,
                    new EffectData
                    {
                        Type = EffectType.STAT_MODIFICATION,
                        Operation = StatOperation.MULTIPLY,
                        Value = attackBonus,
                        TargetStat = StatId.DAMAGE
                    },
                    producer: upgradeSource,
                    affectsBaseValue: true);
            }

            return true;
        }

        private bool ApplyAddMemberUpgrade(int memberCount, Vector3 vfxPosition)
        {
            CrowdConfig crowdConfig = _configsService.GetCrowdConfig();

            int availableSpace = crowdConfig.MaxPopulation - (int)Upgrades[UpgradeType.POPULATION].CurrentValue;

            if (availableSpace <= 0)
                return false;

            Upgrades[UpgradeType.POPULATION].CurrentValue += memberCount;
            _vfxService.PlayVFX(_upgradeVFX, vfxPosition);

            _world
                .GetRequest<AddCrowdMembersRequest>()
                .Publish(new AddCrowdMembersRequest
                {
                    Count = memberCount,
                    EvolutionLevel = (int)Upgrades[UpgradeType.EVOLUTION].CurrentValue,
                    AnimationType = CrowdMemberAnimationType.IDLE
                }, allowNextFrame: true);

            return true;
        }

        private bool ApplyEvolutionUpgrade(Vector3 vfxPosition)
        {
            CrowdConfig crowdConfig = _configsService.GetCrowdConfig();

            int evolutionLevel = (int)Upgrades[UpgradeType.EVOLUTION].CurrentValue;

            if (evolutionLevel >= crowdConfig.MaxEvolutionLevel)
                return false;

            evolutionLevel += 1;

            Upgrades[UpgradeType.EVOLUTION].CurrentValue = evolutionLevel;

            _vfxService.PlayVFX(_upgradeVFX, vfxPosition);

            Request<EvolutionCrowdRequest> evolutionRequest = _world.GetRequest<EvolutionCrowdRequest>();

            foreach (Entity member in _crowdMembers)
            {
                evolutionRequest.Publish(new EvolutionCrowdRequest
                {
                    Member = member,
                    EvolutionLevel = evolutionLevel
                }, allowNextFrame: true);
            }

            return true;
        }

        private bool ApplyIncomeUpgrade(float multiplier, Vector3 vfxPosition)
        {
            Upgrades[UpgradeType.INCOME].CurrentValue += multiplier;
            _vfxService.PlayVFX(_upgradeVFX, vfxPosition);

            return true;
        }

        private void ApplyStatModificationEffect(Entity source, EffectData effectData)
        {
            foreach (Entity member in _crowdMembers)
            {
                ApplyStatModifierToMember(
                    member,
                    effectData,
                    producer: source,
                    affectsBaseValue: false);
            }
        }

        private void ApplyPopulationAddEffect(Entity crowd, int addCount)
        {
            _world
                .GetRequest<AddCrowdMembersRequest>()
                .Publish(new AddCrowdMembersRequest
                {
                    Count = addCount,
                    AnimationType = CrowdMemberAnimationType.JUMP
                }, allowNextFrame: true);
        }

        private void ApplyPopulationMultiplyEffect(Entity crowd, float multiplier)
        {
            _world
                .GetRequest<MultiplyCrowdRequest>()
                .Publish(new MultiplyCrowdRequest
                {
                    Ratio = multiplier
                }, allowNextFrame: true);
        }

        private void ApplyPopulationSubtractEffect(Entity crowd, int subtractCount)
        {
            _world
                .GetRequest<RemoveCrowdMembersRequest>()
                .Publish(new RemoveCrowdMembersRequest
                {
                    Count = subtractCount
                }, allowNextFrame: true);
        }

        private void ApplyEvolutionChangeEffect(int levelChange)
        {
            foreach (Entity unit in _crowdMembers)
            {
                if (!unit.Has<CrowdMemberEvolution>())
                    continue;

                ref CrowdMemberEvolution evolution = ref unit.GetComponent<CrowdMemberEvolution>();
                evolution.EvolutionLevel = Mathf.Max(1, evolution.EvolutionLevel + levelChange);
            }
        }

        private EffectData CreateEffectData(Entity source, EffectType effectType, float effectValue)
        {
            EffectData effectData = new EffectData
            {
                Type = effectType,
                Value = effectValue,
            };

            if (effectType == EffectType.STAT_MODIFICATION)
            {
                ref readonly StatModificationEffect statEffect = ref source.GetComponent<StatModificationEffect>();
                effectData.TargetStat = statEffect.TargetStat;
                effectData.Operation = StatOperation.ADD;
                bool isBuff = effectValue > 0f;
                effectData.Value =
                    CalculateEffectValue(effectData.TargetStat, effectData.Value, isPositiveEffect: isBuff);

                float CalculateEffectValue(StatId statId, float rawEffectValue, bool isPositiveEffect)
                {
                    if (statId == StatId.COOLDOWN || statId == StatId.DAMAGE)
                        return isPositiveEffect ? -Mathf.Abs(rawEffectValue) : Mathf.Abs(rawEffectValue);

                    return isPositiveEffect ? rawEffectValue : -rawEffectValue;
                }
            }

            return effectData;
        }

        private void ApplyStatModifierToMember(
            Entity member,
            EffectData effectData,
            bool affectsBaseValue,
            Entity? producer = null)
        {
            SimpleStatModifierSetup modifierSetup = new SimpleStatModifierSetup
            {
                TargetStatId = effectData.TargetStat,
                Operation = effectData.Operation,
                ModifierValue = effectData.Value,
                Priority = (int)effectData.Operation,
                EffectType = StatEffectType.INSTANT,
                AffectsBaseValue = affectsBaseValue
            };

            _statsService.CreateStatModifier(member, modifierSetup, producer.GetValueOrDefault());
        }

        private bool IsStatModificationDebuff(float effectValue, StatId? statId)
        {
            if (!statId.HasValue)
                return effectValue < 0;

            bool isInverseBeneficial = StatsService.INVERSE_BENEFICIAL_STATS.Contains(statId.Value);
            return isInverseBeneficial ? effectValue > 0 : effectValue < 0;
        }

        private static bool ShouldStoreGlobalEffect(EffectType effectType)
        {
            return effectType == EffectType.STAT_MODIFICATION;
        }

        private int GetPopulationModificationValue(Entity source)
        {
            ref readonly PopulationModificationEffect populationEffect =
                ref source.GetComponent<PopulationModificationEffect>();

            return populationEffect.Value;
        }

        private void LoadFromSave()
        {
            if (_saveLoadService.PlayerSaveData.EnhancementData.Upgrades.Count == 0)
            {
                Upgrades = new Dictionary<UpgradeType, UpgradeData>
                {
                    {
                        UpgradeType.ATTACK, new UpgradeData
                        {
                            CurrentValue = 0f
                        }
                    },
                    { UpgradeType.POPULATION, new UpgradeData() },
                    { UpgradeType.EVOLUTION, new UpgradeData() },
                    { UpgradeType.INCOME, new UpgradeData() }
                };

                SaveToFile();
            }
            else
            {
                Upgrades = _saveLoadService.PlayerSaveData.EnhancementData.Upgrades
                    .ToDictionary(
                        static kvp => (UpgradeType)kvp.Key,
                        static kvp => new UpgradeData
                        {
                            Level = kvp.Value.Level,
                            CurrentValue = kvp.Value.CurrentValue
                        });
            }
        }

        private void SaveToFile()
        {
            _saveLoadService.PlayerSaveData.EnhancementData.Upgrades = Upgrades.ToDictionary(
                static upgrade => (int)upgrade.Key,
                static upgrade => new PlayerSaveData.EnhancementSaveData.UpgradeSaveData
                {
                    Level = upgrade.Value.Level,
                    CurrentValue = upgrade.Value.CurrentValue
                });

            _saveLoadService.SaveData();
        }
    }
}