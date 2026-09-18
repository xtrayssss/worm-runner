using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using Scellecs.Morpeh.Providers;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Providers
{
    public sealed class EntityViewProvider : MonoProvider<EntityViewLink>
    {
        protected override void Initialize()
        {
            ref EntityViewLink entityView = ref GetData();
            entityView.View = GetComponent<EntityView>(); 
            entityView.View.Entity = Entity;
        }

        protected override void OnDisable()
        {
        }
    }
}