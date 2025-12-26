using UnityEngine;
 
public class EnemyScript : MonoBehaviour
{
    public float enemySpeed;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    public bool facingLeft = false;
    public bool isAlive = true;

    private Animator animator;
 
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        sprite.flipX = false;
        animator = GetComponent<Animator>();
    }
 
    private void FixedUpdate()
    {
        if (isAlive){
            // set the x-velocity to positive or negative speed, depending on which way we are facing.
            rb.linearVelocityX = facingLeft ? -enemySpeed : enemySpeed;
        }
    }
 
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "TurnAround")
        {
            facingLeft = !facingLeft;
            sprite.flipX = !sprite.flipX;
        }
    }
 
    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         if (GameManager.Gary != null && GameManager.Gary.state == GameState.Playing)
    //         {
    //             GameManager.Gary.PlayerDied();
    //         }
            
    //     } 
    // }

    public void PlayerKilledEnemy()
    {
        if (isAlive){
            //handle the event of my death
            isAlive = false;

            //run the death animatior
            animator.SetTrigger("EnemyDeath");

            //change rb bodyType
            //rb.bodyType = RigidbodyType2D.Kinematic;
            gameObject.layer = LayerMask.NameToLayer("NotForPlayer");
            
            //Destroy the poison cloud if it exists - mainly for green enemy
            Transform parent = transform.parent;
            if (parent != null)
            {
                Transform poisonCloud = parent.Find("poison cloud");
                if (poisonCloud != null)
                {
                    Destroy(poisonCloud.gameObject);
                }
            }

            //Destroy the game object
            Destroy(gameObject, 2f);
        }
        
    }
}
