using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Localization;
using WitcherRightVersion.Player;
using WitcherRightVersion.Save;

namespace WitcherRightVersion.UI
{
    public sealed class PlayerDeathUI : MonoBehaviour
    {
        private const string MainMenuSceneName = "MainMenuScene";

        [SerializeField] private GameObject root;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text retryButtonText;
        [SerializeField] private Text menuButtonText;

        private Health playerHealth;

        private void Start()
        {
            root?.SetActive(false);
            var player = GameObject.FindGameObjectWithTag("Player");
            playerHealth = player != null ? player.GetComponent<Health>() : null;
            if (playerHealth != null)
            {
                playerHealth.Died += HandlePlayerDied;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= HandlePlayerDied;
            }
        }

        public void Configure(GameObject newRoot, Text newTitle, Text newDescription, Text newRetryButtonText, Text newMenuButtonText)
        {
            root = newRoot;
            titleText = newTitle;
            descriptionText = newDescription;
            retryButtonText = newRetryButtonText;
            menuButtonText = newMenuButtonText;
            RefreshLanguage();
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            if (SaveService.HasAutosave())
            {
                SaveService.RequestAutosaveLoadOnNextScene();
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private void HandlePlayerDied(Health deadHealth, GameObject source)
        {
            var player = deadHealth != null ? deadHealth.gameObject : null;
            if (player != null)
            {
                var movement = player.GetComponent<PlayerController>();
                var combat = player.GetComponent<CombatController>();
                if (movement != null)
                {
                    movement.enabled = false;
                }

                if (combat != null)
                {
                    combat.enabled = false;
                }
            }

            RefreshLanguage();
            root?.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void RefreshLanguage()
        {
            if (titleText != null)
            {
                titleText.text = GameLocalization.Select("Reynard has fallen", "Рейнард погиб");
            }

            if (descriptionText != null)
            {
                descriptionText.text = GameLocalization.Select(
                    "Load the latest autosave or restart the current area.",
                    "Загрузить последний автосейв или начать текущую область заново.");
            }

            if (retryButtonText != null)
            {
                retryButtonText.text = GameLocalization.Select("Retry", "Продолжить");
            }

            if (menuButtonText != null)
            {
                menuButtonText.text = GameLocalization.Select("Main Menu", "Главное меню");
            }
        }
    }
}
