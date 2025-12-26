using UnityEngine;

public class LowGravityZone : MonoBehaviour
{
    [Header("Gravity Settings")]
    public float lowGravityScale = 0.3f; // Lower = less gravity

    public float jumpHeightMultiplier = 1.5f;

    private float normalJumpHeight = 0f;
    public float transitionSpeed = 2f; // How fast gravity changes
    
    private float normalGravityScale = 1f;
    private Rigidbody2D playerRb;
    private CharacterController2D playerController;
    private bool playerInZone = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody2D>();
            playerController = other.GetComponent<CharacterController2D>();
            
            if (playerRb != null)
            {
                // Store the normal gravity for when they leave
                normalGravityScale = playerRb.gravityScale;
                playerInZone = true;

                normalJumpHeight = playerController.jumpHeight;
                playerController.jumpHeight = normalJumpHeight * jumpHeightMultiplier;
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerRb != null)
        {
            playerInZone = false;
            playerController.jumpHeight = normalJumpHeight;
        }
    }
    
    void Update()
    {
        if (playerRb == null) return;
        
        // Smoothly transition gravity
        float targetGravity = playerInZone ? lowGravityScale : normalGravityScale;
        playerRb.gravityScale = Mathf.Lerp(playerRb.gravityScale, targetGravity, Time.deltaTime * transitionSpeed);
    }
}
