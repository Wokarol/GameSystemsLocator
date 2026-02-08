using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Wokarol.GameSystemsLocator.Core
{
    public class ServiceLocator
    {
        private readonly Dictionary<Type, SystemContainer> systems = new();
        private bool isInitialized = false;

        /// <summary>
        /// Gets all registered systems in the locator
        /// </summary>
        public IEnumerable<KeyValuePair<Type, SystemContainer>> Systems => systems;

        /// <summary>
        /// Initializes the service locator
        /// </summary>
        /// <param name="configCallback">Method called to configure the locator</param>
        /// <param name="systemsRoot">Game Object with systems to bind upon initialization</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Initialize(Action<ServiceLocatorBuilder> configCallback, GameObject systemsRoot = null) => Initialize(configCallback, b => systemsRoot);

        /// <summary>
        /// Initializes the service locator
        /// </summary>
        /// <param name="configCallback">Method called to configure the locator</param>
        /// <param name="createSystemsRoot">Method to call when initializing the system root</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Initialize(Action<ServiceLocatorBuilder> configCallback, Func<ServiceLocatorBuilder, GameObject> createSystemsRoot)
        {
            if (isInitialized)
                throw new InvalidOperationException("Service Locator cannot be initialized twice, Clear the locator before second initilization");

            var builder = new ServiceLocatorBuilder(this);
            configCallback(builder);

            var root = createSystemsRoot(builder);
            if (root != null)
            {
                BindSystemsFromObject(root);
                CreateSystemThatAreNotPresent(root);
            }
            else
            {
                var count = Systems.Count((system) => (system.Value.CreateIfNotPresent && system.Value.Instance == null));

                if (count > 0)
                    Debug.LogWarning($"{count} systems with createIfNotPresent but no system root was provided. Therefore those systems will not be created");
            }

            isInitialized = true;
        }

        private void CreateSystemThatAreNotPresent(GameObject root)
        {
            foreach (var system in Systems)
            {
                if (system.Value.CreateIfNotPresent && system.Value.Instance == null)
                {
                    var systemHost = new GameObject(Regex.Replace(system.Key.Name, @"([a-z])([A-Z])", @"$1 $2"));
                    systemHost.transform.SetParent(root.transform);

                    system.Value.BindInstance(systemHost.AddComponent(system.Key));
                }
            }
        }

        /// <summary>
        /// Clears the locator, removes all containers
        /// </summary>
        public void Clear()
        {
            isInitialized = false;
            systems.Clear();
        }

        /// <summary>
        /// Locates Game System matching the type
        /// </summary>
        /// <typeparam name="T">Type of the system to locate</typeparam>
        /// <param name="system">The system that was located or null if no instance is found</param>
        /// <returns>True if the system is found</returns>
        public bool TryGet<T>(out T system) where T : class
        {
            system = Get<T>();
            return system != null;
        }

        /// <summary>
        /// Locates Game System matching the type
        /// </summary>
        /// <param name="type">Type of the system to locate</param>
        /// <param name="system">The system that was located or null if no instance is found</param>
        /// <returns>True if the system is found</returns>
        public bool TryGet(Type type, out object system)
        {
            system = Get(type);
            return system != null;
        }

        /// <summary>
        /// Locates Game System matching the type
        /// </summary>
        /// <typeparam name="T">Type of the system to locate</typeparam>
        /// <returns>The system that was located or null if no instance is found</returns>
        public T Get<T>() where T : class => (T)Get(typeof(T));

        /// <summary>
        /// Locates Game System matching the type
        /// </summary>
        /// <param name="type">Type of the system to locate</param>
        /// <returns>The system that was located or null if no instance is found</returns>
        public object Get(Type type)
        {
            AssertInitialization();

            if (!systems.TryGetValue(type, out var boundSystem))
                throw new InvalidOperationException($"{type.Name} was not registered as the game system");

            var instance = boundSystem.Instance;

            if (instance == null && boundSystem.Required)
            {
                Debug.LogWarning($"Tried to get a required system {type} but found null");
            }

            return instance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public void GetWhenReady<T>(Action<T> callback) where T : class => GetWhenReady(typeof(T), obj => callback((T)obj));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public void GetWhenReady(Type type, Action<object> callback) // Consider adding a static-lambda-compatible override
        {
            AssertInitialization();

            if (!systems.TryGetValue(type, out var boundSystem))
                throw new InvalidOperationException($"{type.Name} was not registered as the game system");

            var instance = boundSystem.Instance;

            if (boundSystem.HasInstanceBound)
            {
                callback(instance);
                return;
            }

            boundSystem.WhenReadyCallbacks += callback;
        }

        /// <summary>
        /// Applies system overrides to containers
        /// </summary>
        /// <param name="holder">Game Object with systems to bind to the locator</param>
        /// <param name="overrides">List of game objects with systems on them</param>
        public void ApplyOverride(GameObject holder = null, List<GameObject> overrides = null)
        {
            AssertInitialization();

            if (holder != null)
                BindSystemsFromObject(holder, false, true);

            if (overrides != null)
            {
                foreach (var system in Systems)
                {
                    if (system.Value.HasNoOverrides)
                        continue;

                    foreach (var obj in overrides)
                    {
                        if (obj.TryGetComponent(system.Key, out var s))
                            system.Value.BindInstance(s);
                    }
                }
            }
        }

        /// <summary>
        /// Removes system overrides from containers, see: <see cref="ApplyOverride(GameObject, List{GameObject})"/>
        /// </summary>
        public void RemoveOverride(GameObject holder = null, List<GameObject> overrides = null)
        {
            AssertInitialization();

            if (holder != null)
                RemoveSystemsFromObject(holder);

            if (overrides != null)
            {
                foreach (var system in Systems)
                {
                    if (system.Value.HasNoOverrides)
                        continue;

                    foreach (var obj in overrides)
                    {
                        if (obj.TryGetComponent(system.Key, out var s))
                            system.Value.UnbindInstance(s);
                    }
                }
            }
        }

        internal void Add(Type type, object nullObject, bool required, bool noOverride = false, bool createIfNotPresent = false)
        {
            if (systems.ContainsKey(type))
                throw new InvalidOperationException($"The system type can only be registered once ({type.Name})");

            systems[type] = new SystemContainer(nullObject, required, noOverride, createIfNotPresent);
        }

        private void AssertInitialization()
        {
            if (!isInitialized)
                throw new InvalidOperationException("Service Locator is not yet initialized, cannot perform the operation");
        }

        private void BindSystemsFromObject(GameObject rootObject, bool errorOnRequired = true, bool isOverriding = false)
        {
            foreach (var system in systems)
            {
                // We skip doing a get component and all that for non overridable systems
                if (isOverriding && system.Value.HasNoOverrides)
                    continue;

                var s = rootObject.GetComponentInChildren(system.Key, true);

                if (s != null)
                {
                    system.Value.BindInstance(s);
                }
                else
                {
                    if (system.Value.Required && errorOnRequired && !system.Value.CreateIfNotPresent)
                        Debug.LogError($"The binding for {system.Key.FullName} is required");
                }
            }
        }

        private void RemoveSystemsFromObject(GameObject rootObject, bool isOverriding = false)
        {
            foreach (var system in systems)
            {
                // We skip doing a get component and all that for non overridable systems
                if (isOverriding && system.Value.HasNoOverrides)
                    continue;

                var s = rootObject.GetComponentInChildren(system.Key, true);

                if (s != null)
                {
                    system.Value.UnbindInstance(s);
                }
            }
        }
    }
}