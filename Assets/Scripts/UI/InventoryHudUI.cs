using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Core;
using WitcherRightVersion.Crafting;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Quest;

namespace WitcherRightVersion.UI
{
    public sealed class InventoryHudUI : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private KeyCode nextPageKey = KeyCode.Tab;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text contentText;

        private static readonly HashSet<string> ArmorItems = new HashSet<string>
        {
            "Leather Witcher Armor",
            "Reinforced Armor",
            "Swamp Cloak"
        };

        private static readonly HashSet<string> ConsumableItems = new HashSet<string>
        {
            "Swallow",
            "Antitoxin",
            "Thunder",
            "Cat",
            "Food",
            "Field Ration",
            "Ash Bomb",
            "Light Bomb",
            "Undead Oil",
            "Bog Creature Oil",
            "Hanged Man Oil"
        };

        private static readonly HashSet<string> QuestItems = new HashSet<string>
        {
            "Old Camp Blade",
            "Mirror Shard",
            "Messenger Note",
            "Ritual Key",
            "Orten Diary",
            "Girl's Medallion",
            "Elder's Seal"
        };

        private readonly StringBuilder builder = new StringBuilder(1024);
        private InventoryPage currentPage;

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
                if (Input.GetKeyDown(nextPageKey) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    CyclePage(1);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    CyclePage(-1);
                }

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

        private void CyclePage(int direction)
        {
            var pageCount = System.Enum.GetValues(typeof(InventoryPage)).Length;
            var next = ((int)currentPage + direction) % pageCount;
            if (next < 0)
            {
                next += pageCount;
            }

            currentPage = (InventoryPage)next;
        }

        private void Refresh()
        {
            if (contentText == null)
            {
                return;
            }

            builder.Clear();
            AppendHeader();

            switch (currentPage)
            {
                case InventoryPage.Overview:
                    AppendOverview();
                    break;
                case InventoryPage.Gear:
                    AppendGear();
                    break;
                case InventoryPage.Items:
                    AppendItems();
                    break;
                case InventoryPage.Crafting:
                    AppendCrafting();
                    break;
            }

            contentText.text = builder.ToString();
        }

        private void AppendHeader()
        {
            builder.Append("Inventory - ").Append(currentPage).Append(" ");
            builder.Append((int)currentPage + 1).Append("/").Append(System.Enum.GetValues(typeof(InventoryPage)).Length).AppendLine();
            builder.AppendLine("------------------------");
        }

        private void AppendOverview()
        {
            var rewards = PlayerRewardService.Instance;
            var inventory = InventoryService.Instance;
            var quest = QuestService.Instance;

            builder.Append("Level: ").Append(rewards != null ? rewards.Level : 1).Append("    ");
            builder.Append("Skill points: ").Append(rewards != null ? rewards.SkillPoints : 0).AppendLine();
            builder.Append("XP: ").Append(rewards != null ? rewards.Experience : 0);
            builder.Append(" (").Append(rewards != null ? rewards.ExperienceIntoLevel : 0).Append("/");
            builder.Append(PlayerRewardService.ExperiencePerLevel).Append(")").AppendLine();
            builder.Append("Coins: ").Append(rewards != null ? rewards.Coins : 0).AppendLine();
            builder.Append("Equipped: ").Append(inventory != null ? inventory.EquippedWeapon : "None").AppendLine();
            builder.Append("Active quest: ").Append(quest != null && quest.HasActiveQuest ? quest.ActiveQuestTitle : "None").AppendLine();
            builder.AppendLine();

            builder.Append("Weapons: ").Append(inventory != null ? inventory.Weapons.Count : 0).AppendLine();
            builder.Append("Items: ").Append(inventory != null ? inventory.Items.Count : 0).AppendLine();
            builder.Append("Known recipes: ").Append(GetKnownRecipeCount()).AppendLine();
        }

        private void AppendGear()
        {
            var inventory = InventoryService.Instance;
            builder.Append("Equipped weapon: ").Append(inventory != null ? inventory.EquippedWeapon : "None").AppendLine();
            builder.AppendLine();
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
            builder.AppendLine("Armor:");
            AppendFilteredItems(item => ArmorItems.Contains(item), "None");
        }

        private void AppendItems()
        {
            builder.AppendLine("Consumables:");
            AppendFilteredItems(item => ConsumableItems.Contains(item), "None");
            builder.AppendLine();
            builder.AppendLine("Resources:");
            AppendFilteredItems(item => !ArmorItems.Contains(item) && !ConsumableItems.Contains(item) && !QuestItems.Contains(item), "None");
            builder.AppendLine();
            builder.AppendLine("Quest items:");
            AppendFilteredItems(item => QuestItems.Contains(item), "None");
        }

        private void AppendCrafting()
        {
            var crafting = CraftingService.Instance;
            if (crafting == null || crafting.Recipes.Count == 0)
            {
                builder.AppendLine("No recipes available.");
                return;
            }

            for (var i = 0; i < crafting.Recipes.Count; i++)
            {
                var recipe = crafting.Recipes[i];
                var canCraft = crafting.CanCraft(recipe.Id, out var reason);

                builder.Append(canCraft ? "[READY] " : "[LOCKED] ");
                builder.Append(recipe.DisplayName).Append(" -> ").Append(recipe.ResultItem).AppendLine();
                builder.Append("  Needs: ");

                for (var ingredientIndex = 0; ingredientIndex < recipe.Ingredients.Length; ingredientIndex++)
                {
                    if (ingredientIndex > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(recipe.Ingredients[ingredientIndex]);
                }

                builder.AppendLine();
                if (!canCraft)
                {
                    builder.Append("  ").Append(reason).AppendLine();
                }

                if (i < crafting.Recipes.Count - 1)
                {
                    builder.AppendLine();
                }
            }
        }

        private void AppendFilteredItems(System.Func<string, bool> predicate, string emptyText)
        {
            var inventory = InventoryService.Instance;
            var count = 0;

            if (inventory == null || inventory.Items.Count == 0)
            {
                builder.Append("- ").Append(emptyText).AppendLine();
                return;
            }

            for (var i = 0; i < inventory.Items.Count; i++)
            {
                var item = inventory.Items[i];
                if (!predicate(item))
                {
                    continue;
                }

                builder.Append("- ").Append(item).AppendLine();
                count++;
            }

            if (count == 0)
            {
                builder.Append("- ").Append(emptyText).AppendLine();
            }
        }

        private int GetKnownRecipeCount()
        {
            var crafting = CraftingService.Instance;
            if (crafting == null)
            {
                return 0;
            }

            var known = 0;
            for (var i = 0; i < crafting.Recipes.Count; i++)
            {
                var recipe = crafting.Recipes[i];
                if (string.IsNullOrWhiteSpace(recipe.RequiredUnlockedRecipe)
                    || (PlayerRewardService.Instance != null && PlayerRewardService.Instance.HasRecipe(recipe.RequiredUnlockedRecipe)))
                {
                    known++;
                }
            }

            return known;
        }

        private void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private enum InventoryPage
        {
            Overview,
            Gear,
            Items,
            Crafting
        }
    }
}
