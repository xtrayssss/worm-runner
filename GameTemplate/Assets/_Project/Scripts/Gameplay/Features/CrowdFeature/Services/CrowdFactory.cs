using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Systems;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.LifeForceFeature;
using _Project.Scripts.Gameplay.Features.LifeForceFeature.Components;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Cinemachine;
using Scellecs.Morpeh;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Services
{
    public sealed class CrowdFactory : IService
    {
        private static readonly Vector3 BASE_CROWD_POSITION = Vector3.forward * 10;

        private readonly ConfigsService _configs;
        private readonly CrowdEvolutionSystem _crowdEvolutionSystem;
        private readonly EnhancementService _enhancementService;

        private readonly VFXService _vfxService;
        private readonly Filter _crowds;
        private readonly AudioService _audioService;

        public CrowdFactory(
            ConfigsService configs,
            CrowdEvolutionSystem crowdEvolutionSystem,
            EnhancementService enhancementService,
            VFXService vfxService,
            AudioService audioService)
        {
            _configs = configs;
            _crowdEvolutionSystem = crowdEvolutionSystem;
            _enhancementService = enhancementService;
            _vfxService = vfxService;
            _audioService = audioService;

            _crowds = World.Default!.Filter
                .With<CrowdTag>()
                .Build();
        }

        public Entity CreateCrowdMember(
            int evolutionLevel,
            int formationIndex,
            CrowdMemberType memberType)
        {
            Entity crowd = _crowds.First();

            return memberType switch
            {
                CrowdMemberType.MILITARY => CreateMilitaryCrowdMember(crowd, evolutionLevel, formationIndex),
                CrowdMemberType.CIVILIAN => CreateCivilianCrowdMember(crowd, formationIndex),
                _ => throw new ArgumentOutOfRangeException(nameof(memberType), memberType, null)
            };
        }

        public Entity CreateCrowd()
        {
            World world = World.Default;

            CrowdConfig crowdConfig = _configs.GetCrowdConfig();

            Entity crowd = world.CreateEntity();

            crowd.AddComponent<CrowdTag>();

            CrowdView crowdView = Object.Instantiate(crowdConfig.CrowdPrefab, BASE_CROWD_POSITION, Quaternion.identity);
            CinemachineVirtualCamera followCamera = Object.Instantiate(crowdConfig.FollowCameraPrefab);
            followCamera.Follow = crowdView.transform;
            crowdView.Entity = crowd;
            crowdView.SetFollowCamera(followCamera);

            crowd.AddComponent<Position>() = new Position
            {
                Value = crowdView.transform.position
            };

            crowd.AddComponent<CharacterControllerLink>() = new CharacterControllerLink
            {
                Controller = crowdView.CharacterController
            };

            crowd.AddComponent<MovementState>();

            crowd.AddComponent<EntityViewLink>() = new EntityViewLink
            {
                View = crowdView
            };

            crowd.AddComponent<CrowdMovableBounds>();

            crowd.AddComponent<CrowdPopulation>() = new CrowdPopulation
            {
                MaxMilitaryPopulation = crowdConfig.MaxPopulation,

                MilitaryPopulation = 0,
                MilitaryMembers = new List<Entity>(capacity: crowdConfig.MaxPopulation),

                CivilianPopulation = 0,
                CivilianMembers = new List<Entity>(capacity: crowdConfig.MaxPopulation)
            };

            crowd.AddComponent<CrowdOrganization>() = new CrowdOrganization
            {
                OrganizationDuration = 1f
            };

            LevelService levelService = AllServices.Instance.Get<LevelService>();
            crowd.AddComponent<MovementSettings>() = new MovementSettings
            {
                HorizontalMoveSpeed = crowdConfig.HorizontalMoveSpeed,
                ForwardMoveSpeed = levelService.CurrentLevel.LevelMode == LevelMode.DEFENSE
                    ? 0f
                    : crowdConfig.ForwardMoveSpeed,
                HorizontalSmoothingTime = crowdConfig.HorizontalSmoothingTime,
                ForwardSmoothingTime = crowdConfig.ForwardSmoothingTime
            };

#if DEBUG
            crowd.AddComponent<EntityName>().Value = $"Crowd_{crowd.Id}";
#endif

            return crowd;
        }

        private Entity CreateBaseCrowdMember(
            Entity crowd,
            int formationIndex,
            CrowdMemberType memberType)
        {
            World world = World.Default;
            Entity member = world.CreateEntity();

            ref readonly EntityViewLink crowdViewLink = ref crowd.GetComponent<EntityViewLink>();
            CrowdView crowdView = (CrowdView)crowdViewLink.View;

            CrowdMemberView memberView = Object.Instantiate(GetCrowdMemberPrefab(), crowdView.transform);

            memberView.Construct();

            memberView.name = $"{memberType}_Member_{formationIndex}";
            memberView.Entity = member;

            member.AddComponent<CrowdMemberTag>();

            member.AddComponent<EntityViewLink>() = new EntityViewLink
            {
                View = memberView
            };

            member.AddComponent<RigidbodyLink>() = new RigidbodyLink
            {
                Value = memberView.Rigidbody
            };

            member.AddComponent<Owner>().Value = crowd;

            member.AddComponent<ColliderLink>() = new ColliderLink
            {
                Value = memberView.Collider
            };

            _enhancementService.ApplyGlobalEffectsToMember(member);

#if DEBUG
            member.AddComponent<EntityName>().Value = $"{memberType}_Member_{member.Id}";
#endif

            return member;

            CrowdMemberView GetCrowdMemberPrefab()
            {
                CrowdConfig crowdConfig = _configs.GetCrowdConfig();

                return memberType switch
                {
                    CrowdMemberType.MILITARY => crowdConfig.MilitaryMemberPrefab,
                    CrowdMemberType.CIVILIAN => crowdConfig.CivilianMemberPrefab,
                    _ => throw new ArgumentOutOfRangeException(nameof(memberType), memberType, null)
                };
            }
        }

        private Entity CreateMilitaryCrowdMember(
            Entity crowd,
            int evolutionLevel,
            int formationIndex)
        {
            Entity member = CreateBaseCrowdMember(
                crowd,
                formationIndex,
                CrowdMemberType.MILITARY);

            ref readonly EntityViewLink memberViewLink = ref member.GetComponent<EntityViewLink>();
            MilitaryCrowdMemberView memberView = (MilitaryCrowdMemberView)memberViewLink.View;
            memberView.Construct(_vfxService, _audioService);

            member.AddComponent<MilitaryCrowdMemberTag>();

            CrowdConfig crowdConfig = _configs.GetCrowdConfig();

            const float HEALTH = 15f;

            ref Stats stats = ref member.AddComponent<Stats>();
            
            stats = new StatsBuilder()
                .SetStat(StatId.DAMAGE, crowdConfig.Damage)
                .SetStat(StatId.COOLDOWN, crowdConfig.ShootingCooldown)
                .SetStat(StatId.SHOOTING_RANGE, crowdConfig.ShootingRange)
                .SetStat(StatId.HEALTH, HEALTH, useBaseValueAsMaximum: false)
                .Build();

            member.AddComponent<AttackCooldown>() = new AttackCooldown
            {
                Cooldown = stats.Value[StatId.COOLDOWN].CurrentValue
            };

            member.AddComponent<Damage>() = new Damage
            {
                CurrentDamage = stats.Value[StatId.DAMAGE].CurrentValue
            };

            member.AddComponent<Health>() = new Health
            {
                CurrentHealth = stats.Value[StatId.HEALTH].CurrentValue,
                MaxHealth = stats.Value[StatId.HEALTH].CachedModifiedBaseValue
            };

            member.AddComponent<ShootingRange>() = new ShootingRange
            {
                CurrentValue = stats.Value[StatId.SHOOTING_RANGE].CurrentValue
            };

            member.AddComponent<StatModifierSetups>().Value = new StatModifierSetup[]
            {
                new HealthModifierSetup
                {
                    TargetStatId = StatId.HEALTH,
                    Operation = StatOperation.ADD,
                    Priority = (int)StatOperation.ADD,
                    EffectType = StatEffectType.INSTANT,
                    AffectsBaseValue = false,
                    SourceStatId = StatId.DAMAGE
                }
            };
            
            member.AddComponent<HealthDisplayLink>() = new HealthDisplayLink
            {
                Value = memberView.HealthBar
            };

            member.AddComponent<HealthDisplaySettings>() = new HealthDisplaySettings
            {
                UpdateMode = StatDisplayUpdateMode.ALWAYS
            };

            member.AddComponent<SpriteRendererLink>() = new SpriteRendererLink
            {
                Value = memberView.SpriteRenderer
            };

            member.AddComponent<CrowdMemberEvolution>() = new CrowdMemberEvolution
            {
                EvolutionLevel = 1
            };
            
            _crowdEvolutionSystem.Evolve(member, evolutionLevel);

            return member;
        }

        private Entity CreateCivilianCrowdMember(
            Entity crowd,
            int formationIndex)
        {
            Entity member = CreateBaseCrowdMember(
                crowd,
                formationIndex,
                CrowdMemberType.CIVILIAN);

            ref Stats stats = ref member.AddComponent<Stats>();
            stats = new StatsBuilder()
                .SetStat(StatId.HEALTH, 1, useBaseValueAsMaximum: false)
                .Build();

            member.AddComponent<Health>() = new Health
            {
                CurrentHealth = stats.Value[StatId.HEALTH].CurrentValue,
                MaxHealth = stats.Value[StatId.HEALTH].CachedModifiedBaseValue
            };

            member.AddComponent<CivilianCrowdMemberTag>();

            return member;
        }

        public DecorativeMemberView CreateDecorativeMember(Vector3 position, Transform parent, int evolutionLevel)
        {
            DecorativeMemberView memberPrefab = _configs.GetDecorativeMemberPrefab();
            
            DecorativeMemberView memberView = Object.Instantiate(
                memberPrefab,
                position,
                memberPrefab.transform.rotation,
                parent);

            memberView.Construct();

            memberView.name = $"Decorative_Member_{evolutionLevel}";

            CrowdConfig crowdConfig = _configs.GetCrowdConfig();

            memberView.SpriteRenderer.sprite = crowdConfig.GetEvolution(evolutionLevel).Sprite;

            return memberView;
        }
    }
}