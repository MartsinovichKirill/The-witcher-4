using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Core;
using WitcherRightVersion.Inventory;

namespace WitcherRightVersion.UI
{
    public sealed class InventoryHudUI : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text contentText;

        private readonly StringBuilder builder = new StringBuilder(512);

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }

            if (panelRoot != null && panelRoot.activeSelf)
            {
                Refresh();
            }
        }

        private void Toggle()
        {
            if (panelRoot == null)
            {
                return;
            }

            panelRoot.SetActive(!panelRoot.activeSelf);
            if (panelRoot.activeSelf)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (contentText == null)
            {
                return;
            }

            var rewards = PlayerRewardService.Instance;
            var inventory = InventoryService.Instance;

            builder.Clear();
            builder.AppendLine("Inventory");
            builder.AppendLine();
            builder.Append("XP: ").Append(rewards != null ? rewards.Experience : 0).AppendLine();
            builder.Append("Coins: ").Append(rewards != null ? rewards.Coins : 0).AppendLine();
            builder.Append("Antitoxin recipe: ").Append(rewards != null && rewards.HasRecipe("antitoxin") ? "Unlocked" : "Locked").AppendLine();
            builder.AppendLine();
            builder.Append("Equipped: ").Append(inventory != null ? inventory.EquippedWeapon : "None").AppendLine();
            builder.AppendLine("Weapons:");

            if (inventory == null || inventory.Weapons.Count == 0)
            {
                builder.AppendLine("- None");
            }
            else
            {
                for (var i = 0; i < inventory.Weapons.Count; i++)
                {
                    builder.Append("- ").Append(inventory.Weapons[i]).AppendLine();
                }
            }

            builder.AppendLine();
            builder.AppendLine("Items:");
            if (inventory == null || inventory.Items.Count == 0)
            {
                builder.AppendLine("- None");
            }
            else
            {
                for (var i = 0; i < inventory.Items.Count; i++)
                {
                    builder.Append("- ").Append(inventory.Items[i]).AppendLine();
                }
            }

            contentText.text = builder.ToString();
        }

        private void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
    }
}
