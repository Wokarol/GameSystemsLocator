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

        internal Action<object> WhenReadyCallbacks = null;

        public SystemContainer(object nullInstance, bool required, bool noOverride)
        {
            NullInstance = nullInstance;
            Required = required;
            HasNoOverrides = noOverride;
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

                if (boundInstance == null) // TODO: Check if the null check performed here actually catches fake Unity nulls
                    return NullInstance;

                return boundInstance;
            }
        }

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

        internal bool HasInstanceBound => boundInstances.Count > 0;

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