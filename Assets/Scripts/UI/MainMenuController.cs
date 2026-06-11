using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WitcherRightVersion.Core;
using WitcherRightVersion.Localization;
using WitcherRightVersion.Save;

namespace WitcherRightVersion.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject settingsPanel;
        public GameObject confirmationPanel;
        public GameObject effectsSettingsPanel;
        public GameObject soundSettingsPanel;
        public GameObject resolutionSettingsPanel;
        public GameObject graphicsSettingsPanel;

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
        public Text applyButtonText;
        public Text backButtonText;
        public Text confirmationText;
        public Text confirmButtonText;
        public Text cancelButtonText;
        public Text effectsTabText;
        public Text soundTabText;
        public Text resolutionTabText;
        public Text graphicsTabText;
        public Text sharpnessLabelText;
        public Text blurLabelText;
        public Text screenModeLabelText;

        [Header("Settings")]
        public Slider volumeSlider;
        public Toggle musicToggle;
        public Dropdown resolutionDropdown;
        public Dropdown graphicsDropdown;
        public Dropdown screenModeDropdown;
        public Toggle sharpnessToggle;
        public Toggle blurToggle;

        private const string FirstGameScene = "VelemarWorldScene";
        private ConfirmationAction pendingConfirmation;

        private enum ConfirmationAction
        {
            None,
            NewGame,
            Continue
        }

        private void Awake()
        {
            LoadSettings();
            ShowMainPanel();
            ApplyLanguage();
            SetStatus(GetText("Ready for a new contract.", "Готов к новому контракту."));
        }

        private void OnDestroy()
        {
        }

        public void OnLanguageChanged(int languageIndex)
        {
            GameLocalization.SetLanguage(GameLocalization.RussianLanguage);
            ApplyLanguage();
            SetStatus("Русский язык закреплён.");
        }

        public void StartNewGame()
        {
            ShowConfirmation(ConfirmationAction.NewGame);
        }

        public void ContinueGame()
        {
            if (!SaveService.HasAutosave())
            {
                SetStatus(GetText("No autosave yet. Start a new game first.", "Автосохранения пока нет. Сначала начни новую игру."));
                return;
            }

            ShowConfirmation(ConfirmationAction.Continue);
        }

        public void ConfirmAction()
        {
            var action = pendingConfirmation;
            HideConfirmation();

            if (action == ConfirmationAction.Continue)
            {
                SaveService.RequestAutosaveLoadOnNextScene();
            }

            if (action != ConfirmationAction.None)
            {
                SceneManager.LoadScene(FirstGameScene);
            }
        }

        public void HideConfirmation()
        {
            pendingConfirmation = ConfirmationAction.None;
            SetActive(confirmationPanel, false);
        }

        public void ShowSettingsPanel()
        {
            SetActive(mainPanel, false);
            SetActive(settingsPanel, true);
            ShowEffectsSettings();
            SetStatus(GetText("Settings opened.", "Настройки открыты."));
        }

        public void ShowMainPanel()
        {
            SetActive(mainPanel, true);
            SetActive(settingsPanel, false);
            HideConfirmation();
        }

        public void ShowEffectsSettings()
        {
            ShowSettingsPage(effectsSettingsPanel);
        }

        public void ShowSoundSettings()
        {
            ShowSettingsPage(soundSettingsPanel);
        }

        public void ShowResolutionSettings()
        {
            ShowSettingsPage(resolutionSettingsPanel);
        }

        public void ShowGraphicsSettings()
        {
            ShowSettingsPage(graphicsSettingsPanel);
        }

        public void ApplySettings()
        {
            var volume = volumeSlider != null ? volumeSlider.value : 1f;
            var musicEnabled = musicToggle == null || musicToggle.isOn;
            var resolutionIndex = resolutionDropdown != null ? resolutionDropdown.value : 0;
            var graphicsIndex = graphicsDropdown != null ? graphicsDropdown.value : 0;
            var screenModeIndex = screenModeDropdown != null ? screenModeDropdown.value : 1;
            var sharpnessEnabled = sharpnessToggle == null || sharpnessToggle.isOn;
            var blurEnabled = blurToggle != null && blurToggle.isOn;

            RuntimeSettingsService.Apply(volume, musicEnabled, resolutionIndex, graphicsIndex, screenModeIndex, sharpnessEnabled, blurEnabled);
            GameLocalization.SetLanguage(GameLocalization.RussianLanguage);

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
            var volume = PlayerPrefs.GetFloat(RuntimeSettingsService.VolumeKey, 0.8f);
            var musicEnabled = PlayerPrefs.GetInt(RuntimeSettingsService.MusicKey, 1) == 1;
            var resolutionIndex = PlayerPrefs.GetInt(RuntimeSettingsService.ResolutionKey, 2);
            var graphicsIndex = PlayerPrefs.GetInt(RuntimeSettingsService.GraphicsKey, Mathf.Min(2, QualitySettings.names.Length - 1));
            var screenModeIndex = PlayerPrefs.GetInt(RuntimeSettingsService.ScreenModeKey, 0);
            var sharpnessEnabled = PlayerPrefs.GetInt(RuntimeSettingsService.SharpnessKey, 1) == 1;
            var blurEnabled = PlayerPrefs.GetInt(RuntimeSettingsService.BlurKey, 0) == 1;

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

            if (screenModeDropdown != null)
            {
                screenModeDropdown.SetValueWithoutNotify(Mathf.Clamp(screenModeIndex, 0, screenModeDropdown.options.Count - 1));
                screenModeDropdown.RefreshShownValue();
            }

            if (sharpnessToggle != null)
            {
                sharpnessToggle.isOn = sharpnessEnabled;
            }

            if (blurToggle != null)
            {
                blurToggle.isOn = blurEnabled;
            }

            RuntimeSettingsService.Apply(volume, musicEnabled, resolutionIndex, graphicsIndex, screenModeIndex, sharpnessEnabled, blurEnabled);
        }

        private void ApplyLanguage()
        {
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
            SetText(applyButtonText, "Apply", "Применить");
            SetText(backButtonText, "Back", "Назад");
            SetText(confirmButtonText, "Confirm", "Подтвердить");
            SetText(cancelButtonText, "Cancel", "Отмена");
            SetText(effectsTabText, "Effects", "Эффекты");
            SetText(soundTabText, "Sound", "Звук");
            SetText(resolutionTabText, "Resolution", "Разрешение");
            SetText(graphicsTabText, "Graphics", "Графика");
            SetText(sharpnessLabelText, "Sharpness", "Резкость");
            SetText(blurLabelText, "Soft edges", "Смягчение");
            SetText(screenModeLabelText, "Screen mode", "Режим экрана");

            if (graphicsDropdown != null && graphicsDropdown.options.Count >= 3)
            {
                graphicsDropdown.options[0].text = "Низкое";
                graphicsDropdown.options[1].text = "Среднее";
                graphicsDropdown.options[2].text = "Высокое";
                graphicsDropdown.RefreshShownValue();
            }

            if (screenModeDropdown != null && screenModeDropdown.options.Count >= 3)
            {
                screenModeDropdown.options[0].text = "Полный экран";
                screenModeDropdown.options[1].text = "Без рамки";
                screenModeDropdown.options[2].text = "Оконный";
                screenModeDropdown.RefreshShownValue();
            }

            RefreshConfirmationText();
        }

        private void ShowConfirmation(ConfirmationAction action)
        {
            pendingConfirmation = action;
            SetActive(confirmationPanel, true);
            RefreshConfirmationText();
        }

        private void RefreshConfirmationText()
        {
            if (confirmationText == null || pendingConfirmation == ConfirmationAction.None)
            {
                return;
            }

            confirmationText.text = pendingConfirmation == ConfirmationAction.NewGame
                ? GetText("Start a new game?", "Начать новую игру?")
                : GetText("Load the latest save?", "Загрузить последнее сохранение?");
        }

        private void ShowSettingsPage(GameObject target)
        {
            SetActive(effectsSettingsPanel, target == effectsSettingsPanel);
            SetActive(soundSettingsPanel, target == soundSettingsPanel);
            SetActive(resolutionSettingsPanel, target == resolutionSettingsPanel);
            SetActive(graphicsSettingsPanel, target == graphicsSettingsPanel);
        }

        private bool IsRussian()
        {
            return true;
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
