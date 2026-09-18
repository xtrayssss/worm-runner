using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Gameplay
{
    public class SceneLoader : IService
    {
        public async UniTask Load(string scene)
        {
            AsyncOperation handler = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

            await handler;
        }
    }
}