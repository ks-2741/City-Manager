using UnityEngine;

/// <summary>
/// Top-down / RTS-style camera controller.
/// - WASD: pans the camera across the ground plane (relative to current yaw)
/// - Hold Right Mouse Button + move mouse: rotates the camera (yaw + pitch), both clamped
/// - Mouse Scroll Wheel: zooms in/out (dolly along the camera's forward vector), clamped
///
/// Attach directly to the Camera (or an empty parent rig with the Camera as a child -
/// see "Rig vs. direct" note near the bottom of this file).
/// </summary>
[RequireComponent(typeof(Camera))]
public class TopDownCameraController : MonoBehaviour
{
    [Header("Panning (WASD)")]
    [Tooltip("Movement speed in units/second.")]
    [SerializeField] private float panSpeed = 20f;
    [Tooltip("Optional speed multiplier while holding Left Shift.")]
    [SerializeField] private float fastPanMultiplier = 2f;
    [SerializeField] private KeyCode fastPanKey = KeyCode.LeftShift;

    [Header("Pan Boundaries (world space, X/Z)")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minZ = -50f;
    [SerializeField] private float maxZ = 50f;

    [Header("Rotation (Mouse, held with Right Mouse Button)")]
    [SerializeField] private bool requireHoldToRotate = true;
    [SerializeField] private KeyCode rotateButton = KeyCode.Mouse1; // right click
    [SerializeField] private float mouseSensitivity = 3f;
    [Tooltip("Pitch = rotation around the local X axis (looking more/less steeply down).")]
    [SerializeField] private float minPitch = 20f;
    [SerializeField] private float maxPitch = 80f;
    [Tooltip("If false, yaw (Y axis) is unrestricted. If true, yaw is clamped between minYaw/maxYaw.")]
    [SerializeField] private bool clampYaw = false;
    [SerializeField] private float minYaw = -180f;
    [SerializeField] private float maxYaw = 180f;

    [Header("Zoom (Mouse Scroll moves camera height up/down)")]
    [Tooltip("How much height changes per scroll tick.")]
    [SerializeField] private float zoomSpeed = 10f;
    [Tooltip("The camera can never go below this world-space Y height (closest zoom).")]
    [SerializeField] private float minHeight = 5f;
    [Tooltip("The camera can never go above this world-space Y height (farthest zoom).")]
    [SerializeField] private float maxHeight = 40f;
    [Tooltip("Smoothing time for zoom; 0 = instant.")]
    [SerializeField] private float zoomSmoothTime = 0.15f;

    // internal state
    private float yaw;
    private float pitch;
    private float currentHeight;
    private float targetHeight;
    private float zoomVelocity;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        // Initialize yaw/pitch from the camera's current rotation so it starts
        // wherever you've placed it in the editor.
        Vector3 startEuler = transform.eulerAngles;
        pitch = startEuler.x;
        yaw = startEuler.y;

        // Start from wherever the camera is placed in the editor, clamped into range.
        currentHeight = Mathf.Clamp(transform.position.y, minHeight, maxHeight);
        targetHeight = currentHeight;

        Vector3 startPos = transform.position;
        startPos.y = currentHeight;
        transform.position = startPos;
    }

    private void Update()
    {
        HandleRotation();
        HandlePanning();
        HandleZoom();
    }

    private void HandleRotation()
    {
        bool rotating = !requireHoldToRotate || Input.GetKey(rotateButton);
        if (!rotating) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY; // subtract so moving mouse up tilts view up

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (clampYaw)
        {
            yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        }

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandlePanning()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        if (h == 0f && v == 0f) return;

        // Move relative to yaw only, flattened onto the XZ plane, so panning
        // stays consistent regardless of current pitch.
        Quaternion flatYawRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 forward = flatYawRotation * Vector3.forward;
        Vector3 right = flatYawRotation * Vector3.right;

        Vector3 moveDir = (forward * v + right * h).normalized;

        float speed = panSpeed;
        if (Input.GetKey(fastPanKey))
        {
            speed *= fastPanMultiplier;
        }

        Vector3 newPos = transform.position + moveDir * speed * Time.deltaTime;

        if (useBoundaries)
        {
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        }

        transform.position = newPos;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            // Scrolling up (positive) zooms in -> lowers height.
            targetHeight -= scroll * zoomSpeed;
            targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);
        }

        if (zoomSmoothTime <= 0f)
        {
            currentHeight = targetHeight;
        }
        else
        {
            currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref zoomVelocity, zoomSmoothTime);
        }

        Vector3 pos = transform.position;
        pos.y = currentHeight;
        transform.position = pos;
    }
}

/*
NOTES / HOW TO ADAPT:

1) Rig vs. direct attachment
   This script moves and rotates the Camera object directly, which works fine
   for most top-down setups. If you'd rather zoom by moving an empty parent
   "rig" forward/back while the camera stays at a fixed local offset (more
   common in RTS games, avoids clipping through terrain), let me know and
   I'll restructure it as a two-object rig (Rig position = pan target,
   Camera local position = zoom offset along -forward).

2) Zoom implementation
   The current HandleZoom() tracks a target/current *distance* value but
   doesn't yet apply it to position, since "distance from what" depends on
   your setup (a fixed ground plane vs. a look-at pivot). Two common options:

   OPTION A - Height-based zoom (simplest, good for a straight-down or
   near-straight-down camera):
       Vector3 pos = transform.position;
       pos.y = currentDistance; // or currentDistance * some scale
       transform.position = pos;

   OPTION B - Dolly along forward vector (works at any pitch angle):
       float delta = currentDistance - previousDistance; // track previousDistance each frame
       transform.position += transform.forward * delta;
       previousDistance = currentDistance;

   Tell me which fits your scene (flat ground plane vs. varied terrain, and
   whether pitch stays fixed or changes) and I'll fill in the exact math.

3) Input System
   This uses the legacy Input Manager (Input.GetAxis / Input.GetKey), which
   matches the WASD + left-click conventions already used elsewhere in your
   project. If you're on Unity's new Input System package instead, say so and
   I'll rewrite the input calls.
*/