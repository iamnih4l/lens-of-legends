using UnityEngine;
using LensOfLegends.Core;
using LensOfLegends.Photography;

namespace LensOfLegends.Player
{
    /// <summary>
    /// First-person exploration controller with WASD movement, mouse look, collision detection,
    /// subtle walking camera cadence, and environmental footstep audio.
    /// Automatically pauses movement when a cinematic is active or a modal UI panel is open.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonExplorerController : MonoBehaviour
    {
        public static FirstPersonExplorerController Instance { get; private set; }

        [Header("Movement & Look Settings")]
        public float walkSpeed = 4.4f;
        public float cameraModeWalkSpeed = 2.1f;
        public float sprintSpeed = 6.8f;
        public float gravity = -15.0f;
        public float mouseSensitivity = 2.0f;
        public float maxLookPitchDegrees = 80f;

        private CharacterController _controller;
        private Camera _playerCamera;
        private float _cameraPitch = 0f;
        private float _verticalVelocity = 0f;
        private float _stepCycleTimer = 0f;
        private Vector3 _cameraBaseLocalPos = new Vector3(0f, 0.72f, 0f);

        private void Awake()
        {
            Instance = this;
            _controller = GetComponent<CharacterController>();
            _controller.height = 1.8f;
            _controller.radius = 0.38f;
            _controller.center = new Vector3(0f, 0f, 0f);
            _controller.stepOffset = 0.35f;

            _playerCamera = GetComponentInChildren<Camera>();
            if (_playerCamera == null)
            {
                GameObject camObj = new GameObject("PlayerFirstPersonCamera");
                camObj.tag = "MainCamera";
                camObj.transform.SetParent(transform, false);
                camObj.transform.localPosition = _cameraBaseLocalPos;
                _playerCamera = camObj.AddComponent<Camera>();
                _playerCamera.nearClipPlane = 0.15f;
                _playerCamera.farClipPlane = 250f;
                if (FindObjectOfType<AudioListener>() == null)
                    camObj.AddComponent<AudioListener>();
            }
        }

        public void TeleportToSpawn(Vector3 worldPos, float yawDegrees)
        {
            if (_controller != null) _controller.enabled = false;
            transform.position = worldPos;
            transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            _cameraPitch = 0f;
            if (_playerCamera != null)
            {
                _playerCamera.transform.localPosition = _cameraBaseLocalPos;
                _playerCamera.transform.localRotation = Quaternion.identity;
            }
            if (_controller != null) _controller.enabled = true;
        }

        private void Update()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInPlayableExploration)
            {
                UnlockCursor();
                return;
            }

            if (CinematicDirector.Instance != null && CinematicDirector.Instance.IsPlayingCinematic)
            {
                UnlockCursor();
                return;
            }

            if (GameStateManager.Instance.IsModalOverlayOpen)
            {
                UnlockCursor();
                return;
            }

            // Hold Tab or Left Alt to temporarily free mouse cursor for clicking HUD buttons if desired
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
                HandleMouseLook();
            }

            HandleMovement();
        }

        private void HandleMouseLook()
        {
            float sens = GameStateManager.Instance != null
                ? GameStateManager.Instance.SaveData.mouseSensitivity
                : mouseSensitivity;

            // Finer aiming control when zoomed in during Camera Mode
            if (PhotographySystem.Instance != null && PhotographySystem.Instance.IsInCameraMode)
            {
                float zoomFactor = Mathf.Clamp(35f / Mathf.Max(18f, PhotographySystem.Instance.Settings.focalLengthMm), 0.25f, 1.0f);
                sens *= zoomFactor;
            }

            float mouseX = Input.GetAxis("Mouse X") * sens;
            float mouseY = Input.GetAxis("Mouse Y") * sens;

            transform.Rotate(Vector3.up * mouseX);

            _cameraPitch -= mouseY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -maxLookPitchDegrees, maxLookPitchDegrees);
            if (_playerCamera != null)
            {
                _playerCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            float moveX = 0f;
            float moveZ = 0f;

            if (Input.GetKey(KeyCode.D)) moveX += 1f;
            if (Input.GetKey(KeyCode.A)) moveX -= 1f;
            if (Input.GetKey(KeyCode.W)) moveZ += 1f;
            if (Input.GetKey(KeyCode.S)) moveZ -= 1f;

            Vector3 inputDir = new Vector3(moveX, 0f, moveZ);
            if (inputDir.sqrMagnitude > 1f)
                inputDir.Normalize();

            bool inCamMode = PhotographySystem.Instance != null && PhotographySystem.Instance.IsInCameraMode;
            float speed = inCamMode
                ? cameraModeWalkSpeed
                : (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);

            Vector3 worldMove = (transform.right * inputDir.x + transform.forward * inputDir.z) * speed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            _verticalVelocity += gravity * Time.deltaTime;
            worldMove.y = _verticalVelocity;

            _controller.Move(worldMove * Time.deltaTime);

            // Subtle walking head-bob and footstep trigger
            float planarSpeed = new Vector2(worldMove.x, worldMove.z).magnitude;
            if (planarSpeed > 0.4f && _controller.isGrounded)
            {
                _stepCycleTimer += Time.deltaTime * (planarSpeed * 1.65f);
                if (_playerCamera != null && !inCamMode)
                {
                    float bobY = Mathf.Sin(_stepCycleTimer * Mathf.PI) * 0.032f;
                    _playerCamera.transform.localPosition = _cameraBaseLocalPos + new Vector3(0f, bobY, 0f);
                }

                if (_stepCycleTimer >= 1.0f)
                {
                    _stepCycleTimer -= 1.0f;
                    if (ProceduralSoundscapeGenerator.Instance != null)
                        ProceduralSoundscapeGenerator.Instance.PlayFootstep();
                }
            }
            else if (_playerCamera != null)
            {
                _playerCamera.transform.localPosition = Vector3.Lerp(
                    _playerCamera.transform.localPosition,
                    _cameraBaseLocalPos,
                    Time.deltaTime * 8f);
            }
        }

        private static void LockCursor()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private static void UnlockCursor()
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
