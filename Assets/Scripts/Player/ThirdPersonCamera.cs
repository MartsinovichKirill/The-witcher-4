using UnityEngine;
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
        [SerializeField] private float fieldOfViewSmooth = 7.5f;

        private Vector3 smoothVelocity;
        private PlayerController playerController;
        private float yaw;
        private float pitch = 15f;
        private float yawVelocity;
        private float pitchVelocity;
        private float targetYaw;
        private float targetPitch = 15f;
        private float lastManualLookTime;
        private bool cursorLocked = true;
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

            var desiredFov = playerController != null && playerController.IsRunning ? runFieldOfView : baseFieldOfView;
            attachedCamera.fieldOfView = Mathf.Lerp(attachedCamera.fieldOfView, desiredFov, fieldOfViewSmooth * Time.deltaTime);
        }

        private void RecenterBehindMovingPlayer()
        {
            if (playerController == null || !playerController.IsMoving || Time.time - lastManualLookTime < recenterDelay)
            {
                return;
            }

            targetYaw = Mathf.LerpAngle(targetYaw, target.eulerAngles.y, recenterSpeed * Time.deltaTime);
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
