using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.GameFeature.Configs;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class BubbleView : InteractiveObjectView
    {
        [SerializeField]
        private SpriteRenderer _bubbleSprite;

        [SerializeField]
        private AudioSource _popSound;

        [SerializeField]
        private TMP_Text _effectValueLabel;

        [SerializeField]
        private SpriteRenderer _effectIcon;

        private Tween _floatingAnimation;

        public void Construct(
            ConfigsService configsService,
            EffectType effectType,
            float effectValue,
            StatId targetStat)
        {
            effectValue = Mathf.Abs(effectValue);

            _effectValueLabel.text = effectValue.ToString("F0");

            if (effectType == EffectType.STAT_MODIFICATION)
                _effectValueLabel.text += "%";

            GameConfig gameConfig = configsService.GetGameConfig();

            _effectIcon.sprite = gameConfig.EffectIcons.GetIcon(effectType, targetStat);
        }

        public Tween PlayFloatingAnimation()
        {
            const float FLOAT_DURATION = 1.5f;
            const float FLOAT_AMOUNT = 0.7f;

            _floatingAnimation = Tween.LocalPositionY(
                VisualRoot,
                VisualRoot.localPosition.y + FLOAT_AMOUNT,
                FLOAT_DURATION,
                ease: Ease.InOutSine,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );

            return _floatingAnimation;
        }

        public async UniTask PlayJumpToCrowdAsync(Vector3 targetPosition)
        {
            const float JUMP_DURATION = 0.4f;
            const float JUMP_HEIGHT = 4f;

            _floatingAnimation.Stop();

            await TweenExtensions
                .Jump(
                    transform,
                    targetPosition,
                    JUMP_DURATION,
                    JUMP_HEIGHT,
                    horizontalEase: Ease.InSine)
                .ToYieldInstruction();
        }

#if UNITY_EDITOR
        private Vector3 _originalPosition;

        [Button]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        private void TestJump(Vector3 targetPosition)
        {
            transform.position = _originalPosition;
            targetPosition = new Vector3(0, 0, 10f);
            PlayJumpToCrowdAsync(targetPosition).Forget();
        }

        private void Awake()
        {
            _originalPosition = transform.position;
        }
#endif
    }
}