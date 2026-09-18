using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.StatusFeature
{
    public sealed class StatusEnhancements
    {
        private Entity _status;

        public static StatusEnhancements Create() =>
            new StatusEnhancements();

        public StatusEnhancements ForStatus(Entity status)
        {
            _status = status;
            return this;
        }
    }
}