using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using PrimeTween;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DamageFlashSystem : ISystem
    {
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvent;

        private static readonly int FLASH_AMOUNT = Shader.PropertyToID("_FlashAmount");

        public void OnAwake() =>
            _damagedEvent = World.GetEvent<DamagedEvent>();

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvent.publishedChanges)
            {
                Entity damageable = damagedEvent.Damageable;

                if (damageable.Has<DamageFlash>())
                {
                    StatEffectType statEffectType = damagedEvent.Modifier.StatEffectType;

                    if (statEffectType != StatEffectType.INSTANT)
                        continue;

                    ref DamageFlash damageFlash = ref damageable.GetComponent<DamageFlash>();
                    ref PartRenderers renderers = ref damageable.GetComponent<PartRenderers>();

                    foreach (PartRenderers.Renderer renderer in renderers.Value)
                        renderer.MeshRenderer.material = damageFlash.Material;

                    damageFlash.Tween.Stop();

                    damageFlash.Tween = Tween.Custom(
                            endValue: 0.5f,
                            startValue: 0.2f,
                            duration: damageFlash.Duration,
                            ease: Ease.OutExpo,
                            target: renderers.Value,
                            cycles: 2,
                            cycleMode: CycleMode.Yoyo,
                            onValueChange: static (renderers, value) =>
                            {
                                foreach (PartRenderers.Renderer renderer in renderers)
                                    renderer.MeshRenderer.material.SetFloat(FLASH_AMOUNT, value);
                            })
                        .OnComplete(renderers.Value, static renderers =>
                        {
                            foreach (var renderer in renderers)
                                renderer.MeshRenderer.material = renderer.OriginalMaterial;
                        });
                }
            }
        }

        public void Dispose()
        {
        }
    }
}