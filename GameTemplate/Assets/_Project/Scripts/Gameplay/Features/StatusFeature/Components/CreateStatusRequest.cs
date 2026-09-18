using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct CreateStatusRequest : IComponent, IRequestData
    {
        public StatusSetup Setup;
        public Entity Producer;
        public Entity Target;
        public Entity EnhancementSource;
    }
}