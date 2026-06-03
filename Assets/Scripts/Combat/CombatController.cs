using UnityEngine;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Combat
{
    public sealed class CombatController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode lightAttackKey = KeyCode.Mouse0;

        [Header("Light Attack")]
        [SerializeField] private float lightAttackDamage = 24f;
        [SerializeField] private float lightAttackRange = 2.15f;
        [SerializeField] private float lightAttackRadius = 1.15f;
        [SerializeField] private float lightAttackCooldown = 0.65f;
        [SerializeField] private LayerMask targetMask = ~0;

        private readonly Collider[] hitResults = new Collider[12];
        private Health ownHealth;
        private float nextLightAttackTime;

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
        }

        private void Update()
        {
            if (ownHealth != null && ownHealth.IsDead)
            {
                return;
            }

            if (DialogueService.Instance != null && DialogueService.Instance.IsDialogueOpen)
            {
                return;
            }

            if (Input.GetKeyDown(lightAttackKey) && Time.time >= nextLightAttackTime)
            {
                LightAttack();
            }
        }

        private void LightAttack()
        {
            nextLightAttackTime = Time.time + lightAttackCooldown;

            var center = transform.position + Vector3.up * 1f + transform.forward * (lightAttackRange * 0.5f);
            var hitCount = Physics.OverlapSphereNonAlloc(center, lightAttackRadius, hitResults, targetMask);
            Health bestTarget = null;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = hitResults[i] == null ? null : hitResults[i].GetComponentInParent<Health>();
                if (candidate == null || candidate == ownHealth || candidate.IsDead)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance < bestDistance)
                {
                    bestTarget = candidate;
                    bestDistance = distance;
                }
            }

            if (bestTarget == null)
            {
                InteractionPromptUI.Instance?.ShowMessage("Light attack missed.");
                return;
            }

            bestTarget.TakeDamage(lightAttackDamage, gameObject);
            InteractionPromptUI.Instance?.ShowMessage($"Hit {bestTarget.DisplayName}: {lightAttackDamage:0} damage.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.15f, 0.1f, 0.35f);
            var center = transform.position + Vector3.up * 1f + transform.forward * (lightAttackRange * 0.5f);
            Gizmos.DrawWireSphere(center, lightAttackRadius);
        }
    }
}
