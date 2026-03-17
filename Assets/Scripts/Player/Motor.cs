using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Motor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10.5f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 22f;
    [SerializeField] private float airControl = 0.45f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.35f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedForce = -2f;

    [Header("Slide")]
    [SerializeField] private float slideStartSpeed = 12f;
    [SerializeField] private float slideEndSpeed = 7f;
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private float slideHeight = 1.0f;
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float minSpeedToSlide = 6f;
    [SerializeField] private float heightSmooth = 14f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.14f;
    [SerializeField] private float dashCooldown = 3f;

    [Header("Camera")]
    [SerializeField] private float baseFov = 78f;
    [SerializeField] private float maxMoveFov = 90f;
    [SerializeField] private float dashFov = 94f;
    [SerializeField] private float fovSmooth = 10f;
    [SerializeField] private float standCameraY = 0.75f;
    [SerializeField] private float slideCameraY = 0.52f;
    [SerializeField] private float cameraHeightSmooth = 14f;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private bool isSliding;
    private float slideTimer;
    private Vector3 slideDirection;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleCapsuleHeight();
        HandleCamera();
    }

    private void HandleMovement()
    {
        bool grounded = controller.isGrounded;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = transform.right * inputX + transform.forward * inputZ;
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        bool hasInput = inputDirection.sqrMagnitude > 0.001f;
        bool sprintHeld = Input.GetKey(KeyCode.LeftShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool slidePressed = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C);
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftAlt);

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (CanStartDash(dashPressed))
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            dashDirection = hasInput ? inputDirection : transform.forward;
        }

        if (CanStartSlide(grounded, sprintHeld, slidePressed))
        {
            isSliding = true;
            slideTimer = slideDuration;

            Vector3 flatVelocity = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
            slideDirection = flatVelocity.sqrMagnitude > 0.01f ? flatVelocity.normalized : transform.forward;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            horizontalVelocity = dashDirection * dashSpeed;

            if (dashTimer <= 0f)
                isDashing = false;
        }
        else if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            float slideT = 1f - Mathf.Clamp01(slideTimer / slideDuration);
            float currentSlideSpeed = Mathf.Lerp(slideStartSpeed, slideEndSpeed, slideT);
            horizontalVelocity = slideDirection * currentSlideSpeed;

            if (slideTimer <= 0f || !grounded)
                isSliding = false;
        }
        else
        {
            float targetSpeed = sprintHeld ? sprintSpeed : walkSpeed;
            Vector3 targetHorizontalVelocity = inputDirection * targetSpeed;

            float control = grounded ? 1f : airControl;
            float lerpSpeed = hasInput ? acceleration : deceleration;

            horizontalVelocity = Vector3.Lerp(
                horizontalVelocity,
                targetHorizontalVelocity,
                lerpSpeed * control * Time.deltaTime
            );
        }

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = groundedForce;

        if (jumpPressed && grounded && !isSliding && !isDashing)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalVelocity = horizontalVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private bool CanStartSlide(bool grounded, bool sprintHeld, bool slidePressed)
    {
        if (!grounded) return false;
        if (isSliding) return false;
        if (isDashing) return false;
        if (!sprintHeld) return false;
        if (!slidePressed) return false;

        Vector3 flatVelocity = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
        return flatVelocity.magnitude >= minSpeedToSlide;
    }

    private bool CanStartDash(bool dashPressed)
    {
        if (!dashPressed) return false;
        if (isDashing) return false;
        if (dashCooldownTimer > 0f) return false;
        return true;
    }

    private void HandleCapsuleHeight()
    {
        float targetHeight = isSliding ? slideHeight : standHeight;

        controller.height = Mathf.Lerp(controller.height, targetHeight, heightSmooth * Time.deltaTime);
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
    }

    private void HandleCamera()
    {
        if (cameraPivot != null)
        {
            Vector3 localPos = cameraPivot.localPosition;
            float targetY = isSliding ? slideCameraY : standCameraY;
            localPos.y = Mathf.Lerp(localPos.y, targetY, cameraHeightSmooth * Time.deltaTime);
            cameraPivot.localPosition = localPos;
        }

        if (playerCamera != null)
        {
            float horizontalSpeed = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;
            float speed01 = Mathf.InverseLerp(0f, sprintSpeed, horizontalSpeed);
            float targetFov = Mathf.Lerp(baseFov, maxMoveFov, speed01);

            if (isDashing)
                targetFov = dashFov;

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, fovSmooth * Time.deltaTime);
        }
    }
}