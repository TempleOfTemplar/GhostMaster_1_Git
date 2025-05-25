using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    public float moveSpeed = 10f;
    public float fastMoveSpeed = 20f;
    public float mouseSensitivity = 2f;
    public float zoomSpeed = 5f;
    public float smoothTime = 0.3f;
    
    [Header("Camera Limits")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;
    public float minY = 2f;
    public float maxY = 30f;
    
    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float zoomSmoothTime = 0.3f;
    
    [Header("Mouse Look")]
    public bool enableMouseLook = true;
    public float rotationSpeed = 100f;
    public float maxLookAngle = 80f;
    public float minLookAngle = -80f;
    
    private Camera cam;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private float targetZoom;
    private float zoomVelocity;
    
    // Mouse look variables
    private float mouseX;
    private float mouseY;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isRotating = false;
    
    // Input tracking
    private bool fastMovePressed = false;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        targetPosition = transform.position;
        targetZoom = cam.fieldOfView;
        
        // Initialize rotation based on current transform
        Vector3 euler = transform.eulerAngles;
        rotationY = euler.y;
        rotationX = euler.x;
        
        // Ensure cursor is visible and not locked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void Update()
    {
        HandleInput();
        HandleMouseLook();
        HandleMovement();
        HandleZoom();
    }
    
    void HandleInput()
    {
        // Check for fast move modifier (Left Shift)
        fastMovePressed = Input.GetKey(KeyCode.LeftShift);
        
        // Mouse look toggle (Right mouse button)
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void HandleMouseLook()
    {
        if (!enableMouseLook || !isRotating) return;
        
        mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        
        rotationY += mouseX;
        rotationX -= mouseY;
        
        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, minLookAngle, maxLookAngle);
        
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
    
    void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;
        
        // WASD movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveDirection += transform.right;
        
        // Vertical movement (Q/E or Page Up/Page Down)
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.PageDown))
            moveDirection -= transform.up;
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.PageUp))
            moveDirection += transform.up;
        
        // Normalize and apply speed
        moveDirection.Normalize();
        float currentSpeed = fastMovePressed ? fastMoveSpeed : moveSpeed;
        moveDirection *= currentSpeed * Time.deltaTime;
        
        // Calculate target position
        targetPosition = transform.position + moveDirection;
        
        // Apply position limits
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        
        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
    
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        
        // Smooth zoom
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetZoom, ref zoomVelocity, zoomSmoothTime);
    }
    
    // Public methods for external control
    public void SetCameraPosition(Vector3 position)
    {
        targetPosition = position;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        transform.position = targetPosition;
    }
    
    public void FocusOnTarget(Transform target, float offsetDistance = 10f)
    {
        if (target == null) return;
        
        Vector3 focusPosition = target.position - transform.forward * offsetDistance;
        SetCameraPosition(focusPosition);
    }
    
    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
    
    public void ResetCamera()
    {
        transform.position = Vector3.zero + Vector3.up * 10f + Vector3.back * 10f;
        transform.LookAt(Vector3.zero);
        targetZoom = (minZoom + maxZoom) / 2f;
        cam.fieldOfView = targetZoom;
        
        Vector3 euler = transform.eulerAngles;
        rotationY = euler.y;
        rotationX = euler.x;
    }
    
    // Enable/disable camera controls (useful for cutscenes or UI)
    public void SetControlsEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw movement bounds
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }
}