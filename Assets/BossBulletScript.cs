using UnityEngine;

public class BossBulletScript : MonoBehaviour
{
    public float bulletLifetime = 3f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        
        // Destroy the bullet after a set time
        Destroy(gameObject, bulletLifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Don't destroy if hitting the boss (prevent friendly fire)
        if (collision.gameObject.tag == "Boss")
        {
            return;
        }
        
        // Destroy on hitting anything else
        Destroy(gameObject);
    }
}
