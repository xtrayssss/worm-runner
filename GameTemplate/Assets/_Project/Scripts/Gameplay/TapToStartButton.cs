using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay
{
    public sealed class TapToStartButton : MonoBehaviour
    {
        [SerializeField] private ButtonPulser _buttonPulser;
        [SerializeField] private Button _button;

        public ButtonPulser ButtonPulser => _buttonPulser;
        public Button Button => _button;

        public void Construct() =>
            _buttonPulser.Construct();
    }
}