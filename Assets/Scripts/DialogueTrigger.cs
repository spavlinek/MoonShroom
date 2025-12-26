using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueData;
    public bool triggerOnStart = false;
    public bool triggerOnce = true;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        if (triggerOnStart)
        {
            TriggerDialogue();
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            TriggerDialogue();
        }
    }
    
    public void TriggerDialogue()
    {
        if (DialogueManager.Instance != null && dialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData);
            
            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
    }
}