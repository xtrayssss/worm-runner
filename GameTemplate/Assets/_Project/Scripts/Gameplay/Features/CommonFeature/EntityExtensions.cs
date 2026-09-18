using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature
{
    public static class EntityExtensions
    {
        public static Vector3 GetEntityPosition(this Entity entity) =>
            entity.GetComponent<EntityViewLink>().View.transform.position;
    }
}