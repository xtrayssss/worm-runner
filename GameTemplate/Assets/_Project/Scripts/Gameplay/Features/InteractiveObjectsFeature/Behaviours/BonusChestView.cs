using System;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class BonusChestView : InteractiveObjectView
    {
        [Header("Bonus Chest Specific")]
        [SerializeField]
        private SpriteRenderer _chestRenderer;

        [SerializeField]
        private Sprite _openedChestIcon;

        [SerializeField]
        private AudioSource _openSound;

        [Header("Open Animation")]
        [SerializeField]
        private AnimationCurve _openSquashXCurve;

        [SerializeField]
        private AnimationCurve _openSquashYCurve;

        [SerializeField]
        private VFXData _moneyExplosionVFX;

        private Sequence _shakeAnimation;
        private VFXService _vfxService;
        public event Action OnChestOpened;

        private Sprite _closedChestIcon;
        private Vector3 _originalRotation;
        private Vector3 _originalPosition;
        private Vector3 _visualRootOriginalScale;
        private AudioService _audioService;

        public void Construct(VFXService vfxService, AudioService audioService)
        {
            _vfxService = vfxService;
            _audioService = audioService;
            _closedChestIcon = _chestRenderer.sprite;
            _originalRotation = VisualRoot.localEulerAngles;
            _originalPosition = VisualRoot.localPosition;
            _visualRootOriginalScale = VisualRoot.localScale;
        }

        [Button]
        public async UniTask OpenChest()
        {
            _shakeAnimation.Stop();
            VisualRoot.localEulerAngles = Vector3.zero;
            VisualRoot.localScale = _visualRootOriginalScale;
            VisualRoot.rotation = Quaternion.Euler(_originalRotation);
            _chestRenderer.sprite = _closedChestIcon;
            
            _audioService.PlaySound(AudioId.Sfx.Gameplay.CHEST_OPEN);

            await StartOpenAnimation();
        }

        [Button]
        public void StartShake()
        {
            const float DURATION = 0.625f;

            _shakeAnimation = Sequence.Create(cycles: -1)
                .Group(
                    Tween.PunchCustom(
                        VisualRoot,
                        startValue: Vector3.zero,
                        new ShakeSettings
                        {
                            strength = new Vector3(0, 0, 7f),
                            frequency = 1,
                            easeBetweenShakes = Ease.OutElastic,
                            duration = DURATION,
                            asymmetry = 0.5f
                        },
                        onValueChange: static (chest, rotation) =>
                        {
                            chest.localEulerAngles = new Vector3(rotation.x, rotation.y, rotation.z);
                        }))
                .Group(
                    Tween.PunchLocalPosition(
                        VisualRoot,
                        Vector3.up * 0.7f,
                        DURATION,
                        frequency: 1,
                        easeBetweenShakes: Ease.OutElastic))
                .Group(
                    Tween.PunchScale(
                        VisualRoot,
                        new Vector3(-0.1f, 0.2f, 0),
                        DURATION,
                        frequency: 1,
                        easeBetweenShakes: Ease.OutElastic));
        }

        private async UniTask StartOpenAnimation()
        {
            if (_shakeAnimation.isAlive)
                _shakeAnimation.Stop();

            VisualRoot.localPosition = _originalPosition;

            const float DURATION = 1.25f;

            Vector3 visualRootScale = VisualRoot.localScale;
            Vector3 step1 = new Vector3(visualRootScale.x * 0.75f, visualRootScale.y * 1.3f, visualRootScale.z);
            Vector3 step2 = new Vector3(visualRootScale.x * 1.3f, visualRootScale.y * 0.65f, visualRootScale.z);

            Sequence squash = Sequence
                .Create()
                .Chain(Tween.Scale(
                    VisualRoot,
                    step1,
                    DURATION * 0.162f,
                    ease: Ease.OutQuad))
                .Chain(Tween.Scale(
                    VisualRoot,
                    step2,
                    DURATION * 0.162f,
                    ease: Ease.OutQuad))
                .Chain(Tween.Scale(
                    VisualRoot,
                    _visualRootOriginalScale,
                    DURATION * 0.2f,
                    ease: Easing.Elastic(strength: 3f, period: .5f)));

            Sequence sequence = Sequence.Create()
                .Group(Tween.PunchLocalPosition(
                    VisualRoot,
                    Vector3.up * 0.8f,
                    0.3f,
                    frequency: 3,
                    enableFalloff: false))
                .Group(squash)
                .InsertCallback((DURATION * 0.2f + DURATION * 0.3f) * 0.64f, this, static chest =>
                {
                    chest._vfxService.PlayVFX(chest._moneyExplosionVFX, chest.transform.position, chest.transform);
                    chest._chestRenderer.sprite = chest._openedChestIcon;
                    chest.OnChestOpened?.Invoke();
                });

            await sequence.AsTask(destroyCancellationToken);
        }

#if UNITY_EDITOR
        [ContextMenu("Setup Default Curves")]
        private void SetupDefaultCurves()
        {
            _openSquashXCurve = new AnimationCurve();
            _openSquashXCurve.AddKey(new Keyframe(0f, 1f));
            _openSquashXCurve.AddKey(new Keyframe(0.3f, 1.2f));
            _openSquashXCurve.AddKey(new Keyframe(0.65f, 0.8f));
            _openSquashXCurve.AddKey(new Keyframe(1f, 1f));

            _openSquashYCurve = new AnimationCurve();
            _openSquashYCurve.AddKey(new Keyframe(0f, 1f));
            _openSquashYCurve.AddKey(new Keyframe(0.3f, 0.8f));
            _openSquashYCurve.AddKey(new Keyframe(0.65f, 1.2f));
            _openSquashYCurve.AddKey(new Keyframe(1f, 1f));

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}