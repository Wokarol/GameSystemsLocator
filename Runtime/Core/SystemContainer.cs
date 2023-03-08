using System.Collections.Generic;

namespace Wokarol.GameSystemsLocator.Core
{
    /// <summary>
    /// Container for a game system, contains instances and properties
    /// Note: Type information is not kept in the container
    /// </summary>
    public class SystemContainer
    {
        internal readonly List<object> BoundInstances = new List<object>();

        public SystemContainer(object nullInstance, bool required)
        {
            NullInstance = nullInstance;
            Required = required;
        }

        /// <summary>
        /// List of instances bound to the container
        /// </summary>
        public IReadOnlyList<object> Instances => BoundInstances;

        /// <summary>
        /// Newest instance bound to the container, considered the main one
        /// </summary>
        public object Instance
        {
            get
            {
                var boundInstance = BoundInstances.Count == 0
                    ? null
                    : BoundInstances[BoundInstances.Count - 1];

                if (boundInstance == null) // <- Check if the null check performed here actually catches fake Unity nulls
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
    }
}