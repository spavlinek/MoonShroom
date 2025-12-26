using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    [Header("Visual Feedback")]
    public GameObject activeVisual;
    
    
    private bool isActivated = false;
    
    void Start()
    {
        // Start with inactive flag
        if (activeVisual) activeVisual.SetActive(false);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Only activate once, and only for the player
        if (!isActivated && collision.gameObject.tag == "Player")
        {
            ActivateCheckpoint();
        }
    }
    
    void ActivateCheckpoint()
    {
        isActivated = true;
        
        if (activeVisual) activeVisual.SetActive(true);
        
        
        // Update spawn point in GameManager
        if (GameManager.Gary != null)
        {
            GameManager.Gary.SetPlayerSpawnPoint(transform.position);
            Debug.Log("Checkpoint activated at: " + transform.position);
        }
    }
}
