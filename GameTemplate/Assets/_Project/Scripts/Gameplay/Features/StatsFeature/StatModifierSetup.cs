using System;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.StatsFeature
{
    [Serializable]
    public abstract record StatModifierSetup
    {
        [SerializeField, FormerlySerializedAs("<Value>k__BackingField")]
        [FoldoutGroup("$GetInspectorDisplayName"), ShowIf("@SourceStatId == StatId.UNKNOWN")]
        public float ModifierValue;

        [SerializeField, FormerlySerializedAs("<SourceStatId>k__BackingField")]
        [FoldoutGroup("$GetInspectorDisplayName")]
        public StatId SourceStatId = StatId.UNKNOWN;

        [SerializeField, FormerlySerializedAs("<TargetStatId>k__BackingField")]
        [FoldoutGroup("$GetInspectorDisplayName")]
        public StatId TargetStatId;

        [SerializeField, FormerlySerializedAs("<Priority>k__BackingField")]
        [FoldoutGroup("$GetInspectorDisplayName")]
        public int Priority;

        [SerializeField, FormerlySerializedAs("<Operation>k__BackingField")]
        [FoldoutGroup("$GetInspectorDisplayName"), OnValueChanged("SyncPriorityWithOperation")]
        public StatOperation Operation;

        [SerializeField, FormerlySerializedAs("<IsBaseValue>k__BackingField")]
        [FoldoutGroup("$GetInspectorDisplayName")]
        public bool AffectsBaseValue;

        [SerializeField, FormerlySerializedAs("<EffectType>k__BackingField")]
        [FoldoutGroup("$GetInspectorDisplayName")]
        public StatEffectType EffectType = StatEffectType.INSTANT;

        public virtual void Compose(Entity entity)
        {
        }

#if UNITY_EDITOR
        public void SyncPriorityWithOperation() =>
            Priority = (int)Operation;

        public string GetInspectorDisplayName() =>
            ObjectNames
                .NicifyVariableName(GetType().Name);
#endif
    }
}