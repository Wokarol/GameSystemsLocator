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
        private static bool isSystemsObjectInitialized = false;
        private static bool isSystemsPrefabSet = false;

        public static bool IsReady => isSystemsObjectInitialized || !isSystemsPrefabSet;

        /// <summary>
        /// List of all systems in form of (Type, Binding) tuple. Gets all registered systems in the locator
        /// </summary>
        public static IEnumerable<(Type Type, SystemBinding Binding)> Systems => systems.Select(kv => (kv.Key, kv.Value));


        /// <summary>
        /// Locates Game System matching the type
        /// </summary>
        /// <typeparam name="T">Type of the system to locate</typeparam>
        /// <returns>The system that was located or null if no instance is found</returns>
        /// <exception cref="InvalidOperationException">Thrown if given type T is not registered in the locator</exception>
        public static T Get<T>() where T : class
        {
            return (T)Get(typeof(T));
        }

        /// <summary>
        /// Locates Game System matching the type
        /// </summary>
        /// <param name="type">Type of the system to locate</param>
        /// <returns>The system that was located or null if no instance is found</returns>
        /// <exception cref="InvalidOperationException">Thrown if given type T is not registered in the locator</exception>
        public static object Get(Type type)
        {
            if (!systems.TryGetValue(type, out var boundSystem))
                throw new InvalidOperationException("The type was not registered as the game system");

            var instance = boundSystem.Instance;
            var nullInstance = boundSystem.NullInstance;

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
                    Debug.LogWarning($"Tried to get a required system {type} but found null");
                }
                return null;
            }
        }

        // TODO: Fill docs
        public static bool TryGet<T>(out T service) where T : class
        {
            service = Get<T>();
            return service != null;
        }

        public static bool TryGet(Type type, out object service)
        {
            service = Get(type);
            return service != null;
        }

        internal static void Clear()
        {
            systems.Clear();
            isSystemsObjectInitialized = false;
        }

        internal static void InitializeSystemsObject(GameObject systemsObject)
        {
            if (systemsObject == null)
                return;

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

            isSystemsObjectInitialized = true;
        }

        internal static void Initialize(GameObject systemsObject, Action<ConfigurationBuilder> configCallback)
        {
            configCallback(new ConfigurationBuilder());
            InitializeSystemsObject(systemsObject);
        }

        internal static void ApplyOverride(GameObject holder, List<GameObject> overrides = null)
        {
            if (!IsReady)
            {
                throw new InvalidOperationException("Applied override before the game systems was ready, that should never happen");
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
            private string prefabPath = "";

            public string PrefabPath
            {
                get => prefabPath; 
                set
                {
                    prefabPath = value;
                    isSystemsPrefabSet = !string.IsNullOrEmpty(prefabPath);
                }
            }

            public bool IsSystemPrefabSet => isSystemsPrefabSet;

            /// <summary>
            /// Adds the type to the locator
            /// </summary>
            /// <typeparam name="T">Type to add to the locator</typeparam>
            /// <param name="nullObject">Optional null object that is used in case no instance is located</param>
            /// <param name="required">Is the system required, if so, additional error will get logged in case an instance is not present</param>
            /// <exception cref="InvalidOperationException">Thrown then the type is registered more than once</exception>
            public void Add<T>(T nullObject = null, bool required = false) where T : class
            {
                Add(typeof(T), nullObject, required);
            }

            /// <summary>
            /// Adds the type to the locator
            /// </summary>
            /// <param name="type">Type to add to the locator</typeparam>
            /// <param name="nullObject">Optional null object that is used in case no instance is located</param>
            /// <param name="required">Is the system required, if so, additional error will get logged in case an instance is not present</param>
            /// <exception cref="InvalidOperationException">Thrown then the type is registered more than once</exception>
            public void Add(Type type, object nullObject,  bool required = false)
            {
                if (systems.TryGetValue(type, out var _))
                    throw new InvalidOperationException("The system type can only be registered once");

                var boundSystem = new SystemBinding()
                {
                    NullInstance = nullObject,
                    Required = required,
                };
                systems.Add(type, boundSystem);
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
