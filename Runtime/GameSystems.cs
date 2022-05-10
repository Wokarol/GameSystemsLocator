using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wokarol.GameSystemsLocator
{
    public class GameSystems
    {
        private static readonly Dictionary<Type, SystemBinding> systems = new();

        public static IEnumerable<(Type Type, SystemBinding Binding)> Systems => systems.Select(kv => (kv.Key, kv.Value));

        public static void Clear()
        {
            systems.Clear();
        }

        public static T Get<T>() where T : class
        {
            if (!systems.TryGetValue(typeof(T), out var boundSystem))
                throw new InvalidOperationException("The type was not registered as the game system");

            var instance = (T)boundSystem.Instance;
            if (instance != null)
                return instance;
            else
                return (T)boundSystem.NullInstance;
        }

        public static void Initialize(GameObject systemsObject)
        {
            foreach (var system in Systems)
            {
                var s = systemsObject.GetComponentInChildren(system.Type, true);
                system.Binding.Instance = s;
            }
        }

        public static void Initialize(GameObject systemsObject, Action<ConfigurationBuilder> configCallback)
        {
            configCallback(new ConfigurationBuilder());
            Initialize(systemsObject);
        }

        public class ConfigurationBuilder
        {
            public string PrefabPath = "";

            public void AddSingleton<T>(T nullObject = null) where T : class
            {
                if (systems.TryGetValue(typeof(T), out var _))
                    throw new InvalidOperationException("The singleton type can only be registered once");

                var boundSystem = new SystemBinding()
                {
                    NullInstance = nullObject
                };
                systems.Add(typeof(T), boundSystem);
            }
        }
    }

    public class SystemBinding
    {
        public object Instance { get; internal set; }
        public object NullInstance { get; internal set; }
    }
}
