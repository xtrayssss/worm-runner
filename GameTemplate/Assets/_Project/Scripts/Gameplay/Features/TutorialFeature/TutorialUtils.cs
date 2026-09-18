using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    public static class TutorialUtils
    {
        public static RectTransform[] ConvertReferencesToTransforms(List<TutorialStepTemplate.ElementReference> references)
        {
            return references
                .Where(r => !string.IsNullOrEmpty(r.ElementKey))
                .Select(r => TutorialRegistry.GetElement(r.ElementKey))
                .Where(t => t != null)
                .ToArray();
        }

        public static RectTransform GetReferenceTransform(TutorialStepTemplate.ElementReference reference)
        {
            if (string.IsNullOrEmpty(reference.ElementKey))
                return null;
            
            return TutorialRegistry.GetElement(reference.ElementKey);
        }
    }
}