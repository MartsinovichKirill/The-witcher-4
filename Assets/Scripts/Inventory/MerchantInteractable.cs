using UnityEngine;
using WitcherRightVersion.Core;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Inventory
{
    public sealed class MerchantInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Merchant";
        [SerializeField] private string interactionPrompt = "Buy";
        [SerializeField] private int cost = 10;
        [SerializeField] private string[] itemsToSell;
        [SerializeField] private string purchaseFlag = "merchantPackBought";
        [SerializeField] private string successMessage = "Supplies bought.";
        [SerializeField] private string notEnoughCoinsMessage = "Not enough coins.";
        [SerializeField] private string alreadyBoughtMessage = "No more supplies for now.";

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => DecisionFlagService.Instance == null || !DecisionFlagService.Instance.HasFlag(purchaseFlag);

        public void Configure(
            string newDisplayName,
            string newPrompt,
            int newCost,
            string[] newItemsToSell,
            string newPurchaseFlag,
            string newSuccessMessage,
            string newNotEnoughCoinsMessage,
            string newAlreadyBoughtMessage)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            cost = Mathf.Max(0, newCost);
            itemsToSell = newItemsToSell;
            purchaseFlag = newPurchaseFlag;
            successMessage = newSuccessMessage;
            notEnoughCoinsMessage = newNotEnoughCoinsMessage;
            alreadyBoughtMessage = newAlreadyBoughtMessage;
        }

        public void Interact(InteractionController interactor)
        {
            if (!CanInteract)
            {
                InteractionPromptUI.Instance?.ShowMessage(alreadyBoughtMessage);
                return;
            }

            var rewards = PlayerRewardService.Instance;
            var inventory = InventoryService.Instance;
            if (rewards == null || inventory == null)
            {
                return;
            }

            if (!rewards.TrySpendCoins(cost))
            {
                InteractionPromptUI.Instance?.ShowMessage(notEnoughCoinsMessage);
                return;
            }

            if (itemsToSell != null)
            {
                for (var i = 0; i < itemsToSell.Length; i++)
                {
                    inventory.AddItem(itemsToSell[i]);
                }
            }

            DecisionFlagService.Instance?.SetFlag(purchaseFlag);
            InteractionPromptUI.Instance?.ShowMessage(successMessage);
        }
    }
}
