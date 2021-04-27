using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTracker : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float maxAngleLimit;
    [SerializeField] float minAngleLimit;
    [SerializeField] float restingAngle;

    private void LateUpdate() {
        // Get angle to target.
        float angle = Vector2.SignedAngle(transform.position, target.position);

        LookTowardsAngle(angle);
        Debug.Log(angle);
    }

    private void LookTowardsAngle(float angle) {
        Quaternion goalAngle = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, goalAngle, Time.deltaTime);
    }
}
