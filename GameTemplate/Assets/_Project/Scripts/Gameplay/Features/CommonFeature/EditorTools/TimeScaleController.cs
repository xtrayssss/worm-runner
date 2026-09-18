using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.EditorTools
{
    public sealed class TimeScaleController : MonoBehaviour
    {
        [SerializeField]
        private float _normalSpeed = 1f;

        [SerializeField]
        private float _fastSpeed = 2f;

        [SerializeField]
        private float _veryFastSpeed = 5f;

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                Time.timeScale = _normalSpeed;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                Time.timeScale = _fastSpeed;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                Time.timeScale = _veryFastSpeed;
        }
    }
}