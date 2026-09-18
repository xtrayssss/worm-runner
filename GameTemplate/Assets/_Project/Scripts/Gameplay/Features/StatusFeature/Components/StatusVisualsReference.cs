using System;
using _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Behaviours;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct StatusVisualsReference : IComponent
    {
        public StatusVisuals Value;
    }
}