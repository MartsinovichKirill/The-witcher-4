using UnityEngine;

namespace WitcherRightVersion.Core
{
    public static class RuntimeSettingsService
    {
        public const string VolumeKey = "settings.volume";
        public const string MusicKey = "settings.music";
        public const string ResolutionKey = "settings.resolution";
        public const string GraphicsKey = "settings.graphics";
        public const string ScreenModeKey = "settings.screenMode";
        public const string SharpnessKey = "settings.sharpness";
        public const string BlurKey = "settings.blur";

        private static readonly Vector2Int[] Resolutions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080)
        };

        public static void Apply(float volume, bool musicEnabled, int resolutionIndex, int graphicsIndex, int screenModeIndex, bool sharpnessEnabled, bool blurEnabled)
        {
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, Resolutions.Length - 1);
            graphicsIndex = Mathf.Clamp(graphicsIndex, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            screenModeIndex = Mathf.Clamp(screenModeIndex, 0, 2);

            AudioListener.volume = Mathf.Clamp01(volume);
            QualitySettings.SetQualityLevel(graphicsIndex, true);
            // Re-apply the cinematic shadow boost: SetQualityLevel resets these to level defaults.
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 85f);
            QualitySettings.shadowCascades = 2;
            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 8);
            QualitySettings.anisotropicFiltering = sharpnessEnabled ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing = blurEnabled ? 2 : 0;

            var resolution = Resolutions[resolutionIndex];
            var mode = screenModeIndex == 0
                ? FullScreenMode.ExclusiveFullScreen
                : screenModeIndex == 1
                    ? FullScreenMode.FullScreenWindow
                    : FullScreenMode.Windowed;
            Screen.SetResolution(resolution.x, resolution.y, mode);

            PlayerPrefs.SetFloat(VolumeKey, volume);
            PlayerPrefs.SetInt(MusicKey, musicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(ResolutionKey, resolutionIndex);
            PlayerPrefs.SetInt(GraphicsKey, graphicsIndex);
            PlayerPrefs.SetInt(ScreenModeKey, screenModeIndex);
            PlayerPrefs.SetInt(SharpnessKey, sharpnessEnabled ? 1 : 0);
            PlayerPrefs.SetInt(BlurKey, blurEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
