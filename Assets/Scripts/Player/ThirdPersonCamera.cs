using UnityEngine;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Player
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.48f, 0f);
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.58f, 0.03f, 0f);

        [Header("Orbit")]
        [SerializeField] private float distance = 4.85f;
        [SerializeField] private float minDistance = 2.85f;
        [SerializeField] private float maxDistance = 6.4f;
        [SerializeField] private float zoomSensitivity = 1.1f;
        [SerializeField] private float minPitch = -18f;
        [SerializeField] private float maxPitch = 54f;
        [SerializeField] private float mouseSensitivity = 3.1f;
        [SerializeField] private float followSmoothTime = 0.055f;
        [SerializeField] private float rotationSmoothTime = 0.018f;
        [SerializeField] private float collisionRadius = 0.28f;
        [SerializeField] private float recenterDelay = 1.35f;
        [SerializeField] private float recenterSpeed = 1.85f;
        [SerializeField] private LayerMask collisionMask = ~0;

        [Header("Feel")]
        [SerializeField] private float baseFieldOfView = 62f;
        [SerializeField] private float runFieldOfView = 67f;
        [SerializeField] private float combatFieldOfView = 59f;
        [SerializeField] private float fieldOfViewSmooth = 7.5f;

        [Header("Combat Framing")]
        [SerializeField] private KeyCode lockTargetKey = KeyCode.Tab;
        [SerializeField] private float combatAwarenessRange = 9f;
        [SerializeField] private float lockOnRange = 14f;
        [SerializeField] private float softCombatRecenterSpeed = 1.25f;
        [SerializeField] private float lockOnRecenterSpeed = 5.2f;
        [SerializeField] private LayerMask combatTargetMask = ~0;

        private Vector3 smoothVelocity;
        private readonly Collider[] combatTargets = new Collider[18];
        private PlayerController playerController;
        private Health ownHealth;
        private Health cameraCombatTarget;
        private float yaw;
        private float pitch = 15f;
        private float yawVelocity;
        private float pitchVelocity;
        private float targetYaw;
        private float targetPitch = 15f;
        private float lastManualLookTime;
        private bool cursorLocked = true;
        private bool isTargetLocked;
        private Camera attachedCamera;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null)
            {
                yaw = target.eulerAngles.y;
                targetYaw = yaw;
                playerController = target.GetComponent<PlayerController>();
                ownHealth = target.GetComponent<Health>();
            }

            attachedCamera = GetComponent<Camera>();
            if (attachedCamera != null)
            {
                attachedCamera.fieldOfView = baseFieldOfView;
            }

            SetCursorLocked(true);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            UpdateCursorLock();

            if (cursorLocked && !IsCameraInputBlocked())
            {
                var mouseX = Input.GetAxis("Mouse X");
                var mouseY = Input.GetAxis("Mouse Y");
                if (Mathf.Abs(mouseX) > 0.001f || Mathf.Abs(mouseY) > 0.001f)
                {
                    targetYaw += mouseX * mouseSensitivity;
                    targetPitch -= mouseY * mouseSensitivity;
                    targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
                    lastManualLookTime = Time.time;
                }
            }

            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                distance = Mathf.Clamp(distance - scroll * zoomSensitivity, minDistance, maxDistance);
            }

            UpdateCombatTargeting();
            RecenterBehindMovingPlayer();
            yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);
            pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var targetPosition = target.position + targetOffset + rotation * shoulderOffset;
            var desiredDistance = GetCollisionAdjustedDistance(targetPosition, rotation);
            var desiredPosition = targetPosition - rotation * Vector3.forward * desiredDistance;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, followSmoothTime);
            transform.rotation = rotation;
            UpdateFieldOfView();
        }

        private void UpdateCursorLock()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLocked(false);
            }
            else if (!IsCameraInputBlocked() && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                SetCursorLocked(true);
            }
        }

        private void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private bool IsCameraInputBlocked()
        {
            return (DialogueService.Instance != null && DialogueService.Instance.IsDialogueOpen) || InventoryHudUI.IsOpen;
        }

        private void UpdateFieldOfView()
        {
            if (attachedCamera == null)
            {
                return;
            }

            var desiredFov = cameraCombatTarget != null ? combatFieldOfView : playerController != null && playerController.IsRunning ? runFieldOfView : baseFieldOfView;
            attachedCamera.fieldOfView = Mathf.Lerp(attachedCamera.fieldOfView, desiredFov, fieldOfViewSmooth * Time.deltaTime);
        }

        private void RecenterBehindMovingPlayer()
        {
            if (cameraCombatTarget != null || playerController == null || !playerController.IsMoving || Time.time - lastManualLookTime < recenterDelay)
            {
                return;
            }

            targetYaw = Mathf.LerpAngle(targetYaw, target.eulerAngles.y, recenterSpeed * Time.deltaTime);
        }

        private void UpdateCombatTargeting()
        {
            if (IsCameraInputBlocked())
            {
                cameraCombatTarget = null;
                isTargetLocked = false;
                return;
            }

            if (Input.GetKeyDown(lockTargetKey))
            {
                if (isTargetLocked)
                {
                    isTargetLocked = false;
                    cameraCombatTarget = null;
                    InteractionPromptUI.Instance?.ShowMessage("Target lock released.");
                }
                else
                {
                    cameraCombatTarget = FindBestCombatTarget(lockOnRange);
                    isTargetLocked = cameraCombatTarget != null;
                    if (isTargetLocked)
                    {
                        InteractionPromptUI.Instance?.ShowMessage($"Locked: {cameraCombatTarget.DisplayName}.");
                    }
                    else
                    {
                        InteractionPromptUI.Instance?.ShowMessage("No target in range.");
                    }
                }
            }

            if (cameraCombatTarget == null || cameraCombatTarget.IsDead || Vector3.Distance(target.position, cameraCombatTarget.transform.position) > lockOnRange + 2f)
            {
                isTargetLocked = false;
                cameraCombatTarget = FindBestCombatTarget(combatAwarenessRange);
            }

            if (cameraCombatTarget == null)
            {
                return;
            }

            var toEnemy = cameraCombatTarget.transform.position - target.position;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude <= 0.01f)
            {
                return;
            }

            var desiredYaw = Mathf.Atan2(toEnemy.x, toEnemy.z) * Mathf.Rad2Deg;
            var strength = isTargetLocked ? lockOnRecenterSpeed : softCombatRecenterSpeed;
            if (isTargetLocked || Time.time - lastManualLookTime >= recenterDelay * 0.6f)
            {
                targetYaw = Mathf.LerpAngle(targetYaw, desiredYaw, strength * Time.deltaTime);
                targetPitch = Mathf.Lerp(targetPitch, 11f, strength * 0.5f * Time.deltaTime);
            }
        }

        private Health FindBestCombatTarget(float range)
        {
            if (target == null)
            {
                return null;
            }

            var hitCount = Physics.OverlapSphereNonAlloc(target.position + Vector3.up, range, combatTargets, combatTargetMask, QueryTriggerInteraction.Ignore);
            Health best = null;
            var bestScore = float.MaxValue;
            var cameraForward = transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            for (var i = 0; i < hitCount; i++)
            {
                var candidate = combatTargets[i] == null ? null : combatTargets[i].GetComponentInParent<Health>();
                if (candidate == null || candidate == ownHealth || candidate.IsDead)
                {
                    continue;
                }

                var toTarget = candidate.transform.position - target.position;
                toTarget.y = 0f;
                var distance = toTarget.magnitude;
                if (distance <= 0.01f)
                {
                    continue;
                }

                var viewBonus = Mathf.Clamp01(Vector3.Dot(cameraForward, toTarget.normalized));
                var score = distance - viewBonus * 3.5f;
                if (score < bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            return best;
        }

        private float GetCollisionAdjustedDistance(Vector3 targetPosition, Quaternion rotation)
        {
            var direction = -(rotation * Vector3.forward);
            if (Physics.SphereCast(targetPosition, collisionRadius, direction, out var hit, distance, collisionMask, QueryTriggerInteraction.Ignore)
                && target != null
                && !hit.transform.IsChildOf(target))
            {
                return Mathf.Clamp(hit.distance - collisionRadius, minDistance * 0.45f, distance);
            }

            return distance;
        }
    }
}
