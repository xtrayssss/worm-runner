using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.LoadingScreenFeature
{
    public sealed class LoadingScreenAnimation : MonoBehaviour
    {
        [FormerlySerializedAs("loadingText")]
        [SerializeField]
        private TextMeshProUGUI _loadingText;

        [Header("Text Animation Settings")]
        [SerializeField] private float _letterAnimDuration = 2f;

        [SerializeField]
        private float _letterDelay = 0.1f;

        [SerializeField]
        private float _minScale = 0.8f;

        [SerializeField]
        private float _maxScale = 1f;

        [SerializeField]
        private float _minAlpha;

        [SerializeField]
        private float _maxAlpha = 1f;

        private TMP_TextInfo _textInfo;
        private Vector3[] _originalScales;
        private Vector3[][] _originalVertexPositions;
        private Sequence _textAnimation;

        public bool IsInitialized { get; private set; }

        public void Construct()
        {
            _loadingText.ForceMeshUpdate();

            _textInfo = _loadingText.textInfo;

            IsInitialized = true;
        }

        public void PlayAnimation()
        {
            if (_textAnimation.isAlive)
                return;

            StopAnimation();

            _loadingText.gameObject.SetActive(true);

            //AnimateLoadingText();
        }

        public void StopAnimation()
        {
            //_textAnimation.Stop();

            //SetAllCharactersAlpha(0);
            _loadingText.gameObject.SetActive(false);
        }
    }
}