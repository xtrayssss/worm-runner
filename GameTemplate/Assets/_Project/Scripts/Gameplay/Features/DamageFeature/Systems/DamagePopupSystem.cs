using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.DamageFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DamagePopupSystem : ISystem
    {
        public World World { get; set; }

        private readonly ConfigsService _configs;

        private Event<DamagedEvent> _damagedEvents;

        public DamagePopupSystem(ConfigsService configs) =>
            _configs = configs;

        public void OnAwake() =>
            _damagedEvents = World.GetEvent<DamagedEvent>();

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvents.publishedChanges)
            {
                Entity damageable = damagedEvent.Damageable;

                if (damageable.IsNullOrDisposed() ||
                    !damageable.Has<ShowDamagePopupMarker>())
                    continue;

                Vector3 position = damagedEvent.Modifier.EffectPoint;

                DamagePopupView popupPrefab = _configs.GetDamagePopupPrefab();

                position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

                DamagePopupView popupView = Object.Instantiate(popupPrefab, position, popupPrefab.transform.rotation);
                popupView.Construct();
                popupView.Text.text = damagedEvent.DamageAmount.ToString("F0");

                popupView.Show(duration: 0.7f);
            }
        }

        public void Dispose()
        {
        }
    }
}