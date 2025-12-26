using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


//CharacterController2D is based upon the 2DCharacterController from Sharp Coder blog
//adapted by Sara Pavlinek on 10/24/25 
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    //move player in 2D
    public float speed = 1f;
    public float jumpHeight = 2f;
    public float gravityScale = 1f;

    //private values
    private Rigidbody2D rb;
    private InputAction moveAction, jumpAction, shootAction;

    private float moveDirection = 0f;

    // Death flag
    private bool isDead = false;
    public bool jumpFlag = false;

    public GameObject bulletPrefab;
    public Transform firePoint; // position where bullet spawns
    public float shootCooldown = 0.5f; // time between shots


    //ground detection
    [HeaderAttribute("Ground Ditection")]
    public bool isGrounded = false;
    public float groundCheckRadius;
    public Vector2 groundCheckOffset;

    public LayerMask groundLayerMask;
    public Animator animator;

    public bool facingRight = true;

    private bool onSurfaceEffector = false;

    private float lastShootTime = -999f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get the components
        rb = GetComponent<Rigidbody2D>();

        //did the animator get properly set
        if (animator == null){
            animator = GetComponentInChildren<Animator>();
        }

        //configure the rb
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = gravityScale;

        //define actions
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        shootAction = InputSystem.actions.FindAction("Attack");
    }

    // Update is called once per frame
    void Update()
    {
        // Don't allow input during Ready or GameOver states
        if (GameManager.Gary.state != GameState.Playing)
        {
            moveDirection = 0;
            return;
        }
        if (!isDead)
        {
            //set the move direction
            moveDirection = moveAction.ReadValue<Vector2>().x;
        }
        else
        {
            moveDirection = 0;
        }

        //get the jump action
        // if (jumpAction.WasPressedThisFrame() && isGrounded && !isDead)
        // {
        //     rb.linearVelocityY = jumpHeight;
        //     SoundManager.Steve.MakeJumpSound();
        //     animator.SetBool("Grounded", false);
        //     animator.SetTrigger("JumpTrigger");

        // } else {
        //     animator.SetBool("Grounded", isGrounded);
        // }
        if (jumpAction.WasPressedThisFrame() && isGrounded && !isDead)
        {
            jumpFlag = true;
            animator.SetBool("Grounded", false);
            animator.SetTrigger("JumpTrigger");

        }

        if (!jumpFlag)
        {
            animator.SetBool("Grounded", isGrounded);
        }

        //update the animator speed
        animator.SetFloat("Speed", Mathf.Abs(moveDirection));


        //set the facing direction
        if (moveDirection < -0.01f && facingRight)
        {
            facingRight = false;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1f;
            transform.localScale = currentScale;
        }
        else if (moveDirection > 0.01f && !facingRight)
        {
            facingRight = true;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1f;
            transform.localScale = currentScale;
        }

        //shooting is only allowed in level 2 +
        if (LevelManager.Larry.currentLevel >= 2)
        {
            if (shootAction.WasPressedThisFrame() && !isDead)
            {
                
                // Check if enough time has passed since last shot
                if (Time.time >= lastShootTime + shootCooldown)
                {
                    animator.SetTrigger("ShootTrigger");
                    StartCoroutine(ShootWithDelay(0.2f));
                    lastShootTime = Time.time;
                }
            }
        }
    }
    
    IEnumerator ShootWithDelay(float delay)
    {
        // Wait for the animation to reach the right frame
        yield return new WaitForSeconds(delay);
        Shoot();
    }

    void FixedUpdate()
    {

        //reset ground check
        isGrounded = false;

        //is the jump flag set
        if (jumpFlag){
            SoundManager.Steve.MakeJumpSound();
            rb.linearVelocityY = jumpHeight;
            //turn off the jump flag
            jumpFlag = false;
        } else
        {
            //ground check
            Vector3 groundCheck = groundCheckOffset;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + groundCheck, groundCheckRadius, groundLayerMask);
            if (colliders.Length > 0){
                isGrounded = true;
            }
        }

        // apply  the movement velocity if not surface effector
        if (!onSurfaceEffector)
        {
            rb.linearVelocityX = moveDirection * speed;
        }
    }

    private void OnDrawGizmos()
    {
        //show the ground radius
        if (isGrounded)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Vector3 groundCheck = groundCheckOffset;
        Gizmos.DrawWireSphere(transform.position + groundCheck, groundCheckRadius);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "FallingBox" && !isDead)
        {
            isDead = true; // Prevent multiple death calls
            GameManager.Gary.PlayerDied();
        }
        
        if (collision.gameObject.tag == "EndOfLevel")
        {
            // Player reached the end of the level
            GameManager.Gary.LevelComplete();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
        {
            return;
        }
        if (collision.gameObject.tag == "Enemy")
        {
            //test the direction of the hit
            ContactPoint2D contact = collision.contacts[0];
            if (contact.normal.y > 0.8f)
            {
                //we killed the enemy
                EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
                if (enemy)
                {
                    enemy.PlayerKilledEnemy();
                }
                else
                {
                    return;
                }
            }
            else
            {
                //enemy has killed us
                Debug.Log("Killed player");
                animator.SetTrigger("DeathTrigger");
                isDead = true;
                GameManager.Gary.PlayerDied();
            }


        }
        if (collision.gameObject.tag == "BossBullet")
        {
            //boss has killed us
            Debug.Log("Killed player");
            animator.SetTrigger("DeathTrigger");
            isDead = true;
            GameManager.Gary.PlayerDied();
        }

        if (collision.gameObject.tag == "Boss")
        {
            Debug.Log("Boss and player collision");
            //test the direction of the hit
            ContactPoint2D contact = collision.contacts[0];
            Debug.Log(contact);
            if (contact.normal.y > 0.8f)
            {
                Debug.Log("Jumped on boss, it got damaged");
                // We jumped on the boss - damage it!
                BossController boss = collision.gameObject.GetComponent<BossController>();
                if (boss != null && boss.isAlive)
                {
                    boss.TakeDamage(20);
                    //the player jumps up a bit
                    jumpFlag = true;
                    animator.SetBool("Grounded", false);
                    animator.SetTrigger("JumpTrigger");

                }
            }
            else
            {
                // Boss hit us from side/below - boss kills us
                Debug.Log("Boss killed player");
                animator.SetTrigger("DeathTrigger");
                isDead = true;
                GameManager.Gary.PlayerDied();
            }
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if we're on the surface effector platform
        if (collision.gameObject.tag == "Slime")
        {
            onSurfaceEffector = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Check if we left the surface effector platform
        if (collision.gameObject.tag == "Slime")
        {
            onSurfaceEffector = false;
        }
    }

    public void ResetAfterDeath()
    {
        isDead = false;
        moveDirection = 0;

        // Reset velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Reset animator if needed
        if (animator != null)
        {
            animator.ResetTrigger("DeathTrigger");
            animator.SetFloat("Speed", 0);
            animator.SetBool("Grounded", true);
        }
    }
    
    void Shoot()
    {
        // Determine spawn position
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        
        Quaternion bulletRotation = bulletPrefab.transform.rotation;
        
        // If facing left, add 180 degrees to flip the bullet
        if (!facingRight)
        {
            bulletRotation *= Quaternion.Euler(0, 0, 180f);
        }
        
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, bulletRotation);
        
        // Set the bullet's direction based on which way the player is facing
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(facingRight);
        }

        SoundManager.Steve.MakeShootSound();
        animator.ResetTrigger("ShootTrigger");
    }
}
