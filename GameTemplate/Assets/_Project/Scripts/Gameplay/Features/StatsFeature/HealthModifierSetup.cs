using System;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine.Scripting.APIUpdating;

namespace _Project.Scripts.Gameplay.Features.StatsFeature
{
    [Serializable]
    [MovedFrom("Project.Scripts.Gameplay.Features.EffectFeature")]
    public sealed record HealthModifierSetup : StatModifierSetup
    {
        public HealthModifierSetup()
        {
            TargetStatId = StatId.HEALTH;
        }

#if UNITY_EDITOR
        [OnInspectorInit]
        public void OnInspectorInit() =>
            TargetStatId = StatId.HEALTH;
#endif

        public override void Compose(Entity entity) =>
            entity.AddComponent<HealthModifierTag>();
    }
}