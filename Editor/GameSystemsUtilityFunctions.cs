using UnityEditor;
using UnityEngine;
using System.IO;

namespace Wokarol.GameSystemsLocator.Editor
{
    // Written (mostly) by Claude
    public static class GameSystemsUtilityFunctions
    {
        [MenuItem("Tools/Game System Locator/Setup System Locator")]
        public static void CreateGameSystems()
        {
            CreateSystemsPrefab();
            CreateGameConfigScript();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Game System Locator/Setup System Locator", true)]
        public static bool CheckIfGameSystemsWereSetup()
        {
            var paths = GetProjectPaths();

            // Check if Systems prefab exists
            bool prefabExists = File.Exists(paths.prefabPath);

            // Check if GameConfig script exists
            bool scriptExists = File.Exists(paths.scriptPath);

            // Return true if either doesn't exist (menu item should be enabled)
            return !prefabExists || !scriptExists;
        }

        private static (string prefabPath, string scriptPath, string resourcesPath, string scriptsPath) GetProjectPaths()
        {
            bool hasProjectFolder = Directory.Exists("Assets/_Project");

            if (hasProjectFolder)
            {
                return (
                    prefabPath: "Assets/_Project/Content/Global/Resources/Systems.prefab",
                    scriptPath: "Assets/_Project/Scripts/Runtime/Global/GameConfig.cs",
                    resourcesPath: "Assets/_Project/Content/Global/Resources",
                    scriptsPath: "Assets/_Project/Scripts/Runtime/Global"
                );
            }
            else
            {
                return (
                    prefabPath: "Assets/Resources/Systems.prefab",
                    scriptPath: "Assets/Scripts/GameConfig.cs",
                    resourcesPath: "Assets/Resources",
                    scriptsPath: "Assets/Scripts"
                );
            }
        }

        private static void CreateSystemsPrefab()
        {
            var paths = GetProjectPaths();

            // Ensure Resources folder exists
            if (!Directory.Exists(paths.resourcesPath))
            {
                Directory.CreateDirectory(paths.resourcesPath);
            }

            // Check if prefab already exists
            if (File.Exists(paths.prefabPath))
            {
                Debug.Log("Systems prefab already exists at: " + paths.prefabPath);
                return;
            }

            // Create empty GameObject
            GameObject systemsObject = new GameObject("Systems");

            // Create prefab from GameObject
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(systemsObject, paths.prefabPath);

            // Destroy the temporary GameObject in scene
            Object.DestroyImmediate(systemsObject);

            Debug.Log("Created Systems prefab at: " + paths.prefabPath);
        }

        private static void CreateGameConfigScript()
        {
            var paths = GetProjectPaths();

            // Ensure Scripts folder exists
            if (!Directory.Exists(paths.scriptsPath))
            {
                Directory.CreateDirectory(paths.scriptsPath);
            }

            // Check if script already exists
            if (File.Exists(paths.scriptPath))
            {
                Debug.Log("GameConfig script already exists at: " + paths.scriptPath);
                return;
            }

            // Get the root namespace based on project settings
            string rootNamespace = GetRootNamespace();

            // Generate the script content
            string scriptContent = GenerateGameConfigScript(rootNamespace);

            // Write the script to file
            File.WriteAllText(paths.scriptPath, scriptContent);

            Debug.Log("Created GameConfig script at: " + paths.scriptPath);
        }

        private static string GetRootNamespace()
        {
            // First, try to get namespace from assembly definition
            string asmDefNamespace = GetNamespaceFromAssemblyDefinition();
            if (!string.IsNullOrEmpty(asmDefNamespace))
            {
                return asmDefNamespace;
            }

            // Fall back to Unity's root namespace setting
            string rootNamespace = EditorSettings.projectGenerationRootNamespace;

            // Return the root namespace (can be empty/null)
            return rootNamespace;
        }

        private static string GetNamespaceFromAssemblyDefinition()
        {
            var paths = GetProjectPaths();

            // Look for assembly definition in Scripts folder and parent folders
            string currentPath = paths.scriptsPath;

            while (currentPath.StartsWith("Assets"))
            {
                // Look for .asmdef files in current directory
                if (Directory.Exists(currentPath))
                {
                    string[] asmdefFiles = Directory.GetFiles(currentPath, "*.asmdef", SearchOption.TopDirectoryOnly);

                    if (asmdefFiles.Length > 0)
                    {
                        // Parse the first assembly definition found
                        string asmdefPath = asmdefFiles[0];
                        string asmdefContent = File.ReadAllText(asmdefPath);

                        try
                        {
                            // Parse JSON to get rootNamespace
                            var asmDef = JsonUtility.FromJson<AssemblyDefinitionData>(asmdefContent);
                            if (!string.IsNullOrEmpty(asmDef.rootNamespace))
                            {
                                return asmDef.rootNamespace;
                            }

                            // If no rootNamespace specified, use the assembly name as namespace
                            if (!string.IsNullOrEmpty(asmDef.name))
                            {
                                return asmDef.name;
                            }
                        }
                        catch (System.Exception)
                        {
                            // If JSON parsing fails, continue looking
                        }
                    }
                }

                // Move up one directory level
                int lastSlash = currentPath.LastIndexOf('/');
                if (lastSlash <= 6) // "Assets".Length = 6
                    break;
                currentPath = currentPath.Substring(0, lastSlash);
            }

            return null; // No assembly definition found
        }

        [System.Serializable]
        private class AssemblyDefinitionData
        {
            public string name;
            public string rootNamespace;
        }

        private static string GenerateGameConfigScript(string rootNamespace)
        {
            string namespaceStart = "";
            string namespaceEnd = "";
            string indentation = "";

            if (!string.IsNullOrEmpty(rootNamespace))
            {
                namespaceStart = $"namespace {rootNamespace}\n{{\n";
                namespaceEnd = "}";
                indentation = "    ";
            }

            return $@"using Wokarol.GameSystemsLocator.Bootstrapping;
using Wokarol.GameSystemsLocator.Core;

{namespaceStart}{indentation}public class GameConfig : ISystemConfiguration
{indentation}{{
{indentation}    public void Configure(ServiceLocatorBuilder builder)
{indentation}    {{
{indentation}        builder.PrefabPath = ""Systems"";

{indentation}        
{indentation}    }}
{indentation}}}
{namespaceEnd}";
        }
    }
}