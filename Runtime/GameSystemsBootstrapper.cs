using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wokarol.GameSystemsLocator
{
    public partial class GameSystemsBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void JustBeforeSplash()
        {
            var scene = SceneManager.GetActiveScene();

            // God forgive me for what I've done, this check prevents bootstrapping for play mode tests
            if (scene.name.StartsWith("InitTestScene"))
                return;

            Initialize();
        }

        public static void Initialize()
        {
            // Holder is created to prevent Awake from running when the systems are spawned
            var temporaryHolder = new GameObject();
            temporaryHolder.SetActive(false);

            var systemsObject = SetupGameSystems(temporaryHolder);

            // Holder is removed so tht the Awake methods can run
            systemsObject.transform.SetParent(null);
            UnityEngine.Object.DontDestroyOnLoad(systemsObject);
        }

        private static GameObject SetupGameSystems(GameObject temporaryHolder)
        {
            var builder = ConfigureGameSystems();
            var systemsObject = CreateSystems(temporaryHolder, builder);

            GameSystems.Initialize(systemsObject);
            return systemsObject;
        }

        private static GameSystems.ConfigurationBuilder ConfigureGameSystems()
        {
            var builder = new GameSystems.ConfigurationBuilder();
            var configurator = GetConfigurator();

            configurator.Configure(builder);
            return builder;
        }

        private static GameObject CreateSystems(GameObject temporaryHolder, GameSystems.ConfigurationBuilder builder)
        {
            var prefab = Resources.Load<GameObject>(builder.PrefabPath);
            var systemsObject = UnityEngine.Object.Instantiate(prefab, temporaryHolder.transform);
            return systemsObject;
        }

        private static ISystemConfiguration GetConfigurator()
        {
            var configuratorTypes = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => typeof(ISystemConfiguration).IsAssignableFrom(t) && t.IsClass)
                            .Take(2)
                            .ToList();

            if (configuratorTypes.Count >= 2)
                throw new Exception("There can only be one system configuration");

            if (configuratorTypes.Count == 0)
                throw new Exception("No configuration was found!");

            var configurator = (ISystemConfiguration)Activator.CreateInstance(configuratorTypes.First());
            return configurator;
        }
    }

    public interface ISystemConfiguration
    {
        void Configure(GameSystems.ConfigurationBuilder builder);
    }
}
