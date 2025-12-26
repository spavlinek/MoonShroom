using UnityEngine;
using System.Collections;

public class BoulderSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject boulderPrefab;
    public float spawnInterval = 3f;       // Spawn boulder every X seconds
    public bool startSpawning = true;      // Start spawning on game start
    
    void Start()
    {
        if (startSpawning && boulderPrefab != null)
        {
            StartCoroutine(SpawnBoulders());
        }
    }

    private IEnumerator SpawnBoulders()
    {
        // Wait a bit before first boulder
        yield return new WaitForSeconds(spawnInterval);
        
        while (true)
        {
            SpawnBoulder();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnBoulder()
    {
        if (boulderPrefab != null)
        {
            Instantiate(boulderPrefab, transform.position, Quaternion.identity);
        }
    }
}
