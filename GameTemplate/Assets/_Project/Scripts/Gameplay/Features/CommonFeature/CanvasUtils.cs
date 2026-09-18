using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature
{
    public static class CanvasUtils
    {
        public static (float left, float right, float top, float bottom) CalculateCanvasBoundaries(
            RectTransform canvasRect)
        {
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);
            return (canvasCorners[0].x, canvasCorners[2].x, canvasCorners[2].y, canvasCorners[0].y);
        }
    }
}