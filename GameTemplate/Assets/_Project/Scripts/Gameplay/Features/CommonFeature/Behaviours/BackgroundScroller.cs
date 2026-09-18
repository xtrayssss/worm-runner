using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    public sealed class BackgroundScroller : MonoBehaviour
    {
        [SerializeField] private Vector2 _scrollSpeed = new Vector2(0.1f, 0);
        private Graphic _graphic;
        private Material _material;

        private void Start()
        {
            _graphic = GetComponent<Graphic>();
            _material = new Material(_graphic.material);
            _graphic.material = _material;
        }

        private void Update()
        {
            Vector2 offset = Time.time * _scrollSpeed;
            _material.mainTextureOffset = offset;
        }
    }
}