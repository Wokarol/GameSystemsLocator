using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[assembly: InternalsVisibleTo("Wokarol.GameSystemsLocator.Tests")]
namespace Wokarol.GameSystemsLocator
{
    public static class GameSystems
    {
        public static IEnumerable<(Type Type, SystemBinding Binding)> Systems => throw new NotImplementedException();
        public static void Clear() => throw new NotImplementedException();
        public static T Get<T>() where T : class => throw new NotImplementedException();
        public static object Get(Type type) => throw new NotImplementedException();
        public static bool TryGet<T>(out T system) where T : class => throw new NotImplementedException();
        public static bool TryGet(Type type, out object system) => throw new NotImplementedException();
        internal static void ApplyOverride(GameObject holder, List<GameObject> systems = null) => throw new NotImplementedException();

        internal static void Initialize(GameObject systemsObject, Action<ServiceLocatorBuilder> value) => throw new NotImplementedException();

        internal static void InitializeSystemsObject(GameObject systemsObject) => throw new NotImplementedException();
        internal static void RemoveOverride(GameObject holder, List<GameObject> systems = null) => throw new NotImplementedException();


        public class SystemBinding
        {

        }
    }

    public class ServiceLocator
    {

    }

    public class ServiceLocatorBuilder
    {
        public string PrefabPath
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool IsSystemPrefabSet => throw new NotImplementedException();
        public void Add<T>(T nullObject = null, bool required = false) where T : class => throw new NotImplementedException();
        public void Add(Type type, object nullObject, bool required = false) => throw new NotImplementedException();
    }
}