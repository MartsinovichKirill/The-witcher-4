namespace WitcherRightVersion.Interaction
{
    public interface IInteractable
    {
        string DisplayName { get; }
        string InteractionPrompt { get; }
        bool CanInteract { get; }
        void Interact(InteractionController interactor);
    }
}
