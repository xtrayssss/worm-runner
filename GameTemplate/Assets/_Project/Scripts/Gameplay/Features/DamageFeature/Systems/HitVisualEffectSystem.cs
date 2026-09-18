// using _Project.Scripts.Gameplay.Features.CommonFeature;
// using _Project.Scripts.Gameplay.Features.DamageFeature.Behaviours;
// using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
// using _Project.Scripts.Gameplay.Features.DeathFeature.Components;
// using _Project.Scripts.Gameplay.Features.EntityViewFeature;
// using _Project.Scripts.Gameplay.Features.StatsFeature;
// using Scellecs.Morpeh;
// using Unity.IL2CPP.CompilerServices;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
// {
//     [Il2CppSetOption(Option.NullChecks, false)]
//     [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
//     [Il2CppSetOption(Option.DivideByZeroChecks, false)]
//     public class HitVisualEffectSystem : ISystem
//     {
//         public World World { get; set; }
//
//         private Filter _impactors;
//
//         private readonly HitVFXView _hitEffectPrefab;
//         private readonly ParticleSystem _hitParticle;
//
//         public HitVisualEffectSystem(ConfigsService configs)
//         {
//             _hitEffectPrefab = configs.GetHitEffectPrefab();
//             _hitParticle = configs.GetHitParticle();
//         }
//
//         public void OnAwake() =>
//             _impactors = World.Filter
//                 .With<DamageImpactEvent>()
//                 .Build();
//
//         public void OnUpdate(float deltaTime)
//         {
//             foreach (Entity impactor in _impactors)
//             {
//                 ref readonly DamageImpactEvent damageImpact = ref impactor.GetComponent<DamageImpactEvent>();
//
//                 if (!damageImpact.IsDirectDamageDealer || damageImpact.StatEffectType != StatEffectType.INSTANT ||
//                     damageImpact.IsHeroDamaged)
//                     continue;
//
//                 HitVFXView vfxView =
//                     Object.Instantiate(_hitEffectPrefab, damageImpact.Point, Quaternion.identity);
//
//                 Entity vfx = vfxView.Entity;
//
//                 vfx.AddComponent<SelfDestructTimer>().Time = vfxView.Duration;
//                 vfx.AddComponent<HitVFXTag>();
//
//                 Object.Instantiate(_hitParticle, damageImpact.Point, Quaternion.identity);
//
// #if DEBUG
//                 vfx.AddComponent<EntityName>().Value = vfxView.name;
// #endif
//             }
//         }
//
//         public void Dispose()
//         {
//         }
//     }
// }