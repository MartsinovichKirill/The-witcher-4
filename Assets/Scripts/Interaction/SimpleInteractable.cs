using UnityEngine;
using UnityEngine.Events;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Interaction
{
    public sealed class SimpleInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Object";
        [SerializeField] private string interactionPrompt = "Interact";
        [SerializeField] private string interactionMessage = "Interaction complete.";
        [SerializeField] private bool canInteract = true;
        [SerializeField] private UnityEvent onInteracted;

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => canInteract;

        public void Configure(string newDisplayName, string newPrompt, string newMessage)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            interactionMessage = newMessage;
        }

        public void Interact(InteractionController interactor)
        {
            if (!canInteract)
            {
                return;
            }

            Debug.Log($"Interacted with {displayName}", this);
            onInteracted?.Invoke();

            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowMessage(interactionMessage);
            }
        }
    }
}
