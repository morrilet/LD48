using System.Collections;
using System.Collections.Generic;
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

    // TODO: Switch this to the game managers tracked depth.
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
        float distanceFromStartPosition = Mathf.Abs(initialPositionTarget - target.position.y);  // How far the target has moved from the start position in total.
        targetDistanceSinceLastChunk = distanceFromStartPosition - (newChunksSpawned * chunkHeight);
        Debug.Log(targetDistanceSinceLastChunk);

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

        totalSpawnedChunks++;
    }

    private GameObject GetRandomChunk() {
        int index = Random.Range(0, levelChunkPrefabs.Length - 1);
        return levelChunkPrefabs[index];
    }
}
