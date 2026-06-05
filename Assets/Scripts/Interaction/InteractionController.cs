using UnityEngine;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Interaction
{
    public sealed class InteractionController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Search")]
        [SerializeField] private float interactionRadius = 2.8f;
        [SerializeField] private float facingPreferenceWeight = 0.85f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private Transform cameraTransform;

        private readonly Collider[] overlapResults = new Collider[16];
        private IInteractable currentInteractable;

        public IInteractable CurrentInteractable => currentInteractable;

        private void Awake()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (DialogueService.Instance != null && DialogueService.Instance.IsDialogueOpen)
            {
                currentInteractable = null;
                InteractionPromptUI.Instance?.HidePrompt();
                return;
            }

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
            var bestScore = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = GetInteractable(overlapResults[i]);
                if (candidate == null || !candidate.CanInteract)
                {
                    continue;
                }

                var closestPoint = overlapResults[i].ClosestPoint(transform.position);
                var distance = Vector3.Distance(transform.position, closestPoint);
                var score = distance - GetFacingBonus(closestPoint);
                if (score < bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            return best;
        }

        private float GetFacingBonus(Vector3 worldPoint)
        {
            var referenceForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            referenceForward.y = 0f;
            if (referenceForward.sqrMagnitude <= 0.01f)
            {
                return 0f;
            }

            referenceForward.Normalize();

            var toPoint = worldPoint - transform.position;
            toPoint.y = 0f;
            if (toPoint.sqrMagnitude <= 0.01f)
            {
                return facingPreferenceWeight;
            }

            var facingDot = Vector3.Dot(referenceForward, toPoint.normalized);
            return Mathf.Clamp01(facingDot) * facingPreferenceWeight;
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
