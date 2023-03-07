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
        public static IEnumerable<(Type Type, SystemContainer Binding)> Systems => throw new NotImplementedException();
        public static void Clear() => throw new NotImplementedException();
        public static T Get<T>() where T : class => throw new NotImplementedException();
        // Note: checking for null instance is no longer a responsibility of the Get method
        public static object Get(Type type) => throw new NotImplementedException();
        public static bool TryGet<T>(out T system) where T : class => throw new NotImplementedException();
        public static bool TryGet(Type type, out object system) => throw new NotImplementedException();
        internal static void ApplyOverride(GameObject holder, List<GameObject> systems = null) => throw new NotImplementedException();

        internal static void Initialize(GameObject systemsObject, Action<ServiceLocatorBuilder> value) => throw new NotImplementedException();

        internal static void InitializeSystemsObject(GameObject systemsObject) => throw new NotImplementedException();
        internal static void RemoveOverride(GameObject holder, List<GameObject> systems = null) => throw new NotImplementedException();
    }
}