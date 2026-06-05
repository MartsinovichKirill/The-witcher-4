using UnityEngine;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Combat
{
    public sealed class CombatController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode lightAttackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode heavyAttackKey = KeyCode.F;
        [SerializeField] private KeyCode blockKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode dodgeKey = KeyCode.Space;
        [SerializeField] private KeyCode aardKey = KeyCode.Q;

        [Header("Attacks")]
        [SerializeField] private float lightAttackDamage = 24f;
        [SerializeField] private float lightAttackRange = 2.45f;
        [SerializeField] private float lightAttackRadius = 1.25f;
        [SerializeField] private float lightAttackCooldown = 0.65f;
        [SerializeField] private float heavyAttackDamage = 42f;
        [SerializeField] private float heavyAttackRange = 2.35f;
        [SerializeField] private float heavyAttackRadius = 1.2f;
        [SerializeField] private float heavyAttackCooldown = 1.35f;
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Block")]
        [SerializeField] private float blockDamageMultiplier = 0.45f;

        [Header("Dodge")]
        [SerializeField] private float dodgeDistance = 2.35f;
        [SerializeField] private float dodgeDuration = 0.18f;
        [SerializeField] private float dodgeCooldown = 1.2f;
        [SerializeField] private Transform cameraTransform;

        [Header("Aard")]
        [SerializeField] private float aardDamage = 8f;
        [SerializeField] private float aardRange = 3.65f;
        [SerializeField] private float aardRadius = 1.55f;
        [SerializeField] private float aardCooldown = 4f;
        [SerializeField] private float aardKnockbackDistance = 1.4f;
        [SerializeField] private float aardStunDuration = 0.8f;

        private readonly Collider[] hitResults = new Collider[12];
        private CharacterController characterController;
        private Health ownHealth;
        private float nextLightAttackTime;
        private float nextHeavyAttackTime;
        private float nextDodgeTime;
        private float dodgeEndTime;
        private float nextAardTime;
        private Vector3 dodgeVelocity;

        public bool IsBlocking { get; private set; }
        public bool IsDodging { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            ownHealth = GetComponent<Health>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (ownHealth != null && ownHealth.IsDead)
            {
                IsBlocking = false;
                return;
            }

            if (DialogueService.Instance != null && DialogueService.Instance.IsDialogueOpen)
            {
                IsBlocking = false;
                return;
            }

            if (IsDodging)
            {
                ContinueDodge();
                return;
            }

            IsBlocking = Input.GetKey(blockKey);

            if (Input.GetKeyDown(dodgeKey) && Time.time >= nextDodgeTime)
            {
                StartDodge();
                return;
            }

            if (IsBlocking)
            {
                return;
            }

            if (Input.GetKeyDown(lightAttackKey) && Time.time >= nextLightAttackTime)
            {
                Attack(lightAttackDamage, lightAttackRange, lightAttackRadius, "Light attack missed.");
                nextLightAttackTime = Time.time + lightAttackCooldown;
                return;
            }

            if (Input.GetKeyDown(heavyAttackKey) && Time.time >= nextHeavyAttackTime)
            {
                Attack(heavyAttackDamage, heavyAttackRange, heavyAttackRadius, "Heavy attack missed.");
                nextHeavyAttackTime = Time.time + heavyAttackCooldown;
                return;
            }

            if (Input.GetKeyDown(aardKey) && Time.time >= nextAardTime)
            {
                CastAard();
                nextAardTime = Time.time + aardCooldown;
            }
        }

        public void ReceiveEnemyAttack(float amount, GameObject source)
        {
            if (ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            var finalDamage = IsBlocking ? amount * blockDamageMultiplier : amount;
            ownHealth.TakeDamage(finalDamage, source);

            var message = IsBlocking
                ? $"Blocked hit: {finalDamage:0} damage taken."
                : $"Reynard took {finalDamage:0} damage.";
            InteractionPromptUI.Instance?.ShowMessage(message);
        }

        private void Attack(float damage, float range, float radius, string missMessage)
        {
            var bestTarget = FindBestTarget(range, radius);

            if (bestTarget == null)
            {
                InteractionPromptUI.Instance?.ShowMessage(missMessage);
                return;
            }

            bestTarget.TakeDamage(damage, gameObject);
            InteractionPromptUI.Instance?.ShowMessage($"Hit {bestTarget.DisplayName}: {damage:0} damage.");
        }

        private Health FindBestTarget(float range, float radius)
        {
            var center = transform.position + Vector3.up * 1f + transform.forward * (range * 0.5f);
            var hitCount = Physics.OverlapSphereNonAlloc(center, radius, hitResults, targetMask);
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

            return bestTarget;
        }

        private void StartDodge()
        {
            if (characterController == null)
            {
                return;
            }

            var direction = GetDodgeDirection();
            dodgeVelocity = direction * (dodgeDistance / Mathf.Max(0.01f, dodgeDuration));
            dodgeEndTime = Time.time + dodgeDuration;
            nextDodgeTime = Time.time + dodgeCooldown;
            IsBlocking = false;
            IsDodging = true;
            InteractionPromptUI.Instance?.ShowMessage("Dodge.");
        }

        private void ContinueDodge()
        {
            if (characterController != null)
            {
                characterController.Move(dodgeVelocity * Time.deltaTime);
            }

            if (Time.time >= dodgeEndTime)
            {
                IsDodging = false;
                dodgeVelocity = Vector3.zero;
            }
        }

        private Vector3 GetDodgeDirection()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            if (input.sqrMagnitude <= 0.01f)
            {
                return -transform.forward;
            }

            if (cameraTransform == null)
            {
                return input.normalized;
            }

            var forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return (forward * input.z + right * input.x).normalized;
        }

        private void CastAard()
        {
            var center = transform.position + Vector3.up * 1f + transform.forward * (aardRange * 0.5f);
            var hitCount = Physics.OverlapSphereNonAlloc(center, aardRadius, hitResults, targetMask);
            var affected = 0;

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = hitResults[i] == null ? null : hitResults[i].GetComponentInParent<Health>();
                if (candidate == null || candidate == ownHealth || candidate.IsDead)
                {
                    continue;
                }

                var offset = candidate.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.01f && Vector3.Dot(transform.forward, offset.normalized) < 0.25f)
                {
                    continue;
                }

                candidate.TakeDamage(aardDamage, gameObject);

                var enemy = candidate.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.ApplyAard(offset.sqrMagnitude > 0.01f ? offset.normalized : transform.forward, aardKnockbackDistance, aardStunDuration);
                }

                affected++;
            }

            InteractionPromptUI.Instance?.ShowMessage(affected > 0 ? $"Aard hit {affected} target(s)." : "Aard hit nothing.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.15f, 0.1f, 0.35f);
            var center = transform.position + Vector3.up * 1f + transform.forward * (lightAttackRange * 0.5f);
            Gizmos.DrawWireSphere(center, lightAttackRadius);

            Gizmos.color = new Color(0.25f, 0.55f, 1f, 0.35f);
            var aardCenter = transform.position + Vector3.up * 1f + transform.forward * (aardRange * 0.5f);
            Gizmos.DrawWireSphere(aardCenter, aardRadius);
        }
    }
}
