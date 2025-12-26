using UnityEngine;

public class CaveTrigger : MonoBehaviour
{
    public ShaderDarknessController lightingController;
    public bool isDarkZone = true; // Set to true for cave entrance, false for exit
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isDarkZone)
            {
                lightingController.EnableDarkness();
            }
            else
            {
                lightingController.DisableDarkness();
            }
        }
    }
}
