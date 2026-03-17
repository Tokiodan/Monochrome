using UnityEngine;

public class CamMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;

    [Header("Look")]
    [SerializeField] private float sensitivity = 2.5f;
    [SerializeField] private float smoothTime = 0.02f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    private float yaw;
    private float pitch;

    private float smoothedMouseX;
    private float smoothedMouseY;
    private float mouseXVelocity;
    private float mouseYVelocity;

    private bool cursorLocked = true;

    private void Awake()
    {
        yaw = transform.eulerAngles.y;
    }

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            cursorLocked = !cursorLocked;

            if (cursorLocked)
                LockCursor();
            else
                UnlockCursor();
        }

        if (!cursorLocked)
            return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        smoothedMouseX = Mathf.SmoothDamp(smoothedMouseX, mouseX, ref mouseXVelocity, smoothTime);
        smoothedMouseY = Mathf.SmoothDamp(smoothedMouseY, mouseY, ref mouseYVelocity, smoothTime);
    }

    private void LateUpdate()
    {
        if (!cursorLocked)
            return;

        yaw += smoothedMouseX;
        pitch -= smoothedMouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}