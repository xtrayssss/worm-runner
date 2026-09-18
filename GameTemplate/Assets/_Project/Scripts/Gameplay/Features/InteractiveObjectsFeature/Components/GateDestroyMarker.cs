using System;
using Unity.IL2CPP.CompilerServices;
using IComponent = Scellecs.Morpeh.IComponent;
using StatId = _Project.Scripts.Gameplay.Features.StatsFeature.StatId;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct GateDestroyMarker : IComponent
    {
    }
}