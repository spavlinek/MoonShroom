using UnityEngine;
using System.Collections;

public class BossTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueData bossDialogue;
    public bool pausePlayerDuringDialogue = true;
    
    [Header("Protective Barrier")]
    public GameObject barrierToActivate;  // Protective barrier between player and boss
    public bool removeBarrierAfterDialogue = true;  // Automatically remove barrier when dialogue ends
    
    private bool hasTriggered = false;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerBossIntro();
        }
    }
    
    private void TriggerBossIntro()
    {
        // Activate protective barrier between player and boss
        // This prevents:
        // - Boss from attacking player during dialogue
        // - Player from attacking boss before dialogue finishes
        if (barrierToActivate != null)
        {
            barrierToActivate.SetActive(true);
        }
        
        // Trigger the dialogue
        if (DialogueManager.Instance != null && bossDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(bossDialogue);
            
            // Start checking for dialogue completion
            if (removeBarrierAfterDialogue)
            {
                StartCoroutine(WaitForDialogueToEnd());
            }
        }
    }
    
    private IEnumerator WaitForDialogueToEnd()
    {
        // Wait for dialogue to complete
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }
        
        // Dialogue finished - remove the barrier
        if (barrierToActivate != null)
        {
            barrierToActivate.SetActive(false);
        }
    }
}

