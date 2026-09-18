using System;
using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature
{
    [Serializable]
    public class TutorialTemplate
    {
        public string Id;
        public List<TutorialStepTemplate> Steps = new List<TutorialStepTemplate>();
    }
}