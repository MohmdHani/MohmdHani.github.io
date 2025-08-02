using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] private bool smoothFollow = true;
    
    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;
    
    [Header("Look Ahead Settings")]
    [SerializeField] private bool enableLookAhead = true;
    [SerializeField] private float lookAheadDistance = 3f;
    [SerializeField] private float lookAheadSpeed = 2f;
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeIntensity = 0.5f;
    
    // Private variables
    private Vector3 desiredPosition;
    private Vector3 currentVelocity;
    private Vector3 lookAheadOffset;
    private bool isShaking = false;
    private Vector3 originalPosition;
    
    // Mobile optimization
    private float targetFrameRate = 60f;
    private float frameTime;
    
    void Start()
    {
        InitializeCamera();
    }
    
    void InitializeCamera()
    {
        // Find target if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Set initial position
        if (target != null)
        {
            transform.position = target.position + offset;
        }
        
        // Store original position
        originalPosition = transform.position;
        
        // Set frame rate for mobile
        frameTime = 1f / targetFrameRate;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        UpdateCameraPosition();
        ApplyBounds();
        ApplyShake();
    }
    
    void UpdateCameraPosition()
    {
        // Calculate base position
        Vector3 basePosition = target.position + offset;
        
        // Add look ahead offset
        if (enableLookAhead)
        {
            UpdateLookAhead();
            basePosition += lookAheadOffset;
        }
        
        // Set desired position
        desiredPosition = basePosition;
        
        // Apply smooth following
        if (smoothFollow)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                desiredPosition, 
                ref currentVelocity, 
                1f / followSpeed
            );
        }
        else
        {
            transform.position = desiredPosition;
        }
    }
    
    void UpdateLookAhead()
    {
        if (target == null) return;
        
        // Get player movement direction
        PlayerController playerController = target.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsMoving())
        {
            // Determine look ahead direction based on player facing
            Vector3 lookDirection = target.right * lookAheadDistance;
            
            // Smoothly move look ahead offset
            lookAheadOffset = Vector3.Lerp(
                lookAheadOffset, 
                lookDirection, 
                lookAheadSpeed * Time.deltaTime
            );
        }
        else
        {
            // Return to center when not moving
            lookAheadOffset = Vector3.Lerp(
                lookAheadOffset, 
                Vector3.zero, 
                lookAheadSpeed * Time.deltaTime
            );
        }
    }
    
    void ApplyBounds()
    {
        if (!useBounds) return;
        
        Vector3 boundedPosition = transform.position;
        
        // Clamp X position
        boundedPosition.x = Mathf.Clamp(boundedPosition.x, minX, maxX);
        
        // Clamp Y position
        boundedPosition.y = Mathf.Clamp(boundedPosition.y, minY, maxY);
        
        transform.position = boundedPosition;
    }
    
    void ApplyShake()
    {
        if (!isShaking) return;
        
        // Apply random shake offset
        Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
        transform.position += shakeOffset;
    }
    
    public void ShakeCamera()
    {
        StartCoroutine(ShakeCoroutine());
    }
    
    public void ShakeCamera(float duration, float intensity)
    {
        StartCoroutine(ShakeCoroutine(duration, intensity));
    }
    
    IEnumerator ShakeCoroutine()
    {
        yield return StartCoroutine(ShakeCoroutine(shakeDuration, shakeIntensity));
    }
    
    IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isShaking = false;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }
    
    public void EnableLookAhead(bool enable)
    {
        enableLookAhead = enable;
        if (!enable)
        {
            lookAheadOffset = Vector3.zero;
        }
    }
    
    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }
    
    public void SetSmoothFollow(bool smooth)
    {
        smoothFollow = smooth;
    }
    
    // Mobile optimization methods
    public void SetTargetFrameRate(float frameRate)
    {
        targetFrameRate = frameRate;
        frameTime = 1f / targetFrameRate;
    }
    
    public void OptimizeForMobile()
    {
        // Reduce follow speed for smoother mobile experience
        followSpeed = Mathf.Min(followSpeed, 3f);
        
        // Reduce look ahead for better performance
        lookAheadDistance = Mathf.Min(lookAheadDistance, 2f);
        
        // Set lower frame rate for mobile
        SetTargetFrameRate(30f);
    }
    
    // Gizmos for debugging bounds
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        
        Gizmos.color = Color.yellow;
        
        // Draw camera bounds
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, transform.position.z);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 1f);
        
        Gizmos.DrawWireCube(center, size);
        
        // Draw current camera position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw target position
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position + offset, 0.3f);
        }
    }
}