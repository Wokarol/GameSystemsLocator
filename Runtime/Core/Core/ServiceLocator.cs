using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wokarol.GameSystemsLocator.Old;

namespace Wokarol.GameSystemsLocator.Core
{
    // Note: This class now takes over the logic of GameSystems to move it away from static
    public class ServiceLocator
    {
        private readonly Dictionary<Type, SystemContainer> systems = new();
        private bool isInitialized = false;

        public IEnumerable<KeyValuePair<Type, SystemContainer>> Systems => systems;


        public void Initialize(Action<ServiceLocatorBuilder> configCallback, GameObject systemsRoot = null) => Initialize(configCallback, b => systemsRoot);

        public void Initialize(Action<ServiceLocatorBuilder> configCallback, Func<ServiceLocatorBuilder, GameObject> createSystemsRoot)
        {
            if (isInitialized)
                throw new InvalidOperationException("Service Locator cannot be initialized twice, Clear the locator before second initilization");

            var builder = new ServiceLocatorBuilder(this);
            configCallback(builder);

            var root = createSystemsRoot(builder);
            if (root != null) BindSystemsFromObject(root);

            isInitialized = true;
        }

        public void Clear()
        {
            isInitialized = false;
            systems.Clear();
        }

        public bool TryGet<T>(out T system) where T : class
        {
            system = Get<T>();
            return system != null;
        }

        public bool TryGet(Type type, out object system)
        {
            system = Get(type);
            return system != null;
        }

        public T Get<T>() where T : class => (T)Get(typeof(T));
        public object Get(Type type)
        {
            AssertInitialization();

            if (!systems.TryGetValue(type, out var boundSystem))
                throw new InvalidOperationException("The type was not registered as the game system");

            var instance = boundSystem.Instance;

            if (instance == null && boundSystem.Required)
            {
                Debug.LogWarning($"Tried to get a required system {type} but found null");
            }

            return instance;
        }

        public void ApplyOverride(GameObject holder = null, List<GameObject> overrides = null)
        {
            throw new NotImplementedException();
        }

        public void RemoveOverride(GameObject holder = null, List<GameObject> overrides = null)
        {
            throw new NotImplementedException();
        }

        // Note: This method now takes the responsibility of ConfigurationBuilder.Add
        internal void Add(Type type, object nullObject, bool required)
        {
            if (systems.ContainsKey(type))
                throw new InvalidOperationException("The system type can only be registered once");

            systems[type] = new SystemContainer(nullObject, required);
        }

        private void AssertInitialization()
        {
            if (!isInitialized)
                throw new InvalidOperationException("Service Locator is not yet initialized, cannot perform the operation");
        }

        private void BindSystemsFromObject(GameObject rootObject, bool errorOnRequired = true)
        {
            foreach (var system in systems)
            {
                var s = rootObject.GetComponentInChildren(system.Key, true);

                if (s != null)
                {
                    system.Value.BoundInstances.Add(s);
                }
                else
                {
                    if (system.Value.Required && errorOnRequired)
                        Debug.LogError($"The binding for {system.Key.FullName} is required");
                }
            }
        }
    }
}