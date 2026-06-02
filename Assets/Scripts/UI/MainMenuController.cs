using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WitcherRightVersion.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject settingsPanel;

        [Header("Feedback")]
        public Text statusText;

        [Header("Settings")]
        public Slider volumeSlider;
        public Toggle musicToggle;
        public Dropdown resolutionDropdown;
        public Dropdown graphicsDropdown;

        private const string FirstGameScene = "VillageScene";
        private const string VolumeKey = "settings.volume";
        private const string MusicKey = "settings.music";
        private const string ResolutionKey = "settings.resolution";
        private const string GraphicsKey = "settings.graphics";

        private void Awake()
        {
            LoadSettings();
            ShowMainPanel();
            SetStatus("Ready for a new contract.");
        }

        public void StartNewGame()
        {
            SceneManager.LoadScene(FirstGameScene);
        }

        public void ContinueGame()
        {
            SetStatus("No save file yet. Start a new game first.");
        }

        public void ShowSettingsPanel()
        {
            SetActive(mainPanel, false);
            SetActive(settingsPanel, true);
            SetStatus("Settings opened.");
        }

        public void ShowMainPanel()
        {
            SetActive(mainPanel, true);
            SetActive(settingsPanel, false);
        }

        public void ApplySettings()
        {
            var volume = volumeSlider != null ? volumeSlider.value : 1f;
            var musicEnabled = musicToggle == null || musicToggle.isOn;
            var resolutionIndex = resolutionDropdown != null ? resolutionDropdown.value : 0;
            var graphicsIndex = graphicsDropdown != null ? graphicsDropdown.value : 0;

            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(VolumeKey, volume);
            PlayerPrefs.SetInt(MusicKey, musicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(ResolutionKey, resolutionIndex);
            PlayerPrefs.SetInt(GraphicsKey, graphicsIndex);
            PlayerPrefs.Save();

            SetStatus("Settings saved.");
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Exit requested from MainMenuScene.");
            SetStatus("Exit requested. In a build this closes the game.");
#else
            Application.Quit();
#endif
        }

        private void LoadSettings()
        {
            var volume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
            var musicEnabled = PlayerPrefs.GetInt(MusicKey, 1) == 1;
            var resolutionIndex = PlayerPrefs.GetInt(ResolutionKey, 0);
            var graphicsIndex = PlayerPrefs.GetInt(GraphicsKey, 1);

            if (volumeSlider != null)
            {
                volumeSlider.value = volume;
            }

            if (musicToggle != null)
            {
                musicToggle.isOn = musicEnabled;
            }

            if (resolutionDropdown != null)
            {
                resolutionDropdown.value = Mathf.Clamp(resolutionIndex, 0, resolutionDropdown.options.Count - 1);
            }

            if (graphicsDropdown != null)
            {
                graphicsDropdown.value = Mathf.Clamp(graphicsIndex, 0, graphicsDropdown.options.Count - 1);
            }

            AudioListener.volume = volume;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}

