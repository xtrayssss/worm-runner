using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Extensions
{
    public static class FilterExtensions
    {
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public static IEnumerable<Entity> Where(this Filter filter, Func<Entity, bool> predicate)
        {
            foreach (var entity in filter)
                if (predicate(entity))
                    yield return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Entity> ToList(this Filter filter, int capacity = 0)
        {
            List<Entity> list = new List<Entity>(capacity);

            foreach (Entity entity in filter)
                list.Add(entity);

#if MORPEH_DEBUG
            enumerator.Dispose();
#endif

            return list;
        }
    }
}