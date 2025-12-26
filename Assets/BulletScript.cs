using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float bulletSpeed = 10f;
    public float bulletLifetime = 3f;
    private Rigidbody2D rb;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Set the bullet velocity in the direction it's facing
        rb.linearVelocityX = movingRight ? bulletSpeed : -bulletSpeed;
        
        // Destroy the bullet after a set time
        Destroy(gameObject, bulletLifetime);
    }

    public void SetDirection(bool facingRight)
    {
        movingRight = facingRight;
        
        if (rb != null)
        {
            rb.linearVelocityX = movingRight ? bulletSpeed : -bulletSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Don't destroy if hitting the player
        if (collision.gameObject.tag == "Player")
        {
            return;
        }
        
        // Check if bullet hit an enemy
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
            if (enemy != null && enemy.isAlive)
            {
                enemy.PlayerKilledEnemy();
            }
            
            // Destroy the bullet
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.tag == "Boss")
        {
            BossController boss = collision.gameObject.GetComponent<BossController>();
            if (boss != null && boss.isAlive)
            {
                boss.TakeDamage(10);
            }
            
            // Destroy the bullet
            Destroy(gameObject);
            return;
        }
        if (collision.gameObject.tag == "BossBullet")
        {
            // Destroy the bullet
            Destroy(gameObject);
            Destroy(collision.gameObject);
            return;
        }


        
        if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}