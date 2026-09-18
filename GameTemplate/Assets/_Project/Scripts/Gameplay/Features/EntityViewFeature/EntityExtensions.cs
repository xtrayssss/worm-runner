using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.EntityViewFeature
{
    public static class EntityExtensions
    {
#if DEBUG
        public static string GetNiceName(this Entity entity) =>
            entity.Id + ": " + entity.GetComponent<EntityName>().Value;
#endif
    }
}