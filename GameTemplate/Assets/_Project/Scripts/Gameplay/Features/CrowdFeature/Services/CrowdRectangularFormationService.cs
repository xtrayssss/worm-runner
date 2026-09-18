using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Services
{
    public sealed class CrowdTriangleFormationService : IService
    {
        public Vector3 GetTrianglePosition(int memberIndex, int totalMembers)
        {
            if (totalMembers <= 1)
                return Vector3.zero;

            const float BASE_Z_OFFSET = 0.8f;
            const float SPACING = 1.5f;
            
            // Ряд 1: 3 члена (центр, лево, право)
            if (memberIndex < 3)
            {
                return GetRow1Position(memberIndex, SPACING);
            }
            
            // Ряд 2: 4 члена (смещенные позиции между членами ряда 1)
            if (memberIndex < 7) // 3 + 4 = 7
            {
                return GetRow2Position(memberIndex - 3, SPACING, BASE_Z_OFFSET);
            }
            
            // Ряд 3: 3 члена (дублирует X-позиции ряда 1)
            if (memberIndex < 10) // 7 + 3 = 10
            {
                return GetRow3Position(memberIndex - 7, SPACING, BASE_Z_OFFSET * 2);
            }
            
            // Остальные члены заполняют промежутки
            return GetFillerPosition(memberIndex - 10, totalMembers - 10, SPACING, BASE_Z_OFFSET);
        }
        
        private Vector3 GetRow1Position(int indexInRow, float spacing)
        {
            // Ряд 1: центр, лево, право
            float[] xPositions = { 0f, -spacing, spacing };
            return new Vector3(xPositions[indexInRow], 0, 0);
        }
        
        private Vector3 GetRow2Position(int indexInRow, float spacing, float zOffset)
        {
            // Ряд 2: 4 члена между позициями ряда 1
            float[] xPositions = { 
                -spacing * 1.5f,    // Слева от левого
                -spacing * 0.5f,    // Между левым и центром
                spacing * 0.5f,     // Между центром и правым
                spacing * 1.5f      // Справа от правого
            };
            
            return new Vector3(xPositions[indexInRow], 0, zOffset);
        }
        
        private Vector3 GetRow3Position(int indexInRow, float spacing, float zOffset)
        {
            // Ряд 3: дублирует X-позиции ряда 1
            return GetRow1Position(indexInRow, spacing) + new Vector3(0, 0, zOffset);
        }
        
        private Vector3 GetFillerPosition(int fillerIndex, int totalFillers, float spacing, float baseZOffset)
        {
            if (totalFillers <= 0)
                return Vector3.zero;
                
            // Создаем дополнительные ряды для заполнения
            const int FILLER_MEMBERS_PER_ROW = 5; // Промежуточные ряды шире
            
            int rowIndex = fillerIndex / FILLER_MEMBERS_PER_ROW;
            int positionInRow = fillerIndex % FILLER_MEMBERS_PER_ROW;
            
            // Z-позиция: размещаем между основными рядами и после них
            float zPosition;
            if (rowIndex % 2 == 0)
            {
                // Четные индексы рядов: между основными рядами
                zPosition = baseZOffset * 0.5f + rowIndex / 2f * baseZOffset;
            }
            else
            {
                // Нечетные индексы рядов: между ряд 2 и ряд 3, затем после ряд 3
                zPosition = baseZOffset * 1.5f + (rowIndex - 1) / 2f * baseZOffset;
            }
            
            // X-позиция: равномерно распределяем по ширине формации
            float maxWidth = spacing * 2f; // Ширина основана на самом широком ряду
            float xStep = maxWidth / (FILLER_MEMBERS_PER_ROW - 1);
            float xPosition = -maxWidth * 0.5f + positionInRow * xStep;
            
            // Добавляем небольшое случайное смещение для более естественного вида
            float randomOffset = GetConsistentRandom(fillerIndex) * spacing * 0.2f;
            xPosition += randomOffset;
            
            return new Vector3(xPosition, 0, zPosition);
        }
        
        private float GetConsistentRandom(int seed)
        {
            // Простая функция для получения консистентного "случайного" значения
            // на основе индекса (для воспроизводимости)
            return Mathf.Sin(seed * 12.9898f + seed * 78.233f) * 2f - 1f;
        }
        
        // Вспомогательный метод для получения общего количества членов в основных рядах
        public int GetCoreFormationSize()
        {
            return 10; // 3 + 4 + 3
        }
        
        // Метод для получения рекомендуемого максимального размера формации
        public int GetRecommendedMaxSize()
        {
            return 25; // Основа + разумное количество заполнителей
        }
    }
}