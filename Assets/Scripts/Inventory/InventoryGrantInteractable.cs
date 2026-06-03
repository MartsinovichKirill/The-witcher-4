using UnityEngine;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Inventory
{
    public sealed class InventoryGrantInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Supplies";
        [SerializeField] private string interactionPrompt = "Take";
        [SerializeField] private string[] itemsToGrant;
        [SerializeField] private string message = "Supplies taken.";

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => true;

        public void Configure(string newDisplayName, string newPrompt, string[] newItemsToGrant, string newMessage)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            itemsToGrant = newItemsToGrant;
            message = newMessage;
        }

        public void Interact(InteractionController interactor)
        {
            var inventory = InventoryService.Instance;
            if (inventory == null)
            {
                return;
            }

            if (itemsToGrant == null)
            {
                return;
            }

            for (var i = 0; i < itemsToGrant.Length; i++)
            {
                inventory.AddItem(itemsToGrant[i]);
            }

            InteractionPromptUI.Instance?.ShowMessage(message);
        }
    }
}
