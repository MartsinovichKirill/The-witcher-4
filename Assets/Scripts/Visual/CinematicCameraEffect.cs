using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace WitcherRightVersion.Visual
{
    /// <summary>
    /// Built-in pipeline post effect: bloom, ACES tonemap, contrast/saturation
    /// grading, and vignette. Attaches itself to the main camera of gameplay
    /// scenes at runtime, so generated scenes do not need to be rebuilt.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CinematicCameraEffect : MonoBehaviour
    {
        [SerializeField] private float exposure = 0.86f;
        [SerializeField] private float contrast = 1.08f;
        [SerializeField] private float saturation = 1.16f;
        [SerializeField] private float bloomThreshold = 1.15f;
        [SerializeField] private float bloomIntensity = 0.22f;
        [SerializeField] private float vignetteStrength = 0.34f;

        private Material material;
        private static bool bootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (bootstrapped)
            {
                return;
            }

            bootstrapped = true;
            SceneManager.sceneLoaded += (scene, mode) => AttachToMainCamera();
            AttachToMainCamera();
        }

        private static void AttachToMainCamera()
        {
            // The main menu is a flat UI photo; grading only the 3D scenes.
            if (SceneManager.GetActiveScene().name == "MainMenuScene")
            {
                return;
            }

            ApplyQualityBoost();
            UpgradeAmbientLighting();

            var mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<CinematicCameraEffect>() == null)
            {
                mainCamera.gameObject.AddComponent<CinematicCameraEffect>();
            }
        }

        private static void ApplyQualityBoost()
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.High;
            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 85f);
            QualitySettings.shadowCascades = 2;
            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 8);
        }

        private static void UpgradeAmbientLighting()
        {
            if (RenderSettings.ambientMode != AmbientMode.Flat)
            {
                return;
            }

            var baseAmbient = RenderSettings.ambientLight;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = baseAmbient * 1.45f + new Color(0.015f, 0.03f, 0.055f, 0f);
            RenderSettings.ambientEquatorColor = baseAmbient;
            RenderSettings.ambientGroundColor = baseAmbient * 0.5f + new Color(0.02f, 0.012f, 0.008f, 0f);
        }

        private void Awake()
        {
            var shader = Resources.Load<Shader>("Shaders/CinematicGrading");
            if (shader == null || !shader.isSupported)
            {
                Debug.LogWarning("CinematicGrading shader unavailable; post effect disabled.", this);
                enabled = false;
                return;
            }

            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        private void OnDestroy()
        {
            if (material != null)
            {
                Destroy(material);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (material == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            material.SetFloat("_Exposure", exposure);
            material.SetFloat("_Contrast", contrast);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_BloomThreshold", bloomThreshold);
            material.SetFloat("_BloomIntensity", bloomIntensity);
            material.SetFloat("_VignetteStrength", vignetteStrength);

            var half = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, source.format);
            var quarter = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, source.format);
            var eighth = RenderTexture.GetTemporary(source.width / 8, source.height / 8, 0, source.format);

            material.SetFloat("_BlurOffset", 1f);
            Graphics.Blit(source, half, material, 0);
            material.SetFloat("_BlurOffset", 1.25f);
            Graphics.Blit(half, quarter, material, 1);
            material.SetFloat("_BlurOffset", 1.5f);
            Graphics.Blit(quarter, eighth, material, 1);

            material.SetTexture("_BloomTex", eighth);
            Graphics.Blit(source, destination, material, 2);

            RenderTexture.ReleaseTemporary(half);
            RenderTexture.ReleaseTemporary(quarter);
            RenderTexture.ReleaseTemporary(eighth);
        }
    }
}
