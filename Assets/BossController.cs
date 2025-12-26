using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [Header("Boss Health")]
    public int maxHealth = 100;
    public bool isAlive = true;
    private int currentHealth;
    public UnityEngine.UI.Slider bossHealthBar;  // Unity UI Slider at top of screen
    public GameObject healthBarUI;

    [Header("Boss Movement")]
    public float walkSpeed = 2f;
    public bool facingRight = true;
    private Rigidbody2D rb;
    private Animator animator;

    public enum BossPhase { Phase1, Phase2}
    private BossPhase currentPhase = BossPhase.Phase1;
    public int phase2HealthThreshold = 50;  // Health % to trigger phase 2
    [Header("Shooting")]
    public GameObject bossBulletPrefab;
    public Transform[] firePoints;
    public float shootInterval = 1.5f;  // Time between shots
    public float phase2ShootInterval = 0.5f;  // Faster in phase 2
    private float lastShootTime = -999f;

    [Header("Area Affectors - Phase 2")]
    public GameObject[] areaEffectorPrefabs;
    public Transform[] affectorSpawnPoints;  // Positions in arena
    public float affectorActiveDuration = 3f;
    public float affectorCycleInterval = 5f;
    public List<GameObject> activeAffectors = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null){
            animator = GetComponentInChildren<Animator>();
        }
        
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // Show health bar
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);
        }
        
        // Start Phase 1
        StartPhase1();
    }
    private void StartPhase1()
    {
        currentPhase = BossPhase.Phase1;
        
        // Start shooting
        StartCoroutine(ShootingCoroutine(shootInterval));
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;  // Already dead
        
        currentHealth -= damage;
        UpdateHealthBar();
        
        // Trigger hurt animation
        animator.SetTrigger("HurtTrigger");
        
        // Check for phase transition
        if (currentHealth <= phase2HealthThreshold && currentPhase == BossPhase.Phase1)
        {
            TransitionToPhase2();
        }
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.value = (float)currentHealth / maxHealth;
        }
    }

    private void TransitionToPhase2()
    {
        currentPhase = BossPhase.Phase2;
        
        // Play roar sound
        if (SoundManager.Steve != null)
        {
            SoundManager.Steve.MakeBossRoarSound();
        }
        
        // Shake the camera
        CameraScript camera = Camera.main?.GetComponent<CameraScript>();
        if (camera != null)
        {
            camera.Shake(0.5f, 0.3f);  // 0.5 seconds duration, 0.3 magnitude
        }
        
        // Speed up shooting
        StopAllCoroutines();
        StartCoroutine(ShootingCoroutine(phase2ShootInterval));
        
        // Activate area affectors
        StartCoroutine(AreaAffectorCoroutine());
    }

    private IEnumerator ShootingCoroutine(float interval)
    {
        while (isAlive)
        {
            yield return new WaitForSeconds(interval);
            
            if (currentPhase == BossPhase.Phase1)
            {
                // Shoot from random fire points
                ShootFromRandomFirePoints(Random.Range(1, 3));
            }
            else if (currentPhase == BossPhase.Phase2)
            {
                // Shoot from ALL fire points
                ShootFromAllFirePoints();
            }
            
            animator.SetTrigger("AttackTrigger");
        }
    }

    private IEnumerator AreaAffectorCoroutine()
    {
        Debug.Log("Area affector coroutine started");
        while (currentPhase == BossPhase.Phase2 && isAlive)
        {
            
            // Activate affectors
            foreach (GameObject affector in activeAffectors)
            {
                if (affector != null)
                    Debug.Log("affector activated");
                    affector.SetActive(true);
            }
            
            yield return new WaitForSeconds(affectorActiveDuration);
            
            // Deactivate affectors
            foreach (GameObject affector in activeAffectors)
            {
                if (affector != null)
                    Debug.Log("affector activated");
                    affector.SetActive(false);
            }
            
            yield return new WaitForSeconds(affectorCycleInterval - affectorActiveDuration);
        }
    }

    private void ShootFromRandomFirePoints(int count)
    {
        // Get random fire points
        List<Transform> shuffled = new List<Transform>(firePoints);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            Transform temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        
        // Shoot from first count fire points
        for (int i = 0; i < Mathf.Min(count, shuffled.Count); i++)
        {
            ShootBullet(shuffled[i]);
        }
    }

    private void ShootFromAllFirePoints()
    {
        foreach (Transform firePoint in firePoints)
        {
            ShootBullet(firePoint);
        }
    }

    private void ShootBullet(Transform firePoint)
    {
        if (bossBulletPrefab == null || firePoint == null) return;
        
        // Calculate direction from boss to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2 direction = Vector2.right;  // Default right
        
        if (player != null)
        {
            direction = (player.transform.position - firePoint.position).normalized;
        }
        
        // Instantiate bullet
        GameObject bullet = Instantiate(bossBulletPrefab, firePoint.position, Quaternion.identity);
        
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * 10f;  // Adjust speed
        }
    }

    void Update()
    {
        // Update walk animation
        float speed = Mathf.Abs(rb.linearVelocityX);
        animator.SetFloat("Speed", speed);
        
    }

    void FixedUpdate()
    {
        // Simple patrol movement (example)
        if (isAlive)
        {
            rb.linearVelocityX = facingRight ? walkSpeed : -walkSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "TurnAround" && isAlive)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        
        // Flip the sprite scale
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1f;
        transform.localScale = currentScale;
    }

    private void Die()
    {
        isAlive = false;
        // Stop all coroutines
        StopAllCoroutines();
        
        // Trigger death animation
        animator.SetTrigger("DeathTrigger");
        
        // Disable movement
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Hide health bar
        if (healthBarUI != null)
            healthBarUI.SetActive(false);
        
        // Disable area affectors
        foreach (GameObject affector in activeAffectors)
        {
            if (affector != null)
                affector.SetActive(false);
        }
        
        // Trigger victory event
        if (GameManager.Gary != null)
        {
            GameManager.Gary.BossDefeated();
        }
        
        // Destroy boss after animation
        Destroy(gameObject, 3f);
    }
}
