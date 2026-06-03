using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WitcherRightVersion.Save;

namespace WitcherRightVersion.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject settingsPanel;

        [Header("Feedback")]
        public Text titleText;
        public Text subtitleText;
        public Text statusText;
        public Text newGameButtonText;
        public Text continueButtonText;
        public Text settingsButtonText;
        public Text exitButtonText;
        public Text settingsTitleText;
        public Text volumeLabelText;
        public Text musicLabelText;
        public Text resolutionLabelText;
        public Text graphicsLabelText;
        public Text languageLabelText;
        public Text applyButtonText;
        public Text backButtonText;

        [Header("Settings")]
        public Slider volumeSlider;
        public Toggle musicToggle;
        public Dropdown resolutionDropdown;
        public Dropdown graphicsDropdown;
        public Dropdown languageDropdown;

        private const string FirstGameScene = "VillageScene";
        private const string VolumeKey = "settings.volume";
        private const string MusicKey = "settings.music";
        private const string ResolutionKey = "settings.resolution";
        private const string GraphicsKey = "settings.graphics";
        private const string LanguageKey = "settings.language";

        private void Awake()
        {
            LoadSettings();
            ShowMainPanel();
            ApplyLanguage();
            SetStatus(GetText("Ready for a new contract.", "Готов к новому контракту."));
        }

        public void StartNewGame()
        {
            SceneManager.LoadScene(FirstGameScene);
        }

        public void ContinueGame()
        {
            if (!SaveService.HasAutosave())
            {
                SetStatus(GetText("No autosave yet. Start a new game first.", "Автосохранения пока нет. Сначала начни новую игру."));
                return;
            }

            SaveService.RequestAutosaveLoadOnNextScene();
            SceneManager.LoadScene(FirstGameScene);
        }

        public void ShowSettingsPanel()
        {
            SetActive(mainPanel, false);
            SetActive(settingsPanel, true);
            SetStatus(GetText("Settings opened.", "Настройки открыты."));
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
            var languageIndex = languageDropdown != null ? languageDropdown.value : 0;

            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(VolumeKey, volume);
            PlayerPrefs.SetInt(MusicKey, musicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(ResolutionKey, resolutionIndex);
            PlayerPrefs.SetInt(GraphicsKey, graphicsIndex);
            PlayerPrefs.SetInt(LanguageKey, languageIndex);
            PlayerPrefs.Save();

            ApplyLanguage();
            SetStatus(GetText("Settings saved.", "Настройки сохранены."));
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Exit requested from MainMenuScene.");
            SetStatus(GetText("Exit requested. In a build this closes the game.", "Выход запрошен. В билде игра закроется."));
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
            var languageIndex = PlayerPrefs.GetInt(LanguageKey, 0);

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

            if (languageDropdown != null)
            {
                languageDropdown.value = Mathf.Clamp(languageIndex, 0, languageDropdown.options.Count - 1);
                languageDropdown.RefreshShownValue();
            }

            AudioListener.volume = volume;
        }

        private void ApplyLanguage()
        {
            var russian = IsRussian();

            SetText(titleText, "The Witcher 4", "Ведьмак 4");
            SetText(subtitleText, "Right Version", "Правильная версия");
            SetText(newGameButtonText, "New Game", "Новая игра");
            SetText(continueButtonText, "Continue", "Продолжить");
            SetText(settingsButtonText, "Settings", "Настройки");
            SetText(exitButtonText, "Exit", "Выход");
            SetText(settingsTitleText, "Settings", "Настройки");
            SetText(volumeLabelText, "Volume", "Громкость");
            SetText(musicLabelText, "Music", "Музыка");
            SetText(resolutionLabelText, "Resolution", "Разрешение");
            SetText(graphicsLabelText, "Graphics", "Графика");
            SetText(languageLabelText, "Language", "Язык");
            SetText(applyButtonText, "Apply", "Применить");
            SetText(backButtonText, "Back", "Назад");

            if (languageDropdown != null && languageDropdown.options.Count >= 2)
            {
                languageDropdown.options[0].text = russian ? "Английский" : "English";
                languageDropdown.options[1].text = russian ? "Русский" : "Russian";
                languageDropdown.RefreshShownValue();
            }
        }

        private bool IsRussian()
        {
            return languageDropdown != null && languageDropdown.value == 1;
        }

        private string GetText(string english, string russian)
        {
            return IsRussian() ? russian : english;
        }

        private void SetText(Text target, string english, string russian)
        {
            if (target != null)
            {
                target.text = GetText(english, russian);
            }
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
