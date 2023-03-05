using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wokarol.GameSystemsLocator
{
    /// <summary>
    /// Should not be used in code, provides functionality for starting the Game Systems Locator on game start
    /// </summary>
    public partial class GameSystemsBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
            GameSystems.Clear();
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AfterSceneLoaded()
        {
            var scene = SceneManager.GetActiveScene();

            // God forgive me for what I've done, this check prevents bootstrapping for play mode tests
            if (scene.name.StartsWith("InitTestScene"))
                return;

            Initialize();
        }

        private static void Initialize()
        {
            // Holder is created to prevent Awake from running when the systems are spawned
            var temporaryHolder = new GameObject();
            temporaryHolder.SetActive(false);

            try
            {
                var systemsObject = SetupGameSystems(temporaryHolder);

                // Holder is removed so the the Awake methods can run
                systemsObject.transform.SetParent(null);
                UnityEngine.Object.DontDestroyOnLoad(systemsObject);
            }
            finally
            {
                UnityEngine.Object.Destroy(temporaryHolder);
            }
        }

        private static GameObject SetupGameSystems(GameObject temporaryHolder)
        {
            GameSystems.Clear();

            var builder = ConfigureGameSystems();
            var systemsObject = CreateSystems(temporaryHolder, builder);

            GameSystems.InitializeSystemsObject(systemsObject);

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
            if (prefab == null)
            {
                throw new InvalidOperationException($"There is no prefab in Resources at \"{builder.PrefabPath}\". Make sure the prefab name is typed correctly");
            }

            var systemsObject = UnityEngine.Object.Instantiate(prefab, temporaryHolder.transform);
            systemsObject.name = prefab.name;
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

    /// <summary>
    /// Mark a class that will be instanced and used for configuring the Game Systems Locator
    /// </summary>
    public interface ISystemConfiguration
    {
        /// <summary>
        /// Run as the Game Systems Locator is bootrstrapped and should define types locator should expect 
        /// </summary>
        /// <param name="builder">The builder providing method needed to configure the locator</param>
        void Configure(GameSystems.ConfigurationBuilder builder);
    }
}
