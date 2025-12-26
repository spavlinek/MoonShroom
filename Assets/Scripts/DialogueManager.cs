using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private TypewriterEffect typewriterEffect;
    private bool hasSkippedCurrentLine = false;
    
    void OnEnable()
    {
        Debug.Log($"=== DialogueManager OnEnable === GameObject: {gameObject.name}, Scene: {gameObject.scene.name}, GameObject active? {gameObject.activeInHierarchy}");
    }
    
    void OnDisable()
    {
        Debug.LogError($"!!! DialogueManager OnDisable !!! GameObject: {gameObject.name}, Scene: {gameObject.scene.name}");
    }
    
    void Awake()
    {
        Debug.Log($"=== DialogueManager Awake === GameObject: {gameObject.name}, Scene: {gameObject.scene.name}, enabled? {enabled}, gameObject.activeSelf? {gameObject.activeSelf}");
        
        // Allow each scene to have its own DialogueManager
        // Just set Instance to the current scene's DialogueManager
        Instance = this;
        Debug.Log($"DialogueManager Instance set to {gameObject.name}");
        
        typewriterEffect = dialogueText.GetComponent<TypewriterEffect>();
        
        // Setup button listener HERE in Awake since Start() sometimes doesn't run
        if (nextButton != null)
        {
            Debug.Log($"Setting up button: {nextButton.gameObject.name}, GameObject path: {GetGameObjectPath(nextButton.gameObject)}");
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
            Debug.Log($"Next button listener added in Awake. Listener count: {nextButton.onClick.GetPersistentEventCount()}, Runtime listeners can't be counted");
        }
        else
        {
            Debug.LogError("Next button is NULL in Awake!");
        }
        
        // Subscribe to typewriter events
        TypewriterEffect.CompleteTextRevealed += OnTextComplete;
        TypewriterEffect.CharacterRevealed += OnCharacterRevealed;
        Debug.Log("Subscribed to TypewriterEffect events in Awake");
    }
    
    void Start()
    {
        Debug.Log($"=== DialogueManager Start === {gameObject.name}");
        
        // Hide dialogue box at start
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = 0;
        }
    }

    private void OnCharacterRevealed(char character)
    {
        // Only play sound if dialogue is active
        if (isDialogueActive && !hasSkippedCurrentLine)
        {
            if (character != ' ')
            {
                SoundManager.Steve?.MakeTypingSound();
            }
        }
    }
    
    void OnDestroy()
    {
        Debug.Log($"=== DialogueManager OnDestroy === GameObject: {gameObject.name}");
        TypewriterEffect.CompleteTextRevealed -= OnTextComplete;
        TypewriterEffect.CharacterRevealed -= OnCharacterRevealed;
        
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
        }
    }
    
    // Helper to get full path of GameObject in hierarchy
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    void Update()
    {
        // If dialogue is active and player clicks, show all text
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            // If text is still typing, show it all immediately
            if (dialogueText.maxVisibleCharacters < dialogueText.textInfo.characterCount)
            {
                dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
                nextButton.interactable = true;
            }
        }
    }
    
    public void StartDialogue(DialogueData dialogue)
    {
        Debug.Log($"=== StartDialogue called === Instance is null? {Instance == null}, This GameObject: {gameObject.name}");
        Debug.Log($"nextButton is null? {nextButton == null}, dialogueBox is null? {dialogueBox == null}");
        
        if (isDialogueActive) return;
        
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        
        // Pause game
        if (GameManager.Gary != null)
        {
            GameManager.Gary.state = GameState.Ready;
            Debug.Log("Dialogue started - GameState set to Ready");
        }
        
        dialogueBox.SetActive(true);
        ShowNextLine();
    }
    
    private void ShowNextLine()
    {
        Debug.Log($"ShowNextLine called. currentLineIndex: {currentLineIndex}, total lines: {currentDialogue?.lines.Length}");
        
        if (currentLineIndex < currentDialogue.lines.Length)
        {
            hasSkippedCurrentLine = false; 
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.text = currentDialogue.lines[currentLineIndex].text;
            currentLineIndex++;

            if (nextButton != null)
            {
                nextButton.interactable = false;
                Debug.Log($"Next button set to NOT interactable. Button is null? {nextButton == null}, enabled? {nextButton.enabled}");
            }
        }
        else
        {
            EndDialogue();
        }
    }
    
    private void OnNextButtonClicked()
    {
        Debug.Log($"!!! OnNextButtonClicked called !!! Instance == this? {Instance == this}");
        ShowNextLine();
    }
    
    private void OnTextComplete()
    {
        Debug.Log($"Text complete - setting next button to interactable. Button is null? {nextButton == null}");
        if (nextButton != null)
        {
            nextButton.interactable = true;
            Debug.Log($"Button NOW interactable! enabled={nextButton.enabled}, activeInHierarchy={nextButton.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("Cannot set button to interactable - button is NULL!");
        }
    }
    
    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueBox.SetActive(false);
        
        // Resume game
        if (GameManager.Gary != null)
        {
            GameManager.Gary.state = GameState.Playing;
        }
    }
    
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}