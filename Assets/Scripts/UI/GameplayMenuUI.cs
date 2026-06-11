using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Core;
using WitcherRightVersion.Localization;
using WitcherRightVersion.Quest;

namespace WitcherRightVersion.UI
{
    public sealed class GameplayMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text mapText;
        [SerializeField] private Text characterText;
        [SerializeField] private Text strengthButtonText;
        [SerializeField] private Text resilienceButtonText;
        [SerializeField] private Text vitalityButtonText;

        public static bool IsOpen { get; private set; }

        private void Awake()
        {
            Close();
            GameLocalization.LanguageChanged += Refresh;
        }

        private void OnDestroy()
        {
            GameLocalization.LanguageChanged -= Refresh;
            IsOpen = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMap();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCharacter();
            }
            else if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }

            if (IsOpen)
            {
                Refresh();
            }
        }

        public void ToggleMap()
        {
            var shouldOpen = !IsOpen || mapPanel == null || !mapPanel.activeSelf;
            if (!shouldOpen)
            {
                Close();
                return;
            }

            Open(mapPanel);
        }

        public void ToggleCharacter()
        {
            var shouldOpen = !IsOpen || characterPanel == null || !characterPanel.activeSelf;
            if (!shouldOpen)
            {
                Close();
                return;
            }

            Open(characterPanel);
        }

        public void Close()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            IsOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UpgradeStrength()
        {
            PlayerRewardService.Instance?.TryUpgradeStrength();
            Refresh();
        }

        public void UpgradeResilience()
        {
            PlayerRewardService.Instance?.TryUpgradeResilience();
            Refresh();
        }

        public void UpgradeVitality()
        {
            PlayerRewardService.Instance?.TryUpgradeVitality();
            Refresh();
        }

        private void Open(GameObject panel)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (mapPanel != null)
            {
                mapPanel.SetActive(panel == mapPanel);
            }

            if (characterPanel != null)
            {
                characterPanel.SetActive(panel == characterPanel);
            }

            IsOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Refresh();
        }

        private void Refresh()
        {
            var russian = GameLocalization.IsRussian;
            var mapActive = mapPanel != null && mapPanel.activeSelf;
            if (titleText != null)
            {
                titleText.text = mapActive
                    ? (russian ? "Карта Велемара" : "Velemar Map")
                    : (russian ? "Характеристики Рейнарда" : "Reynard Character");
            }

            if (mapText != null)
            {
                mapText.text = BuildMapText(russian);
            }

            if (characterText != null)
            {
                characterText.text = BuildCharacterText(russian);
            }

            SetButtonText(strengthButtonText, russian ? "Улучшить силу" : "Upgrade Strength");
            SetButtonText(resilienceButtonText, russian ? "Улучшить стойкость" : "Upgrade Resilience");
            SetButtonText(vitalityButtonText, russian ? "Улучшить живучесть" : "Upgrade Vitality");
        }

        private static string BuildMapText(bool russian)
        {
            var quest = QuestService.Instance;
            var objective = quest != null && quest.HasActiveQuest
                ? GameLocalization.Text(quest.CurrentObjective)
                : (russian ? "Нет активного задания" : "No active quest");

            return russian
                ? $"                         [Руины Башни]\n" +
                  $"                                |\n" +
                  $"[Старый Лес] --- [Вересковый Брод] --- [Пепельный тракт]\n" +
                  $"                                |\n" +
                  $"                       [Чёрное Болото]\n\n" +
                  $"Текущая цель:\n{objective}\n\nM — закрыть карту   C — характеристики"
                : $"                         [Tower Ruins]\n" +
                  $"                                |\n" +
                  $"[Old Forest] --- [Heather Ford] --- [Ash Road]\n" +
                  $"                                |\n" +
                  $"                       [Black Swamp]\n\n" +
                  $"Current objective:\n{objective}\n\nM - close map   C - character";
        }

        private static string BuildCharacterText(bool russian)
        {
            var rewards = PlayerRewardService.Instance;
            if (rewards == null)
            {
                return russian ? "Данные персонажа недоступны." : "Character data unavailable.";
            }

            return russian
                ? $"Уровень: {rewards.Level}\nОпыт: {rewards.Experience} ({rewards.ExperienceIntoLevel}/{PlayerRewardService.ExperiencePerLevel})\n" +
                  $"Свободные очки: {rewards.SkillPoints}\nМонеты: {rewards.Coins}\n\n" +
                  $"Сила: {rewards.StrengthRank}  (+{(rewards.DamageMultiplier - 1f) * 100f:0}% урона)\n" +
                  $"Стойкость: {rewards.ResilienceRank}  (-{(1f - rewards.IncomingDamageMultiplier) * 100f:0}% урона)\n" +
                  $"Живучесть: {rewards.VitalityRank}  ({rewards.PlayerMaxHealth:0} здоровья)"
                : $"Level: {rewards.Level}\nXP: {rewards.Experience} ({rewards.ExperienceIntoLevel}/{PlayerRewardService.ExperiencePerLevel})\n" +
                  $"Free skill points: {rewards.SkillPoints}\nCoins: {rewards.Coins}\n\n" +
                  $"Strength: {rewards.StrengthRank}  (+{(rewards.DamageMultiplier - 1f) * 100f:0}% damage)\n" +
                  $"Resilience: {rewards.ResilienceRank}  (-{(1f - rewards.IncomingDamageMultiplier) * 100f:0}% damage)\n" +
                  $"Vitality: {rewards.VitalityRank}  ({rewards.PlayerMaxHealth:0} health)";
        }

        private static void SetButtonText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
