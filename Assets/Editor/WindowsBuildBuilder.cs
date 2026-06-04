using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WitcherRightVersion.Editor
{
    public static class WindowsBuildBuilder
    {
        private const string OutputPath = "Builds/Windows/The Witcher 4 Right Version.exe";

        [MenuItem("Tools/Witcher Right Version/Build Windows Player")]
        public static void Build()
        {
            MvpAcceptanceValidator.Validate();

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

            var options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Scenes/MainMenuScene.unity",
                    "Assets/Scenes/VillageScene.unity",
                    "Assets/Scenes/ForestScene.unity",
                    "Assets/Scenes/AshRoadScene.unity",
                    "Assets/Scenes/VelemarWorldScene.unity"
                },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                var message = $"Windows build failed: {report.summary.result}";
                Debug.LogError(message);
                throw new InvalidOperationException(message);
            }

            Debug.Log($"Windows build succeeded: {OutputPath} ({report.summary.totalSize} bytes)");
        }
    }
}
