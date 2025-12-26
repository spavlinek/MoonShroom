using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Steve;
    
    [Header("Music")]
    public AudioSource backgroundMusic;
    
    [Header("Player Sounds")]
    public AudioClip jumpSound;
    public AudioClip playerDeathSound;
    public AudioClip coinSound;
    public AudioClip endOfLevelSound;
    public AudioClip shootSound;

    [Header("Boss Sounds")]
    public AudioClip bossRoarSound;

    [Header("UI Sounds")]
    public AudioClip typingSound;
    
    private AudioSource thisAudio;
    
    private void Awake()
    {
        // Check for an existing singleton
        if (Steve)
        {
            // Singleton exists - destroy this duplicate
            Destroy(this.gameObject);
        }
        else
        {
            // No singleton exists - set it up
            Steve = this;
            DontDestroyOnLoad(this.gameObject);
        }
        
        thisAudio = GetComponent<AudioSource>();

        // Start background music with error checking
        if (backgroundMusic != null)
        {
            if (backgroundMusic.clip != null)
            {
                backgroundMusic.loop = true;
                backgroundMusic.spatialBlend = 0f; // Make it 2D
                backgroundMusic.Play();
                Debug.Log("Background music started successfully!");
            }
            else
            {
                Debug.LogError("BackgroundMusic AudioSource has NO AUDIO CLIP assigned! Please assign a music file in the Inspector.");
            }
        }
        else
        {
            Debug.LogError("BackgroundMusic AudioSource reference is NULL! Please assign it in the SoundManager Inspector.");
        }
    }
    
    // Music controls
    public void StartTheMusic()
    {
        if (backgroundMusic && backgroundMusic.clip != null)
        {
            backgroundMusic.Play();
        }
    }
    
    public void StopTheMusic()
    {
        if (backgroundMusic)
        {
            backgroundMusic.Stop();
        }
    }

    public void MakeCoinSound()
    {
        if (coinSound)
        {
            thisAudio.PlayOneShot(coinSound);
        }
    }

    // Player sounds
    public void MakeJumpSound()
    {
        if (jumpSound)
        {
            thisAudio.PlayOneShot(jumpSound);
        }
    }
    
    public void MakeShootSound()
    {
        if (shootSound)
        {
            thisAudio.PlayOneShot(shootSound);
        }
    }

    public void MakePlayerDeathSound()
    {
        StopTheMusic();

        if (playerDeathSound)
        {
            thisAudio.PlayOneShot(playerDeathSound);
        }
    }
    
    public void MakeEndOfLevelSound()
    {
        StopTheMusic();
        
        if (endOfLevelSound)
        {
            thisAudio.PlayOneShot(endOfLevelSound);
        }
    }

    public void MakeTypingSound()
    {
        if (typingSound)
        {
            thisAudio.PlayOneShot(typingSound);
        }
    }

    public void MakeBossRoarSound()
    {
        if (bossRoarSound)
        {
            thisAudio.PlayOneShot(bossRoarSound);
        }
    }
}