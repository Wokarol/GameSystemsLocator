using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Wokarol.GameSystemsLocator.Tests")]
namespace Wokarol.GameSystemsLocator
{
    /// <summary>
    /// Allows for fetching and managing systems present in the game
    /// </summary>
    public static class GameSystems
    {
        private static readonly Dictionary<Type, SystemBinding> systems = new();

        /// <summary>
        /// List of all systems in form of (Type, Binding) tuple. Gets all registered systems in the locator
        /// </summary>
        public static IEnumerable<(Type Type, SystemBinding Binding)> Systems => systems.Select(kv => (kv.Key, kv.Value));

        /// <summary>
        /// Is there any system registered for the game systems?
        /// </summary>
        public static bool IsGameSystemInitialized { get; private set; } = false;

        private static Queue<QueuedSystemOverride> systemOverridesQueue = new();

        /// <summary>
        /// Locates Game System matching the type
        /// </summary>
        /// <typeparam name="T">Type of the system to locate</typeparam>
        /// <returns>The system that was located or null if no instance is found</returns>
        /// <exception cref="InvalidOperationException">Thrown if given type T is not registered in the locator</exception>
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
            IsGameSystemInitialized = false;
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

            IsGameSystemInitialized = true;

            while (systemOverridesQueue.Count != 0)
            {
                var systemOverride = systemOverridesQueue.Dequeue();
                ApplyOverride(systemOverride.Holder, systemOverride.Overrides);
            }
        }

        internal static void Initialize(GameObject systemsObject, Action<ConfigurationBuilder> configCallback)
        {
            configCallback(new ConfigurationBuilder());
            Initialize(systemsObject);
        }

        internal static void TryApplyOverride(GameObject holder, List<GameObject> overrides = null)
        {
            if (IsGameSystemInitialized)
            {
                ApplyOverride(holder, overrides);
            }
            else
            {
                systemOverridesQueue.Enqueue(new(holder, overrides));
            }
        }

        internal static void ApplyOverride(GameObject holder, List<GameObject> overrides = null)
        {
            if (!IsGameSystemInitialized)
            {
                throw new InvalidOperationException("Applied override before the game systems config was initialized");
            }

            if (holder != null)
            {
                foreach (var system in Systems)
                {
                    var s = holder.GetComponentInChildren(system.Type);
                    if (s == null) continue;

                    system.Binding.InstancesInternal.Add(s);
                }
            }

            if (overrides != null)
            {
                foreach (var system in Systems)
                {
                    foreach (var obj in overrides)
                    {
                        if (obj.TryGetComponent(system.Type, out var s))
                        {
                            system.Binding.InstancesInternal.Add(s);
                        }
                    }
                }
            }
        }

        internal static void RemoveOverride(GameObject holder, List<GameObject> overrides = null)
        {
            if (holder != null)
            {
                foreach (var system in Systems)
                {
                    var s = holder.GetComponentInChildren(system.Type);
                    if (s == null) continue;

                    system.Binding.InstancesInternal.Remove(s);
                }
            }

            if (overrides != null)
            {
                foreach (var system in Systems)
                {
                    foreach (var obj in overrides)
                    {
                        if (obj.TryGetComponent(system.Type, out var s))
                        {
                            system.Binding.InstancesInternal.Add(s);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builder allowing for configuring the locator and the systems it should look for
        /// </summary>
        public class ConfigurationBuilder
        {
            /// <summary>
            /// Path to the prefab that should be spawned, relative to Resource folder without file suffix
            /// </summary>
            public string PrefabPath = "";

            /// <summary>
            /// Adds the type to the locator
            /// </summary>
            /// <typeparam name="T">Type to add to the locator</typeparam>
            /// <param name="nullObject">Optional null object that is used in case no instance is located</param>
            /// <param name="required">Is the system required, if so, additional error will get logged in case an instance is not present</param>
            /// <exception cref="InvalidOperationException">Thrown then the type is registered more than once</exception>
            public void Add<T>(T nullObject = null, bool required = false) where T : class
            {
                if (systems.TryGetValue(typeof(T), out var _))
                    throw new InvalidOperationException("The system type can only be registered once");

                var boundSystem = new SystemBinding()
                {
                    NullInstance = nullObject,
                    Required = required,
                };
                systems.Add(typeof(T), boundSystem);
            }
        }

        private struct QueuedSystemOverride
        {
            public GameObject Holder;
            public List<GameObject> Overrides;

            public QueuedSystemOverride(GameObject holder, List<GameObject> overrides)
            {
                Holder = holder;
                Overrides = overrides;
            }
        }
    }

    /// <summary>
    /// Describes the system and it's properties
    /// Note: The binding is not aware of type so the system instance should be obtained via the GameSystem class
    /// </summary>
    public class SystemBinding
    {
        internal readonly List<object> InstancesInternal = new List<object>();

        /// <summary>
        /// All instances registered for the system currently
        /// </summary>
        public IReadOnlyList<object> Instances => InstancesInternal;

        /// <summary>
        /// Current instance registered for the system
        /// Note: This is the last instance from Instances list, aka the newest one
        /// </summary>
        public object Instance
        {
            get
            {
                if (InstancesInternal.Count == 0) return null;
                else return InstancesInternal[InstancesInternal.Count - 1];
            }
        }

        /// <summary>
        /// Object that should be returned in case the Instances list has no elements <see cref="GameSystems.ConfigurationBuilder.Add{T}(T, bool)"/>
        /// </summary>
        public object NullInstance { get; internal set; }

        /// <summary>
        /// Is the system required? <see cref="GameSystems.ConfigurationBuilder.Add{T}(T, bool)"/>
        /// </summary>
        public bool Required { get; internal set; }
    }
}
