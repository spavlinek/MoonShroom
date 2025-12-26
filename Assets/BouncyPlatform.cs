using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    public float bounceForce = 15f;
    private bool canBounce = true;
    public float bounceCooldown = 0.3f;
    public Animator animator;

    void Start()
    {
        if (animator == null){
            animator = GetComponentInChildren<Animator>();
        }
    } 

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && canBounce)
        {
            animator.SetBool("PlayerOnPlatform", true);
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                // Only bounce if player is moving downward
                if (playerRb.linearVelocityY <= 0)
                {
                    playerRb.linearVelocityY = bounceForce;
                    SoundManager.Steve.MakeJumpSound();
                    StartCoroutine(BounceCooldown());
                }
            }
        }
    }

    System.Collections.IEnumerator BounceCooldown()
    {
        canBounce = false;
        yield return new WaitForSeconds(bounceCooldown);
        animator.SetBool("PlayerOnPlatform", false);
        canBounce = true;
    }
}
