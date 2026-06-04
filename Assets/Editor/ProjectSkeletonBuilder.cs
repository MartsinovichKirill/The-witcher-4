using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WitcherRightVersion.Editor
{
    public static class ProjectSkeletonBuilder
    {
        private static readonly string[] RequiredFolders =
        {
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Scripts/Core",
            "Assets/Scripts/Player",
            "Assets/Scripts/Combat",
            "Assets/Scripts/Dialogue",
            "Assets/Scripts/Quest",
            "Assets/Scripts/Inventory",
            "Assets/Scripts/Crafting",
            "Assets/Scripts/Save",
            "Assets/Scripts/UI",
            "Assets/Prefabs",
            "Assets/ScriptableObjects",
            "Assets/Art",
            "Assets/Audio",
            "Assets/Materials",
            "Assets/Docs"
        };

        [MenuItem("Tools/Witcher Right Version/Build Project Skeleton")]
        public static void Create()
        {
            foreach (var folder in RequiredFolders)
            {
                Directory.CreateDirectory(folder);
            }

            AssetDatabase.Refresh();

            CreateSceneIfMissing("MainMenuScene");
            CreateSceneIfMissing("VillageScene");
            CreateSceneIfMissing("ForestScene");
            CreateSceneIfMissing("AshRoadScene");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/VillageScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/ForestScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/AshRoadScene.unity", true)
            };

            PlayerSettings.companyName = "StudentProject";
            PlayerSettings.productName = "The Witcher 4: Right Version";
            EditorSettings.serializationMode = SerializationMode.ForceText;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateSceneIfMissing(string sceneName)
        {
            var scenePath = $"Assets/Scenes/{sceneName}.unity";
            if (File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject($"{sceneName}Root");
            root.transform.position = Vector3.zero;

            if (sceneName == "VillageScene" || sceneName == "ForestScene" || sceneName == "AshRoadScene")
            {
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = sceneName == "VillageScene"
                    ? "VillageBlockoutGround"
                    : sceneName == "ForestScene" ? "ForestBlockoutGround" : "AshRoadBlockoutGround";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(10f, 1f, 10f);
            }

            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
