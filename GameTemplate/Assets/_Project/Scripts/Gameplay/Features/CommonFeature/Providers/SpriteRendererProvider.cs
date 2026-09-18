using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using Scellecs.Morpeh.Providers;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Providers
{
    public sealed class SpriteRendererProvider : MonoProvider<SpriteRendererLink>
    {
        protected override void Initialize()
        {
            ref SpriteRendererLink spriteRenderer = ref GetData();
            spriteRenderer.Value = GetComponentInChildren<SpriteRenderer>();
        }
    }
}