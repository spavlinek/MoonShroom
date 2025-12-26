using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
    public Transform player;
    
    // Horizontal settings
    public float smoothTime = 1f;
    public float currentVelocity = 0f;
     // Vertical settings
    public float verticalSmoothTime = 0.3f;
    public float verticalVelocity = 0f;
    public float verticalDeadzone = 2f;
    
    // Camera shake
    private Vector3 originalPosition;
    private bool isShaking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player == null) return;
        
        // Store target position before adding shake
        Vector3 cameraPosition = transform.position;
        //cameraPosition.x = player.position.x;
        cameraPosition.x = Mathf.SmoothDamp(cameraPosition.x, player.position.x, ref currentVelocity, smoothTime);


        // Vertical movement with deadzone
        float verticalDifference = player.position.y - cameraPosition.y;

        // Only move camera if player is outside the safe zone
        if (verticalDifference > verticalDeadzone)
        {
            // Calculate target position aka player position minus the deadzone
            float targetY = player.position.y - verticalDeadzone;
            cameraPosition.y = Mathf.SmoothDamp(cameraPosition.y, targetY, ref verticalVelocity, verticalSmoothTime);
        }
        else
        {
            // Reset velocity when inside safe zone for smoother transitions
            verticalVelocity = 0;
        }

        // Store the target position for shake reference
        if (!isShaking)
        {
            originalPosition = cameraPosition;
        }
        
        transform.position = cameraPosition;
    }
    
    // Public method to snap camera to player position instantly
    public void SnapToPlayer()
    {
        if (player != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = player.position.x;
            newPosition.y = player.position.y;
            transform.position = newPosition;
            
            // Reset velocities so smoothing starts fresh
            currentVelocity = 0;
            verticalVelocity = 0;
        }
    }
    
    // Camera shake effect
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Generate random offset
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            
            // Apply shake to current camera position
            transform.position = new Vector3(
                transform.position.x + offsetX,
                transform.position.y + offsetY,
                transform.position.z
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isShaking = false;
    }
}
