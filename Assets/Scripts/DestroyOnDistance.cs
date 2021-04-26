using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnDistance : MonoBehaviour
{
    [SerializeField] float distanceLimit = 15f;

    float distance;

    private void Update() {
        distance = (GameManager.instance.player.transform.position - transform.position).magnitude;
        if (distance >= distanceLimit) {
            GameObject.Destroy(this.gameObject);
        }
    }
}
