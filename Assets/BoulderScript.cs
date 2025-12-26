using UnityEngine;

public class BoulderScript : MonoBehaviour
{
    [Header("Boulder Settings")]
    public float fallSpeed = 5f;           // Gravity scale when falling
    public float lifetime = 1.5f;            // Boulder disappears after this many seconds
    
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Start falling immediately
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallSpeed;
        
        // Always destroy after lifetime
        Destroy(gameObject, lifetime);
    }

}