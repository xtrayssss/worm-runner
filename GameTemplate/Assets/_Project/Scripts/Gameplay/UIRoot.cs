using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class UIRoot : MonoBehaviour, IService
    {
        public GameWindow GameWindow { get; private set; }
        public void Construct(GameWindow gameWindow)
        {
            GameWindow = gameWindow;
        }
    }
}