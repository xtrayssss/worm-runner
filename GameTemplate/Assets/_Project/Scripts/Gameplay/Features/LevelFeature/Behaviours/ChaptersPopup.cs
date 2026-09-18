using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours
{
    public sealed class ChaptersPopup : ModularWindow
    {
        [SerializeField]
        private ChapterRow[] _rows;

        [SerializeField]
        private Button _closeButton;

        private Action _onClosed;
        private LevelsDatabase _levelsDatabase;
        private LevelService _levelService;

        public void Construct(LevelsDatabase levelsDatabase, LevelService levelService, Action onClosed)
        {
            _onClosed = onClosed;
            _levelsDatabase = levelsDatabase;
            _levelService = levelService;

            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnDestroy() =>
            _closeButton.onClick.RemoveListener(OnCloseClicked);

        public override async UniTask ShowAsync()
        {
            base.ShowAsync();

            int currentChapterIndex = _levelsDatabase.GetChapterIndex(_levelService.CurrentLevelIndex);

            for (int i = 0; i < _rows.Length; i++)
            {
                bool isUnlocked = i < currentChapterIndex;
                _rows[i].SetUnlocked(isUnlocked);
            }

            await WindowTweener
                .ShowGrowBounceWindow(
                    ContentRectTransform,
                    canvasGroup: SelfCanvasGroup,
                    useUnscaledTime: true)
                .OnComplete(this, static p => p.EnableInteraction())
                .ToUniTask();

            await _rows[currentChapterIndex].PlayUnlockAnimationAsync();
        }

        public override async UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            CancellationToken linkedToken = CreateLinkedCancellationToken(externCancellationToken);
            base.CloseAsync(linkedToken);
            await WindowTweener
                .HideGrowBounceWindow(ContentRectTransform, canvasGroup: SelfCanvasGroup, useUnscaledTime: true)
                .AsTask(linkedToken);
        }

        private void OnCloseClicked() =>
            _onClosed?.Invoke();
    }
}