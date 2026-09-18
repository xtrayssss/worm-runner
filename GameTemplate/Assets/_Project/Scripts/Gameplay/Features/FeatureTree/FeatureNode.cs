using System;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Gameplay.Features.FeatureTree
{
    [Serializable]
    public sealed class FeatureNode
    {
        [ShowInInspector] public IFeature Feature { get; }
        [ShowInInspector] public FeatureNode Parent { get; }
        [ShowInInspector] public bool IsPausable { get; private set; }
        [ShowInInspector] private bool HasExplicitPauseSetting { get; set; }

        public FeatureNode(IFeature feature, FeatureNode parent = null)
        {
            Feature = feature;
            Parent = parent;
            IsPausable = false;
            HasExplicitPauseSetting = false;
        }

        public FeatureNode CreateChild(IFeature feature) =>
            new FeatureNode(feature, this);

        public void SetPausable(bool isPausable)
        {
            IsPausable = isPausable;
            HasExplicitPauseSetting = true;
        }

        public bool IsFeaturePausable()
        {
            if (HasExplicitPauseSetting)
                return IsPausable;

            if (!IsPausable)
                return false;

            return Parent == null || Parent.IsFeaturePausable();
        }
    }
}