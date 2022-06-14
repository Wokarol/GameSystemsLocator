using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Wokarol.GameSystemsLocator.Tests")]
namespace Wokarol.GameSystemsLocator
{
    public class GameSystems
    {
        private static readonly Dictionary<Type, SystemBinding> systems = new();

        public static IEnumerable<(Type Type, SystemBinding Binding)> Systems => systems.Select(kv => (kv.Key, kv.Value));

        public static T Get<T>() where T : class
        {
            if (!systems.TryGetValue(typeof(T), out var boundSystem))
                throw new InvalidOperationException("The type was not registered as the game system");

            var instance = (T)boundSystem.Instance;
            var nullInstance = (T)boundSystem.NullInstance;

            if (instance != null)
            {
                return instance;
            }
            else if (nullInstance != null)
            {
                return nullInstance;
            }
            else
            {
                if (boundSystem.Required)
                {
                    Debug.LogWarning($"Tried to get a required system {typeof(T)} but found null");
                }
                return null;
            }
        }

        internal static void Clear()
        {
            systems.Clear();
        }

        internal static void Initialize(GameObject systemsObject)
        {
            foreach (var system in Systems)
            {
                var s = systemsObject.GetComponentInChildren(system.Type, true);
                system.Binding.InstancesInternal.Add(s);

                if (system.Binding.Required && s == null)
                {
                    // Consider using exceptions
                    Debug.LogError($"The binding for {system.Type.FullName} is required");
                }
            }
        }

        internal static void Initialize(GameObject systemsObject, Action<ConfigurationBuilder> configCallback)
        {
            configCallback(new ConfigurationBuilder());
            Initialize(systemsObject);
        }

        internal static void ApplyOverride(GameObject holder)
        {
            foreach (var system in Systems)
            {
                var s = holder.GetComponentInChildren(system.Type);
                if (s == null) continue;

                system.Binding.InstancesInternal.Add(s);
            }
        }

        internal static void RemoveOverride(GameObject holder)
        {
            foreach (var system in Systems)
            {
                var s = holder.GetComponentInChildren(system.Type);
                if (s == null) continue;

                system.Binding.InstancesInternal.Remove(s);
            }
        }

        public class ConfigurationBuilder
        {
            public string PrefabPath = "";

            public void Add<T>(T nullObject = null, bool required = false) where T : class
            {
                if (systems.TryGetValue(typeof(T), out var _))
                    throw new InvalidOperationException("The singleton type can only be registered once");

                var boundSystem = new SystemBinding()
                {
                    NullInstance = nullObject,
                    Required = required,
                };
                systems.Add(typeof(T), boundSystem);
            }
        }
    }

    public class SystemBinding
    {
        internal readonly List<object> InstancesInternal = new List<object>();

        public IReadOnlyList<object> Instances => InstancesInternal;
        public object Instance => InstancesInternal[InstancesInternal.Count - 1];
        public object NullInstance { get; internal set; }
        public bool Required { get; internal set; }
    }
}
