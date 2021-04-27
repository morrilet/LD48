using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] Transform target;

    [Space]

    [SerializeField] GameObject[] levelChunkPrefabs;
    [SerializeField] int chunkCount;  // How many level chunks are spawned in at any given time.
    [SerializeField] float chunkHeight;  // How tall each chunk is. This is our spawn interval.
    [SerializeField] float initialOffset;  // The y-position to start spawning chunks from.

    Queue<GameObject> chunks;
    int totalSpawnedChunks;

    float targetDistanceSinceLastChunk;
    float initialPositionTarget;

    private void Awake() {
        chunks = new Queue<GameObject>();
    }

    private void Start() {
        for (int i = 0; i < chunkCount; i++)
            SpawnChunk();

        initialPositionTarget = target.position.y;
    }
    // if distance % interval => Add new chunk to queue and pop oldest.

    private void Update() {
        int newChunksSpawned = totalSpawnedChunks - chunkCount;  // How many chunks we've spawned, minus the initial ones.
        targetDistanceSinceLastChunk = GameManager.instance.rawPlayerDepth - (newChunksSpawned * chunkHeight);

        if (targetDistanceSinceLastChunk >= chunkHeight) {
            SpawnChunk();
        }
    }

    private void SpawnChunk() {
        Vector3 position = transform.position;
        position.y -= (totalSpawnedChunks * chunkHeight) + initialOffset;

        GameObject newChunk = Instantiate(GetRandomChunk(), position, Quaternion.identity, this.transform);
        chunks.Enqueue(newChunk);

        // If we're over the chunk count remove the oldest chunk.
        if (chunks.Count > chunkCount) {
            GameObject oldestChunk = chunks.Dequeue();
            GameObject.Destroy(oldestChunk);
        }

        // Spawn enemies in the new chunk
        SpawnEnemies(newChunk);

        // Spawn spikes in the new chunk
        SpawnSpikes(newChunk);

        totalSpawnedChunks++;
    }

    private void SpawnEnemies(GameObject chunk) {
        EnemySpawner[] spawners = chunk.GetComponentsInChildren<EnemySpawner>();
        
        // Decide how many enemies to spawn based on player depth.
        int maxSpawnersTriggered = 0;
        if (GameManager.instance.playerDepth > GlobalVariables.TIER_2_DEPTH) 
            maxSpawnersTriggered = 6;
        if (GameManager.instance.playerDepth > GlobalVariables.TIER_1_DEPTH) 
            maxSpawnersTriggered = 4;
        if (GameManager.instance.playerDepth > GlobalVariables.TIER_0_DEPTH) 
            maxSpawnersTriggered = 2;

        // If we don't have enough spawners then just use them all.
        if (maxSpawnersTriggered >= spawners.Length) {
            foreach (EnemySpawner spawner in spawners) {
                spawner.Spawn();
            }
        } else {
            // Shuffle the spawner list.
            System.Random random = new System.Random();
            spawners = spawners.OrderBy(item => random.Next()).ToArray();

            // Spawn from the first however many.
            for (int i = 0; i < maxSpawnersTriggered; i++) {
                spawners[i].Spawn();
            }
        }
    }

    private void SpawnSpikes(GameObject chunk) {
        TetherCutter[] spikes = chunk.GetComponentsInChildren<TetherCutter>();
        
        // Decide how many enemies to allow based on player depth.
        int maxSpikesAllowed = 0;
        if (GameManager.instance.playerDepth > GlobalVariables.TIER_2_DEPTH) 
            maxSpikesAllowed = 3;
        if (GameManager.instance.playerDepth > GlobalVariables.TIER_1_DEPTH) 
            maxSpikesAllowed = 2;
        if (GameManager.instance.playerDepth > GlobalVariables.TIER_0_DEPTH) 
            maxSpikesAllowed = 1;

        if (maxSpikesAllowed >= spikes.Length) {
            foreach (TetherCutter spike in spikes) {
                spike.gameObject.SetActive(true);
            }
        } else {
            // Shuffle the spike list.
            System.Random random = new System.Random();
            spikes = spikes.OrderBy(item => random.Next()).ToArray();

            // Set active on the first however many spikes to true, false on the others.
            for (int i = 0; i < spikes.Length; i++) {
                spikes[i].gameObject.SetActive(i < maxSpikesAllowed);
            }
        }
    }

    private GameObject GetRandomChunk() {
        int index = Random.Range(0, levelChunkPrefabs.Length - 1);
        return levelChunkPrefabs[index];
    }
}
