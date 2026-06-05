using UnityEngine;
using UnityEngine.SceneManagement;
using WitcherRightVersion.Localization;
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

            InteractionPromptUI.Instance?.ShowMessage(GameLocalization.Select($"Traveling to {targetSceneName}.", $"Переход: {GetLocalizedSceneName(targetSceneName)}."));
            SaveService.PrepareSceneTransfer();
            SceneManager.LoadScene(targetSceneName);
        }

        private static string GetLocalizedSceneName(string sceneName)
        {
            switch (sceneName)
            {
                case "VillageScene":
                    return "Вересковый Брод";
                case "ForestScene":
                    return "Старый Лес";
                case "AshRoadScene":
                    return "Пепельный тракт";
                case "VelemarWorldScene":
                    return "Велемар";
                default:
                    return sceneName;
            }
        }
    }
}
