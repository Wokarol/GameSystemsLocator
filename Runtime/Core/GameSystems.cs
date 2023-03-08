using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Wokarol.GameSystemsLocator.Core;

[assembly: InternalsVisibleTo("Wokarol.GameSystemsLocator.Tests")]
namespace Wokarol.GameSystemsLocator
{
    public static class GameSystems
    {
        private static ServiceLocator locator = new ServiceLocator();

        /// <inheritdoc cref="ServiceLocator.Systems"/>
        public static IEnumerable<KeyValuePair<Type, SystemContainer>> Systems => locator.Systems;

        /// <inheritdoc cref="ServiceLocator.Initialize(Action{ServiceLocatorBuilder}, GameObject)"/>
        internal static void Initialize(Action<ServiceLocatorBuilder> configCallback, GameObject systemsRoot = null)
        {
            locator.Clear();
            locator.Initialize(configCallback, systemsRoot);
        }
        
        /// <inheritdoc cref="ServiceLocator.Initialize(Action{ServiceLocatorBuilder}, Func{ServiceLocatorBuilder, GameObject})"/>
        internal static void Initialize(Action<ServiceLocatorBuilder> configCallback, Func<ServiceLocatorBuilder, GameObject> createSystemsRoot)
        {
            locator.Clear();
            locator.Initialize(configCallback, createSystemsRoot);
        }
        
        /// <inheritdoc cref="ServiceLocator.Clear"/>
        public static void Clear() => locator.Clear();

        
        /// <inheritdoc cref="ServiceLocator.Get{T}"/>
        public static T Get<T>() where T : class => locator.Get<T>();

        /// <inheritdoc cref="ServiceLocator.Get(Type)"/>
        public static object Get(Type type) => locator.Get(type);
        
        /// <inheritdoc cref="ServiceLocator.TryGet{T}(out T)"/>
        public static bool TryGet<T>(out T system) where T : class => locator.TryGet<T>(out system);
        
        /// <inheritdoc cref="ServiceLocator.TryGet(Type, out object)"/>
        public static bool TryGet(Type type, out object system) => locator.TryGet(type, out system);
        
        /// <inheritdoc cref="ServiceLocator.ApplyOverride(GameObject, List{GameObject})"/>
        internal static void ApplyOverride(GameObject holder, List<GameObject> systems = null) => locator.ApplyOverride(holder, systems);
        
        /// <inheritdoc cref="ServiceLocator.RemoveOverride(GameObject, List{GameObject})"/>
        internal static void RemoveOverride(GameObject holder, List<GameObject> systems = null) => locator.RemoveOverride(holder, systems);

    }
}