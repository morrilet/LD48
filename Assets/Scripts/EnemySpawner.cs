using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;

    // TODO: Consider some sort of distance-based spawn. That way spawners won't trigger
    //       unles they're enabled and near the player.

    bool shouldSpawn;

    private void Update() {
        if (shouldSpawn) {
            GameObject.Instantiate(enemyPrefab, transform.position, Quaternion.identity, null);
            shouldSpawn = false;
        }
    }

    public void Spawn() {
        // When we spawn from the level generator we call it too early. This lets us wait until we've instantiated.
        shouldSpawn = true;
    }
}
