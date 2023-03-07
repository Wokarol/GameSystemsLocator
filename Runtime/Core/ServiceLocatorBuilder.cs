using System;
using Wokarol.GameSystemsLocator.Core;

namespace Wokarol.GameSystemsLocator
{
    /// <summary>
    /// Utility class for configuring the Service Locator
    /// </summary>
    public class ServiceLocatorBuilder
    {
        private readonly ServiceLocator locator;

        /// <summary>
        /// Path to the prefab that should be spawned as the Game Systems root
        /// </summary>
        public string PrefabPath { get; set; }
        public bool IsSystemPrefabSet => String.IsNullOrEmpty(PrefabPath);

        public ServiceLocatorBuilder(ServiceLocator locator)
        {
            this.locator = locator;
        }

        /// <summary>
        /// Adds a container to the service locator
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="nullObject">Optional null object to return when no instance is found</param>
        /// <param name="required">Optional flag enabling additional checks and warnings to make sure there is always an instance bound to the container</param>
        public void Add<T>(T nullObject = null, bool required = false) where T : class
        {
            Add(typeof(T), nullObject, required);
        }

        /// <summary>
        /// Adds a container to the service locator
        /// </summary>
        /// <param name="type">Type of the service</typeparam>
        /// <param name="nullObject">Optional null object to return when no instance is found</param>
        /// <param name="required">Optional flag enabling additional checks and warnings to make sure there is always an instance bound to the container</param>
        public void Add(Type type, object nullObject, bool required = false)
        {
            locator.Add(type, nullObject, required);
        }
    }
}