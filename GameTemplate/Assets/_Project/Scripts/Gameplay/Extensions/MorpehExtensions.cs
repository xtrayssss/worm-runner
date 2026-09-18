using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Extensions
{
    public static class MorpehExtensions
    {
        public static FeatureTree CreateFeatureTree(this World world) =>
            new FeatureTree(world.CreateSystemsGroup());

        public static IEnumerable<Entity> ClearWorld(this World source)
        {
            foreach (Archetype archetype in source.archetypes.archetypes)
            {
                if (archetype != null)
                {
                    foreach (Entity entity in archetype.entities.data)
                    {
                        if (!entity.IsNullOrDisposed())
                        {
                            yield return entity;

                            source.RemoveEntity(entity);
                        }
                    }
                }
            }
        }

        public static IEnumerable<Entity> ClearWorld<TWithout>(this World source) where TWithout : struct, IComponent
        {
            foreach (Archetype archetype in source.archetypes.archetypes)
            {
                if (archetype != null)
                {
                    foreach (Entity entity in archetype.entities.data)
                    {
                        if (!entity.IsNullOrDisposed() && !entity.Has<TWithout>())
                        {
                            yield return entity;

                            source.RemoveEntity(entity);
                        }
                    }
                }
            }
        }

        public static void RemoveAllSystemsGroups(this World source)
        {
            SystemsGroup[] systemsGroups = source.systemsGroups.Values.ToArray();

            foreach (SystemsGroup systemsGroup in systemsGroups)
                source.RemoveSystemsGroup(systemsGroup);
        }
    }
}