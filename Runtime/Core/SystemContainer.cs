using System;
using System.Collections.Generic;

namespace Wokarol.GameSystemsLocator.Core
{
    /// <summary>
    /// Container for a game system, contains instances and properties
    /// Note: Type information is not kept in the container
    /// </summary>
    public class SystemContainer
    {
        private readonly List<object> boundInstances = new List<object>();

        /// <summary>
        /// Defaut object to be returned where there is no instance bound to the container.
        /// For more information see <see cref="ServiceLocatorBuilder.Add{T}(T, bool)"/>
        /// </summary>
        public readonly object NullInstance;

        /// <summary>
        /// Defines if there always should be a system bound to this container
        /// For more information see <see cref="ServiceLocatorBuilder.Add{T}(T, bool)"/>
        /// </summary>
        public readonly bool Required;

        /// <summary>
        /// Defines if this system can have overrides or if the instance is only present in the root
        /// For more information see <see cref="ServiceLocatorBuilder.Add{T}(T, bool)"/>
        /// </summary>
        public readonly bool HasNoOverrides;

        /// <summary>
        /// If set, signals to the bootstrapper than an instance of a system should be created during bootstrapping
        /// </summary>
        public readonly bool CreateIfNotPresent;

        internal bool HasInstanceBound => boundInstances.Count > 0;

        internal bool MarkedForDestruction { get; set; }


        internal Action<object> WhenReadyCallbacks = null;

        public SystemContainer(object nullInstance, bool required, bool noOverride, bool createIfNotPresent)
        {
            NullInstance = nullInstance;
            Required = required;
            HasNoOverrides = noOverride;
            CreateIfNotPresent = createIfNotPresent;
            MarkedForDestruction = false;
        }

        /// <summary>
        /// List of instances bound to the container
        /// </summary>
        public IReadOnlyList<object> Instances => boundInstances;

        /// <summary>
        /// Newest instance bound to the container, considered the main one
        /// </summary>
        public object Instance
        {
            get
            {
                var boundInstance = boundInstances.Count == 0
                    ? null
                    : boundInstances[boundInstances.Count - 1];

                if (boundInstance == null)
                    return NullInstance;

                if (boundInstances.Count > 0 && (boundInstance == null || boundInstance is UnityEngine.Object obj && obj == null))
                {
                    // This property is set when the editor closes or the game stops by listening to the OnDestroy message on the root game object (provided or created)
                    if (!MarkedForDestruction)
                    {
                        UnityEngine.Debug.LogError("Bound instance in the list is null. That suggests a system object was destroyed without being removed from the service locator");
                        return NullInstance;
                    }
                }

                return boundInstance;
            }
        }

        internal void BindInstance(object instance)
        {
            boundInstances.Add(instance);

            if (WhenReadyCallbacks != null)
            {
                WhenReadyCallbacks(Instance);
                WhenReadyCallbacks = null;
            }
        }

        internal void UnbindInstance(object instance)
        {
            boundInstances.Remove(instance);
        }
    }
}