using UnityEngine;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Interaction
{
    public sealed class InteractionController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Search")]
        [SerializeField] private float interactionRadius = 2.2f;
        [SerializeField] private LayerMask interactionMask = ~0;

        private readonly Collider[] overlapResults = new Collider[16];
        private IInteractable currentInteractable;

        public IInteractable CurrentInteractable => currentInteractable;

        private void Update()
        {
            currentInteractable = FindBestInteractable();
            UpdatePrompt();

            if (currentInteractable != null && Input.GetKeyDown(interactKey))
            {
                currentInteractable.Interact(this);
            }
        }

        private IInteractable FindBestInteractable()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, overlapResults, interactionMask);
            IInteractable best = null;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = GetInteractable(overlapResults[i]);
                if (candidate == null || !candidate.CanInteract)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, overlapResults[i].ClosestPoint(transform.position));
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static IInteractable GetInteractable(Collider candidate)
        {
            if (candidate == null)
            {
                return null;
            }

            var behaviours = candidate.GetComponentsInParent<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IInteractable interactable)
                {
                    return interactable;
                }
            }

            return null;
        }

        private void UpdatePrompt()
        {
            if (InteractionPromptUI.Instance == null)
            {
                return;
            }

            if (currentInteractable == null)
            {
                InteractionPromptUI.Instance.HidePrompt();
                return;
            }

            InteractionPromptUI.Instance.ShowPrompt(currentInteractable.DisplayName, currentInteractable.InteractionPrompt, interactKey);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.95f, 0.75f, 0.25f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
