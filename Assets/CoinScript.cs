using UnityEngine;

public class CoinScript : MonoBehaviour
{
    public int coinValue = 10;
    private bool hasBeenCollected = false; // Prevent multiple collections

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player collided with the coin AND it hasn't been collected yet
        if (collision.gameObject.CompareTag("Player") && !hasBeenCollected)
        {
            hasBeenCollected = true;
            
            GameManager.Gary?.AddToScore(coinValue);
            
            SoundManager.Steve?.MakeCoinSound();
            
            // Destroy the coin
            Destroy(gameObject);
        }
    }
}
