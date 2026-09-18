using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay
{
    public sealed class ButtonSound : MonoBehaviour
    {
        private Button _button;

        [SerializeField]
        [ValueDropdown("@AudioService.AudioLibrary.AudioFile.GetAudioIdDropdown()",
            NumberOfItemsBeforeEnablingSearch = 1)]
        private string _customSoundId;

        [SerializeField]
        [HideIf("@!string.IsNullOrEmpty(_customSoundId)")]
        private AudioClip _customSoundClip;

        private void Awake() =>
            _button = GetComponent<Button>();

        private void OnEnable() =>
            _button.onClick.AddListener(PlayButtonClickSound);

        private void OnDisable() =>
            _button.onClick.RemoveListener(PlayButtonClickSound);

        private void PlayButtonClickSound()
        {
            AudioService audioService = AllServices.Instance.Get<AudioService>();

            if (!string.IsNullOrEmpty(_customSoundId))
                audioService.PlaySound(_customSoundId);
            else if (_customSoundClip != null)
                audioService.PlaySound(_customSoundClip);
            else
                audioService.PlaySound(AudioId.Sfx.UI.BUTTON_CLICK);
        }
    }
}