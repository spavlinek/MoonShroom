using UnityEngine;
using System.Collections;

//I used this tutorial from youtube on how to make a platform fall: https://www.youtube.com/watch?v=k70z88Xivzs
public class FallingPlatformScript : MonoBehaviour
{
    public float fallDelay = 2f;
    public float destroyDelay = 3f;
    public float shakeAmount = 0.1f;       // How much to shake
    public float shakeSpeed = 20f;         // How fast to shake

    private Rigidbody2D rb;
    private bool isFalling = false;
    private Vector3 originalPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            StartCoroutine(FallAfterDelay());
        }
    }

    private IEnumerator FallAfterDelay()
    {
        float elapsed = 0f;

        // Shake while waiting
        while (elapsed < fallDelay)
        {
            float shakeOffset = Mathf.Sin(elapsed * shakeSpeed) * shakeAmount;
            transform.position = originalPosition + new Vector3(shakeOffset, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Make the platform fall
        isFalling = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;  // Faster fall

        Destroy(gameObject, destroyDelay);
    }
}
