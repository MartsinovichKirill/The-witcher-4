using UnityEngine;
using WitcherRightVersion.Combat;

namespace WitcherRightVersion.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(CombatController))]
    public sealed class PlayerActionVisualAnimator : MonoBehaviour
    {
        private enum ActionPose
        {
            None,
            LightAttack,
            HeavyAttack,
            Dodge,
            Aard,
            Igni,
            Hit
        }

        [Header("Visual References")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform steelSword;
        [SerializeField] private Transform silverSword;

        [Header("Locomotion")]
        [SerializeField] private float walkBobHeight = 0.035f;
        [SerializeField] private float runBobHeight = 0.06f;
        [SerializeField] private float walkBobSpeed = 7f;
        [SerializeField] private float runBobSpeed = 11f;
        [SerializeField] private float movementLean = 5f;
        [SerializeField] private float poseSmooth = 14f;

        [Header("Combat Poses")]
        [SerializeField] private float lightAttackDuration = 0.42f;
        [SerializeField] private float heavyAttackDuration = 0.72f;
        [SerializeField] private float dodgePoseDuration = 0.3f;
        [SerializeField] private float aardPoseDuration = 0.55f;
        [SerializeField] private float igniPoseDuration = 0.58f;
        [SerializeField] private float hitPoseDuration = 0.24f;

        private PlayerController movement;
        private CombatController combat;
        private Health health;
        private Vector3 visualBasePosition;
        private Quaternion visualBaseRotation;
        private Vector3 steelRestPosition;
        private Quaternion steelRestRotation;
        private Vector3 silverRestPosition;
        private Quaternion silverRestRotation;
        private ActionPose currentPose;
        private float poseStartedAt;
        private float poseDuration;

        private void Awake()
        {
            movement = GetComponent<PlayerController>();
            combat = GetComponent<CombatController>();
            health = GetComponent<Health>();

            ResolveReferences();
            CaptureRestPose();
        }

        private void OnEnable()
        {
            combat.LightAttackStarted += HandleLightAttack;
            combat.HeavyAttackStarted += HandleHeavyAttack;
            combat.DodgeStarted += HandleDodge;
            combat.AardStarted += HandleAard;
            combat.IgniStarted += HandleIgni;

            if (health != null)
            {
                health.Damaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (combat != null)
            {
                combat.LightAttackStarted -= HandleLightAttack;
                combat.HeavyAttackStarted -= HandleHeavyAttack;
                combat.DodgeStarted -= HandleDodge;
                combat.AardStarted -= HandleAard;
                combat.IgniStarted -= HandleIgni;
            }

            if (health != null)
            {
                health.Damaged -= HandleDamaged;
            }
        }

        private void LateUpdate()
        {
            if (visualRoot == null)
            {
                return;
            }

            var desiredPosition = visualBasePosition;
            var desiredRotation = visualBaseRotation;
            var steelPosition = steelRestPosition;
            var steelRotation = steelRestRotation;
            var silverPosition = silverRestPosition;
            var silverRotation = silverRestRotation;

            ApplyLocomotion(ref desiredPosition, ref desiredRotation);
            ApplyCombatPose(ref desiredPosition, ref desiredRotation, ref steelPosition, ref steelRotation);

            var blend = 1f - Mathf.Exp(-poseSmooth * Time.deltaTime);
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, desiredPosition, blend);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, desiredRotation, blend);

            if (steelSword != null)
            {
                steelSword.localPosition = Vector3.Lerp(steelSword.localPosition, steelPosition, blend);
                steelSword.localRotation = Quaternion.Slerp(steelSword.localRotation, steelRotation, blend);
            }

            if (silverSword != null)
            {
                silverSword.localPosition = Vector3.Lerp(silverSword.localPosition, silverPosition, blend);
                silverSword.localRotation = Quaternion.Slerp(silverSword.localRotation, silverRotation, blend);
            }
        }

        public void Configure(Transform newVisualRoot, Transform newSteelSword, Transform newSilverSword)
        {
            visualRoot = newVisualRoot;
            steelSword = newSteelSword;
            silverSword = newSilverSword;
            CaptureRestPose();
        }

        private void ResolveReferences()
        {
            visualRoot ??= transform.Find("ReynardKnightModel");
            steelSword ??= transform.Find("ReynardSteelSword_Visual");
            silverSword ??= transform.Find("ReynardSilverSword_Visual");
        }

        private void CaptureRestPose()
        {
            if (visualRoot != null)
            {
                visualBasePosition = visualRoot.localPosition;
                visualBaseRotation = visualRoot.localRotation;
            }

            if (steelSword != null)
            {
                steelRestPosition = steelSword.localPosition;
                steelRestRotation = steelSword.localRotation;
            }

            if (silverSword != null)
            {
                silverRestPosition = silverSword.localPosition;
                silverRestRotation = silverSword.localRotation;
            }
        }

        private void ApplyLocomotion(ref Vector3 position, ref Quaternion rotation)
        {
            if (movement == null || !movement.IsMoving || currentPose != ActionPose.None)
            {
                return;
            }

            var running = movement.IsRunning;
            var speed = running ? runBobSpeed : walkBobSpeed;
            var height = running ? runBobHeight : walkBobHeight;
            var cycle = Time.time * speed;
            position.y += Mathf.Abs(Mathf.Sin(cycle)) * height;

            var localDirection = transform.InverseTransformDirection(movement.MoveDirection);
            var pitch = Mathf.Sin(cycle * 0.5f) * (running ? 2.2f : 1.2f);
            var roll = -localDirection.x * movementLean;
            rotation *= Quaternion.Euler(pitch, 0f, roll);
        }

        private void ApplyCombatPose(
            ref Vector3 position,
            ref Quaternion rotation,
            ref Vector3 steelPosition,
            ref Quaternion steelRotation)
        {
            if (combat != null && combat.IsBlocking && currentPose == ActionPose.None)
            {
                position.z -= 0.05f;
                rotation *= Quaternion.Euler(-8f, -18f, 5f);
                SetSwordCombatPose(ref steelPosition, ref steelRotation, 0.68f);
                return;
            }

            if (currentPose == ActionPose.None)
            {
                return;
            }

            var elapsed = Time.time - poseStartedAt;
            var normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, poseDuration));
            var arc = Mathf.Sin(normalized * Mathf.PI);

            switch (currentPose)
            {
                case ActionPose.LightAttack:
                    rotation *= Quaternion.Euler(arc * 5f, Mathf.Lerp(-42f, 68f, normalized), -arc * 12f);
                    position.z += arc * 0.13f;
                    SetSwordCombatPose(ref steelPosition, ref steelRotation, normalized);
                    break;
                case ActionPose.HeavyAttack:
                    rotation *= Quaternion.Euler(-arc * 18f, Mathf.Lerp(-58f, 92f, normalized), -arc * 18f);
                    position.y -= arc * 0.08f;
                    position.z += arc * 0.18f;
                    SetSwordCombatPose(ref steelPosition, ref steelRotation, normalized);
                    break;
                case ActionPose.Dodge:
                    rotation *= Quaternion.Euler(arc * 18f, 0f, -arc * 24f);
                    position.y -= arc * 0.16f;
                    break;
                case ActionPose.Aard:
                    rotation *= Quaternion.Euler(-arc * 11f, 0f, arc * 7f);
                    position.z += arc * 0.08f;
                    break;
                case ActionPose.Igni:
                    rotation *= Quaternion.Euler(-arc * 8f, arc * 6f, -arc * 12f);
                    position.z += arc * 0.1f;
                    break;
                case ActionPose.Hit:
                    rotation *= Quaternion.Euler(arc * 12f, -arc * 18f, arc * 10f);
                    position.z -= arc * 0.1f;
                    break;
            }

            if (normalized >= 1f)
            {
                currentPose = ActionPose.None;
            }
        }

        private static void SetSwordCombatPose(ref Vector3 position, ref Quaternion rotation, float progress)
        {
            var ready = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 2.5f));
            position = Vector3.Lerp(position, new Vector3(0.5f, 1.06f, 0.38f), ready);
            rotation = Quaternion.Slerp(rotation, Quaternion.Euler(8f, 8f, 112f), ready);
        }

        private void BeginPose(ActionPose pose, float duration)
        {
            currentPose = pose;
            poseStartedAt = Time.time;
            poseDuration = duration;
        }

        private void HandleLightAttack()
        {
            BeginPose(ActionPose.LightAttack, lightAttackDuration);
        }

        private void HandleHeavyAttack()
        {
            BeginPose(ActionPose.HeavyAttack, heavyAttackDuration);
        }

        private void HandleDodge()
        {
            BeginPose(ActionPose.Dodge, dodgePoseDuration);
        }

        private void HandleAard()
        {
            BeginPose(ActionPose.Aard, aardPoseDuration);
        }

        private void HandleIgni()
        {
            BeginPose(ActionPose.Igni, igniPoseDuration);
        }

        private void HandleDamaged(Health damagedHealth, float amount, GameObject source)
        {
            BeginPose(ActionPose.Hit, hitPoseDuration);
        }
    }
}
