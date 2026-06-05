using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Localization;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Combat
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyLootDrop : MonoBehaviour
    {
        [SerializeField] private string lootFlag;
        [SerializeField] private string[] items;
        [SerializeField] private int coins;
        [SerializeField] private int experience;
        [SerializeField] private string englishMessage = "Loot collected.";
        [SerializeField] private string russianMessage = "Добыча собрана.";

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += HandleDeath;
        }

        public void Configure(string newLootFlag, string[] newItems, int newCoins, int newExperience, string newEnglishMessage, string newRussianMessage)
        {
            lootFlag = newLootFlag;
            items = newItems;
            coins = Mathf.Max(0, newCoins);
            experience = Mathf.Max(0, newExperience);
            englishMessage = newEnglishMessage;
            russianMessage = newRussianMessage;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void HandleDeath(Health deadHealth, GameObject source)
        {
            var flags = DecisionFlagService.Instance;
            if (!string.IsNullOrWhiteSpace(lootFlag) && flags != null && flags.HasFlag(lootFlag))
            {
                return;
            }

            var inventory = InventoryService.Instance;
            if (inventory != null && items != null)
            {
                for (var i = 0; i < items.Length; i++)
                {
                    inventory.AddItem(items[i]);
                }
            }

            PlayerRewardService.Instance?.AddCoins(coins);
            PlayerRewardService.Instance?.AddExperience(experience);
            if (!string.IsNullOrWhiteSpace(lootFlag))
            {
                flags?.SetFlag(lootFlag);
            }

            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select(englishMessage, russianMessage));
        }
    }
}
