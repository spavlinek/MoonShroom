using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public enum GameState {Ready, Menu, Playing, GameOver}

public class GameManager : MonoBehaviour
{
    // Game Manager Singleton
    public static GameManager Gary;

    public GameState state = GameState.Menu;

    // Game variables
    private int livesRemaining;
    private int score;
    private int levelStartScore;  // Score at the beginning of the current level
    public int LIVES_AT_START = 3;

    // UI variables
    public TextMeshProUGUI messageOverlay;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI scoreText;
    
    private GameObject player;
    public Vector3 playerSpawnPoint;

    private static Vector3 savedCheckpointPosition = Vector3.zero;
    private static bool hasCheckpoint = false;
    
    // Prevent multiple death calls
    private bool isHandlingDeath = false;

    [Header("Game Over UI")]
    public Image blackBackground;          
    public GameObject gameOverPanel;        // Panel containing buttons and "GAME OVER" text
    
    [Header("Victory UI")]
    public TextMeshProUGUI victoryText;     // Text that shows "VICTORY!" and final score
    public GameObject restartLevelButton;   // Button to restart level (hide on victory)
    public GameObject gameOverText;         // "GAME OVER" text (hide on victory)
    
    void Awake()
    {
        Debug.Log($"=== GAMEMANAGER AWAKE === GameObject: {gameObject.name}, Gary is null? {Gary == null}");
        Debug.Log($"Parent of {gameObject.name}: {(transform.parent != null ? transform.parent.name : "NULL (root object)")}");
        
        // Check for an existing singleton
        if (Gary != null)
        {
            Debug.LogWarning($"Gary already exists! Copying UI references and destroying duplicate {gameObject.name}");
            
            // Copy UI references from this new instance to the persistent one
            Gary.blackBackground = this.blackBackground;
            Gary.gameOverPanel = this.gameOverPanel;
            
            // Destroy the duplicate
            Destroy(gameObject);
            return;
        }
        
        // This is the first instance - make it the singleton
        Debug.Log("Setting Gary = this and calling DontDestroyOnLoad");
        
        // CRITICAL: Make sure this GameObject is a root object (not a child)
        // If it's a child, detach it from its parent first
        if (transform.parent != null)
        {
            Debug.LogWarning($"GameManager was a child of {transform.parent.name}. Detaching to make it a root object!");
            transform.SetParent(null);
        }
        
        // IMPORTANT: Detach any UI children before DontDestroyOnLoad
        // UI elements should stay in the scene's Canvas, not move to DontDestroyOnLoad
        if (transform.childCount > 0)
        {
            Debug.LogWarning($"GameManager has {transform.childCount} children. Detaching them so they stay in the scene!");
            // Store children in array first (can't modify collection while iterating)
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }
            // Now detach them
            foreach (Transform child in children)
            {
                child.SetParent(null);
                Debug.Log($"Detached {child.name} from GameManager");
            }
        }
        
        Gary = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"GameManager is now persistent: {gameObject.name}");
        Debug.Log($"About to exit Awake(). Gary = {Gary.gameObject.name}");
    }
    
    void OnEnable()
    {
        Debug.Log($"=== GAMEMANAGER OnEnable === GameObject: {gameObject.name}");
    }
    
    void OnDisable()
    {
        Debug.LogError($"!!! GAMEMANAGER OnDisable !!! GameObject: {gameObject.name}");
    }

    void Start()
    {
        Debug.Log($"=== GAMEMANAGER START === This is Gary? {Gary == this}, GameObject: {gameObject.name}");
        
        // Safety check: if this is a duplicate that hasn't been destroyed yet, skip
        if (Gary != this)
        {
            Debug.LogWarning("This is not Gary in Start(), skipping Start logic");
            return;
        }
        
        // Hide game over UI at start
        if (blackBackground != null)
        {
            Color color = blackBackground.color;
            color.a = 0f;
            blackBackground.color = color;
            blackBackground.gameObject.SetActive(false);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        Debug.Log("GameManager Start() completed successfully");
    }

    void OnDestroy()
    {
        Debug.LogError($"!!! GAMEMANAGER IS BEING DESTROYED !!! GameObject: {gameObject.name}, Was this Gary? {Gary == this}");
        Debug.LogError($"Stack trace: {System.Environment.StackTrace}");
    }

    // Called by LevelManager when a level starts (every time scene loads)
    public void LevelStarted()
    {
        Debug.Log("Level Started called");
        SoundManager.Steve.StartTheMusic();
        
        // Initialize lives if this is the first time
        if (livesRemaining == 0)
        {
            livesRemaining = LIVES_AT_START;
        }
        // Don't reset score - let it accumulate across levels
        // But save the starting score for this level (in case player dies)
        levelStartScore = score;

        // Re-find UI elements
        messageOverlay = GameObject.Find("Level text")?.GetComponent<TextMeshProUGUI>();
        livesText = GameObject.Find("Lives Text")?.GetComponent<TextMeshProUGUI>();
        scoreText = GameObject.Find("Score text")?.GetComponent<TextMeshProUGUI>();
        
        // Re-find Game Over UI elements (they should be ACTIVE in the scene at start)
        blackBackground = GameObject.Find("BlackBackground")?.GetComponent<Image>();
        gameOverPanel = GameObject.Find("GameOverPanel");
        
        // Find individual UI elements within the panel
        if (gameOverPanel != null)
        {
            gameOverText = GameObject.Find("GameOverText");
            restartLevelButton = GameObject.Find("RestartButton");
            victoryText = GameObject.Find("VictoryText")?.GetComponent<TextMeshProUGUI>();
        }
        
        Debug.Log($"UI References found - BlackBackground: {blackBackground != null}, GameOverPanel: {gameOverPanel != null}");
        
        // Now HIDE the game over UI (we found it while active, now deactivate it)
        if (blackBackground != null)
        {
            blackBackground.gameObject.SetActive(false);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        player = GameObject.FindGameObjectWithTag("Player");
        
        // If we have a saved checkpoint, use it; otherwise use player's starting position
        if (hasCheckpoint && player != null)
        {
            playerSpawnPoint = savedCheckpointPosition;
            player.transform.position = savedCheckpointPosition;
            Debug.Log("Restored checkpoint: " + savedCheckpointPosition);
        }
        else if (player != null)
        {
            playerSpawnPoint = player.transform.position;
            Debug.Log("Player spawn point set to: " + playerSpawnPoint);
        }
        
        // Reset the death flag when level starts
        isHandlingDeath = false;
        
        
        StartANewGame();
    }

    void Update()
    {
        // Press R to restart at any time
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    private void StartANewGame()
    {
        state = GameState.Ready;



        UpdateLivesDisplay();
        UpdateScoreDisplay();

        isHandlingDeath = false;

        if (messageOverlay)
        {
            messageOverlay.enabled = true;
            messageOverlay.text = LevelManager.GetLevelDisplayName(LevelManager.Larry.currentLevel);
        }

        SoundManager.Steve.StartTheMusic();

        StartCoroutine(ReadyStateCountdown());
    }
    
    private IEnumerator ReadyStateCountdown()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);
        
        // Transition to Playing state
        state = GameState.Playing;
        
        // Hide the message overlay
        if (messageOverlay)
        {
            messageOverlay.enabled = false;
        }
    }

    public void SetPlayerSpawnPoint(Vector3 spawnPoint)
    {
        playerSpawnPoint = spawnPoint;
        savedCheckpointPosition = spawnPoint;
        hasCheckpoint = true;
        Debug.Log("Checkpoint saved: " + spawnPoint);
    }

    private void UpdateLivesDisplay()
    {
        if (livesText)
        {
            livesText.enabled = true;
            livesText.text = "Lives: " + livesRemaining.ToString();
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText)
        {
            scoreText.enabled = true;
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void AddToScore(int points)
    {
        score += points;
        UpdateScoreDisplay();
    }

    public void AddLife()
    {
        livesRemaining += 1;
        UpdateLivesDisplay();
    }

    public void PlayerDied()
    {
        // Prevent multiple death calls
        if (isHandlingDeath)
        {
            Debug.Log("Already handling death - ignoring duplicate call");
            return;
        }
        
        isHandlingDeath = true;
        
        // Play death sound
        SoundManager.Steve?.MakePlayerDeathSound();

        // Lose a life
        Debug.Log("Lives remaining minus 1");
        livesRemaining--;
        
        // Reset score to what it was at the start of this level (not 0)
        score = levelStartScore;
        UpdateLivesDisplay();
        UpdateScoreDisplay();

        
        // Destroy the player
        // if (player != null)
        // {
        //     Destroy(player);
        //     player = null;
        // }

        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        state = GameState.GameOver; // Pause game temporarily

        if (messageOverlay)
        {
            messageOverlay.enabled = true;
            messageOverlay.text = "Oops!";
        }

        yield return new WaitForSeconds(2f);

        if (livesRemaining > 0)
        {
            // Reload scene - everything resets but checkpoint is saved
            LevelManager.Larry.ReloadLevel();
        }
        else
        {
            // Game over - clear checkpoint
            hasCheckpoint = false;
            savedCheckpointPosition = Vector3.zero;
            StartCoroutine(GameOverState());
        }
    }

    private void RespawnPlayer()
    {
        if (player != null)
        {
            // Respawn at the saved spawn point (either start or checkpoint)
            player.transform.position = playerSpawnPoint;

            // Snap camera to the respawn position
            CameraScript camera = Camera.main?.GetComponent<CameraScript>();
            if (camera != null)
            {
                camera.SnapToPlayer();
            }
            
            // Reset player state
            CharacterController2D controller = player.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.ResetAfterDeath();
            }
        }
        else
        {
            // Player was destroyed, reload the scene
            LevelManager.Larry.ReloadLevel();
        }
    }


    private IEnumerator GameOverState()
    {
        state = GameState.GameOver;
        
        // Show "Game Over!" text briefly
        if (messageOverlay)
        {
            messageOverlay.enabled = true;
            messageOverlay.text = "Game Over!";
        }

        SoundManager.Steve?.StopTheMusic();

        yield return new WaitForSeconds(2f);
        
        // Hide the temporary message
        if (messageOverlay)
        {
            messageOverlay.enabled = false;
        }
        
        // Fade to black with circular effect
        yield return StartCoroutine(FadeToBlack(1f));
        
        // Show Game Over panel with buttons
        ShowGameOverScreen();
    }
    
    private void ShowGameOverScreen()
    {
        // Show the panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // SHOW the "Game Over" text
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }
        
        // SHOW the "Restart Level" button
        if (restartLevelButton != null)
        {
            restartLevelButton.SetActive(true);
        }
        
        // HIDE victory text
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(false);
        }
        
        // Both menu and restart buttons are visible for game over
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (blackBackground == null) yield break;
        
        blackBackground.gameObject.SetActive(true);
        
        float elapsed = 0f;
        Color color = blackBackground.color;
        color.a = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            // Circular fade: starts slow, speeds up, ends slow
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            color.a = smoothT;
            blackBackground.color = color;
            
            yield return null;
        }
        
        // Ensure fully black
        color.a = 1f;
        blackBackground.color = color;
    }
    

    public void LevelComplete()
    {
        StartCoroutine(LevelCompleteState());
    }

    private IEnumerator LevelCompleteState()
    {
        state = GameState.GameOver; // Pause game

        SoundManager.Steve.MakeEndOfLevelSound();
        
        if (messageOverlay)
        {
            messageOverlay.enabled = true;
            messageOverlay.text = "Level Cleared!";
        }

        yield return new WaitForSeconds(3f);
    
        // Clear checkpoint when moving to next level
        hasCheckpoint = false;
        savedCheckpointPosition = Vector3.zero;
        
        // Load the next level
        LevelManager.Larry.LoadNextLevel();
    }
    
    public void RestartLevel()
    {
        Debug.Log("Restarting current level...");
        
        // Reset lives for a fresh restart
        livesRemaining = LIVES_AT_START;
        
        // Check what level we're on
        int currentLevel = LevelManager.Larry.currentLevel;
        
        if (currentLevel == 1)
        {
            // Level 1 restart - reset everything to 0
            score = 300;
            levelStartScore = 300;
        }
        else
        {
            // Level 2+ restart - only reset to the score at start of this level
            score = levelStartScore;
        }
        
        // Clear checkpoint on restart
        hasCheckpoint = false;
        savedCheckpointPosition = Vector3.zero;

        // Reload the current scene
        LevelManager.Larry.ReloadLevel();
        SoundManager.Steve.StartTheMusic();
    }

    public void RestartFromBeginning()
    {
        Debug.Log("Restarting game from Level 1...");
        
        // Full game restart - reset everything
        livesRemaining = LIVES_AT_START;
        score = 0;
        levelStartScore = 0;
        
        // Clear checkpoint
        hasCheckpoint = false;
        savedCheckpointPosition = Vector3.zero;
        
        // Load Level 1
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        SoundManager.Steve.StartTheMusic();
    }

    public void BossDefeated()
    {
        StartCoroutine(BossVictoryState());
    }

    private IEnumerator BossVictoryState()
    {
        state = GameState.GameOver; // Pause game
        
        SoundManager.Steve?.StopTheMusic();
        
        // Stop the boss music
        StopBossMusic();
        
        // Hide all boss UI elements (health bar, name labels, etc.)
        HideBossUI();
        
        // Wait a moment for the boss death animation
        yield return new WaitForSeconds(2f);
        
        // STEP 1: Fade to black FIRST
        yield return StartCoroutine(FadeToBlack(1f));
        
        // STEP 2: Show victory dialogue ON TOP of black background
        if (DialogueManager.Instance != null)
        {
            DialogueData victoryDialogue = ScriptableObject.CreateInstance<DialogueData>();
            victoryDialogue.lines = new DialogueData.DialogueLine[1];
            victoryDialogue.lines[0] = new DialogueData.DialogueLine { text = "Astro1, this is EarthCommand. Do you copy?" };
            
            DialogueManager.Instance.StartDialogue(victoryDialogue);
            
            // Wait for dialogue to finish
            while (DialogueManager.Instance.IsDialogueActive())
            {
                yield return null;
            }
        }
        
        // STEP 3: Show victory screen
        ShowVictoryScreen();
    }
    
    private void ShowVictoryScreen()
    {
        // Show the panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // HIDE the "Game Over" text
        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }
        
        // HIDE the "Restart Level" button
        if (restartLevelButton != null)
        {
            restartLevelButton.SetActive(false);
        }
        
        // SHOW victory text with final score
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(true);
            victoryText.text = "VICTORY!\nFinal Score: " + score;
        }
        else
        {
            // Fallback to message overlay if victoryText not found
            if (messageOverlay != null)
            {
                messageOverlay.enabled = true;
                messageOverlay.text = "VICTORY!\nFinal Score: " + score;
            }
        }
        
        // Menu button remains visible and active
    }

    public int GetCurrentScore()
    {
        return score;
    }
    
    private void StopBossMusic()
    {
        // Find and stop the BossMusic GameObject
        GameObject bossMusic = GameObject.Find("BossMusic");
        if (bossMusic != null)
        {
            AudioSource audioSource = bossMusic.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Stop();
                Debug.Log("Boss music stopped");
            }
            // Optionally deactivate the entire GameObject
            bossMusic.SetActive(false);
        }
    }
    
    private void HideBossUI()
    {
        // Find and hide common boss UI elements
        GameObject bossHealthBar = GameObject.Find("BossHealth");
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(false);
        }
        
        GameObject bossNameLabel = GameObject.Find("Boss text");
        if (bossNameLabel != null)
        {
            bossNameLabel.SetActive(false);
        }
        
    }
}
