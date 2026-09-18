using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public class TutorialDynamicElement : MonoBehaviour
    {
        [SerializeField]
        private string _elementKey;

        private RectTransform _cachedRectTransform;
        private bool _isRegistered;

        public string ElementKey
        {
            get => _elementKey;
            set
            {
                _elementKey = value;

                if (!string.IsNullOrEmpty(_elementKey) && gameObject.activeInHierarchy)
                    Register();
            }
        }

        protected virtual void Awake() =>
            _cachedRectTransform = transform as RectTransform;

        protected virtual void OnEnable()
        {
            if (!string.IsNullOrEmpty(_elementKey))
                Register();
        }

        protected virtual void OnDisable() =>
            Unregister();

        protected void Register()
        {
            if (string.IsNullOrEmpty(_elementKey))
                return;

            TutorialRegistry.Register(_elementKey, _cachedRectTransform);
            _isRegistered = true;

#if DEBUG
            Debug.Log($"[Tutorial] Registered element: {_elementKey} on {gameObject.name}");
#endif
        }

        protected void Unregister()
        {
            if (!_isRegistered || string.IsNullOrEmpty(_elementKey))
                return;

            TutorialRegistry.Unregister(_elementKey);
            _isRegistered = false;

#if DEBUG
            Debug.Log($"[Tutorial] Unregistered element: {_elementKey} from {gameObject.name}");
#endif
        }
    }
}