using UnityEngine;
using WitcherRightVersion.Interaction;

namespace WitcherRightVersion.Dialogue
{
    public sealed class DialogueInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Speaker";
        [SerializeField] private string interactionPrompt = "Talk";
        [SerializeField] private string startNodeId = "start";
        [SerializeField] private DialogueNode[] dialogueNodes;

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => DialogueService.Instance != null && !DialogueService.Instance.IsDialogueOpen;

        public void Configure(string newDisplayName, string newPrompt, string newStartNodeId, DialogueNode[] newDialogueNodes)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            startNodeId = newStartNodeId;
            dialogueNodes = newDialogueNodes;
        }

        public void Interact(InteractionController interactor)
        {
            if (DialogueService.Instance == null)
            {
                Debug.LogWarning("DialogueService is missing from the scene.", this);
                return;
            }

            DialogueService.Instance.StartDialogue(dialogueNodes, startNodeId);
        }
    }
}
