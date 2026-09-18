using System;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine.Scripting.APIUpdating;

namespace _Project.Scripts.Gameplay.Features.StatsFeature
{
    [Serializable]
    [MovedFrom("Project.Scripts.Gameplay.Features.EffectFeature")]
    public sealed record SpeedModifierSetup : StatModifierSetup
    {
        [OnInspectorInit]
        public void OnInspectorInit() =>
            TargetStatId = StatId.SPEED;

        public override void Compose(Entity entity) =>
            entity.AddComponent<SpeedModifierTag>();
    }
}