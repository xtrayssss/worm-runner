using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    public struct PartRenderers : IComponent
    {
        public Renderer[] Value;

        public struct Renderer
        {
            public MeshRenderer MeshRenderer;
            public Material OriginalMaterial;
        }
    }
}