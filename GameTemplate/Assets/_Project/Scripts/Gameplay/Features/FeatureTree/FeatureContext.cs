using System;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.FeatureTree
{
    public sealed class FeatureContext
    {
        private readonly FeatureNode _featureNode;
        private bool _isCompleted;
        public FeatureTree Tree { get; }

        public FeatureContext(FeatureTree tree, FeatureNode featureNode)
        {
            Tree = tree;
            _featureNode = featureNode;
        }

        public FeatureContext AddSystem(ISystem system, SystemPauseMode pauseMode = SystemPauseMode.DEFAULT)
        {
            EnsureNotCompleted();

            Tree
                .AddSystem(system, _featureNode)
                .SetSystemPauseMode(system, pauseMode);

            return this;
        }

        public FeatureContext AddFeature(IFeature feature)
        {
            EnsureNotCompleted();

            FeatureNode childNode = _featureNode.CreateChild(feature);
            FeatureContext nestedContext = new FeatureContext(Tree, childNode);
            feature.Configure(nestedContext);
            nestedContext.Complete();

            return this;
        }

        public FeatureContext SetPausable(bool isPausable = true)
        {
            _featureNode.SetPausable(isPausable);
            return this;
        }

        public void Complete() =>
            _isCompleted = true;

        private void EnsureNotCompleted()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Cannot add systems after context is completed");
        }
    }
}