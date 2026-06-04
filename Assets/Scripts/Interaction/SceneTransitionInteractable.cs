using UnityEngine;
using UnityEngine.SceneManagement;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Interaction
{
    public sealed class SceneTransitionInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Path";
        [SerializeField] private string interactionPrompt = "Travel";
        [SerializeField] private string targetSceneName = "VillageScene";

        public string DisplayName => displayName;
        public string InteractionPrompt => interactionPrompt;
        public bool CanInteract => !string.IsNullOrWhiteSpace(targetSceneName);

        public void Configure(string newDisplayName, string newPrompt, string newTargetSceneName)
        {
            displayName = newDisplayName;
            interactionPrompt = newPrompt;
            targetSceneName = newTargetSceneName;
        }

        public void Interact(InteractionController interactor)
        {
            if (!CanInteract)
            {
                return;
            }

            InteractionPromptUI.Instance?.ShowMessage($"Traveling to {targetSceneName}.");
            SaveService.PrepareSceneTransfer();
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
