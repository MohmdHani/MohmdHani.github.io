using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 2, -10);
    public float lookAheadDistance = 3f;
    
    [Header("Boundaries")]
    public bool useBoundaries = true;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;
    
    [Header("Mobile Optimization")]
    public bool enableShake = true;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    
    private Vector3 desiredPosition;
    private Vector3 smoothedPosition;
    private bool isShaking = false;
    private float shakeTimer = 0f;
    private Vector3 originalPosition;
    
    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Calculate desired position with look ahead
        Vector3 lookAhead = Vector3.zero;
        if (target.GetComponent<Rigidbody2D>() != null)
        {
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            lookAhead = targetRb.velocity.normalized * lookAheadDistance;
        }
        
        desiredPosition = target.position + offset + lookAhead;
        
        // Apply boundaries
        if (useBoundaries)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Smooth follow
        smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Apply shake if active
        if (isShaking)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            smoothedPosition += shakeOffset;
            
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                isShaking = false;
            }
        }
        
        transform.position = smoothedPosition;
    }
    
    public void ShakeCamera(float intensity = -1f, float duration = -1f)
    {
        if (!enableShake) return;
        
        isShaking = true;
        shakeTimer = duration > 0 ? duration : shakeDuration;
        shakeIntensity = intensity > 0 ? intensity : shakeIntensity;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetBoundaries(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }
    
    void OnDrawGizmosSelected()
    {
        if (useBoundaries)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, transform.position.z);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 1);
            Gizmos.DrawWireCube(center, size);
        }
        
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position + offset, 0.5f);
        }
    }
}