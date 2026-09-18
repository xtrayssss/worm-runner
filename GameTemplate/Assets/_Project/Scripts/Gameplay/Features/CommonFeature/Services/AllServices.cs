using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Services
{
    public class AllServices
    {
        [ShowInInspector]
        [Searchable]
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        private static AllServices _instance;

        public static AllServices Instance => _instance ??= new AllServices();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            _instance?.Clear();
            _instance = null;
        }

        public TService Register<TService>(TService service) where TService : IService
        {
            Type type = typeof(TService);
            _services.Add(type, service);
            return service;
        }

        public void Unregister<TService>() where TService : IService
        {
            Type type = typeof(TService);
            _services.Remove(type);
        }

        public T Get<T>(bool ignoreException = false) where T : class, IService
        {
            Type type = typeof(T);

            if (!_services.TryGetValue(type, out IService service))
            {
                if (ignoreException)
                    return null;

#if DEBUG
                Debug.LogError($"Service of type {type} is not registered!");
#endif

                return null;
            }

            return service as T;
        }

        public Dictionary<Type, IService> GetAllServices() => 
            _services;

        public void Clear() =>
            _services.Clear();
    }
}