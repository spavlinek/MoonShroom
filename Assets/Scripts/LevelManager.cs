using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Larry;
    public int currentLevel = 1;
    
    [System.Serializable]
    public class LevelDialogueData
    {
        public int levelNumber;
        public DialogueData dialogue;
    }
    
    [Header("Level Dialogues")]
    public List<LevelDialogueData> levelDialogues = new List<LevelDialogueData>();
    
    private bool hasShownDialogueThisLevel = false;
    private bool isInitialLoad = true;

    void Awake()
    {
        if (Larry != null)
        {
            Destroy(gameObject); // Destroy the entire GameObject
            return; // Exit immediately, don't run any more code
        }
        
        // This is the first instance - make it the singleton
        Larry = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Safety check: if this is a duplicate that hasn't been destroyed yet, skip
        if (Larry != this)
        {
            return;
        }
        
        
        if (isInitialLoad)
        {
            currentLevel = GetLevelNumberFromScene(SceneManager.GetActiveScene());
            
            GameManager.Gary.LevelStarted();
            StartCoroutine(TriggerDialogueAfterDelay(0.5f));
            
            isInitialLoad = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Safety check
        if (Larry != this) return;
        
        
        if (isInitialLoad)
        {
            return;
        }
        
        int loadedLevel = GetLevelNumberFromScene(scene);
        
        if (loadedLevel != currentLevel)
        {
            currentLevel = loadedLevel;
            hasShownDialogueThisLevel = false;
        }
        else
        {
            Debug.Log($"Same level {currentLevel} reloaded - dialogue flag: {hasShownDialogueThisLevel}");
        }
        
        // Check if GameManager exists before calling it
        if (GameManager.Gary == null)
        {
            Debug.LogError("GameManager.Gary is NULL in OnSceneLoaded! Cannot call LevelStarted()");
        }
        else
        {
            GameManager.Gary.LevelStarted();
        }
        
        StartCoroutine(TriggerDialogueAfterDelay(0.5f));
    }
    
    private IEnumerator TriggerDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerLevelDialogue();
    }
    
    private int GetLevelNumberFromScene(Scene scene)
    {
        string sceneName = scene.name;
        
        // Handle specific scene names
        if (sceneName == "Level1") return 1;
        if (sceneName == "Level2") return 2;
        if (sceneName == "Level3") return 3;
        if (sceneName == "BossLevel") return 4;
        
        if (sceneName.Contains("Level"))
        {
            string numberPart = sceneName.Replace("Level", "").Replace("level", "");
            if (int.TryParse(numberPart, out int levelNum))
            {
                return levelNum;
            }
        }
        
        // Fallback to buildIndex + 1
        return scene.buildIndex + 1;
    }
    
    // Get display name for level (shown to player)
    public static string GetLevelDisplayName(int levelNumber)
    {
        if (levelNumber == 4)
        {
            return "Final Level";
        }
        return "Level " + levelNumber.ToString();
    }
    
    private void TriggerLevelDialogue()
    {        
        if (hasShownDialogueThisLevel)
        {
            Debug.Log($"Dialogue already shown for level {currentLevel}, SKIPPING");
            return;
        }
        
        LevelDialogueData levelDialogue = levelDialogues.Find(ld => ld.levelNumber == currentLevel);
        
        if (levelDialogue != null && levelDialogue.dialogue != null)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(levelDialogue.dialogue);
                hasShownDialogueThisLevel = true;
            }
            else
            {
                Debug.LogWarning("DialogueManager.Instance is null!");
            }
        }
        else
        {
            Debug.Log($"No dialogue configured for level {currentLevel}");
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("All levels completed!");
            SceneManager.LoadScene(0);
        }
    }

    private void OnDestroy()
    {
        // Only clean up if this is the actual singleton
        if (Larry == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Larry = null;
        }
    }
}