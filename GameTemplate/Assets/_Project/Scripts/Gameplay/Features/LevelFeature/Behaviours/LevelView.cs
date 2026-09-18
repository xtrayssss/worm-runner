using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours
{
    public sealed class LevelView : MonoBehaviour
    {
        [SerializeField]
        private Material _roadMaterial;

        [SerializeField]
        private SpriteRenderer _groundRenderer;

        private UIRoot _uiRoot;

        private static readonly int COLOR_ID = Shader.PropertyToID("_BaseColor");

        public void Construct(UIRoot uiRoot) => 
            _uiRoot = uiRoot;

        public void ApplyBiome(ChapterConfig.BiomeData biome)
        {
            _uiRoot.GameWindow.Fog.color = biome.FogColor;
            
            _roadMaterial.SetColor(COLOR_ID, biome.RoadColor);
            
            _groundRenderer.sprite = biome.GroundSprite;
            
            ParticleSystem.ColorOverLifetimeModule col = _uiRoot.GameWindow.FogParticle.colorOverLifetime;
            Gradient gradient = col.color.gradient;
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
            GradientColorKey[] colorKeys = gradient.colorKeys;

            alphaKeys[1] = new GradientAlphaKey(biome.FogParticleAlpha, alphaKeys[1].time);
            gradient.SetKeys(colorKeys, alphaKeys);
            col.color = new ParticleSystem.MinMaxGradient(gradient);
        }
    }
}