using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Configs
{
    [CreateAssetMenu(fileName = "LevelsDatabase",
        menuName = ProjectConfig.PROJECT_NAME + "/Configs/Level/Levels Database")]
    public class LevelsDatabase : SerializedScriptableObject
    {
        [SerializeField]
        [ListDrawerSettings(
            DraggableItems = true,
            HideAddButton = false,
            HideRemoveButton = false,
            NumberOfItemsPerPage = 20
        )]
        private List<ChapterConfig> _chapters;

        public IReadOnlyList<ChapterConfig> Chapters => _chapters;

        public ChapterConfig GetChapter(int index)
        {
            if (index < 0 || index >= _chapters.Count)
                return null;

            return _chapters[index];
        }

        public LevelConfig GetLevel(int globalIndex)
        {
            int i = 0;

            foreach (ChapterConfig chapter in _chapters)
            {
                foreach (LevelConfig level in chapter.Levels)
                {
                    if (i == globalIndex)
                        return level;

                    i++;
                }
            }

            return null;
        }

        public ChapterConfig GetChapterForLevel(int globalIndex, out int localIndex)
        {
            int i = 0;

            foreach (ChapterConfig ch in _chapters)
            {
                for (int l = 0; l < ch.LevelsCount; l++, i++)
                {
                    if (i == globalIndex)
                    {
                        localIndex = l;
                        return ch;
                    }
                }
            }

            localIndex = -1;
            return null;
        }

        public int GetChapterIndex(int globalLevelIndex)
        {
            int i = 0;
            int chapterIdx = 0;
            
            foreach (ChapterConfig ch in _chapters)
            {
                for (int l = 0; l < ch.LevelsCount; l++, i++)
                {
                    if (i == globalLevelIndex)
                        return chapterIdx;
                }

                chapterIdx++;
            }

            return -1;
        }

        public bool IsLastLevelInChapter(int globalIndex)
        {
            int i = 0;

            foreach (var ch in _chapters)
            {
                int end = i + ch.LevelsCount;

                if (globalIndex == end - 1)
                    return true;

                i = end;
            }

            return false;
        }

        public int GetLevelsCount() =>
            _chapters.Sum(static ch => ch.LevelsCount);
    }
}