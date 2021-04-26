using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FishAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform target;
    [SerializeField] GameObject spriteObj;
    [SerializeField] LayerMask sightLayerMask;
    [SerializeField] Animator animator;

    [Space, Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float windUpDuration;  // How long we should spend winding up for an attack.
    [SerializeField] Vector2 maxPositionDifferenceForAttack;  // How much higher, lower, left, or right we can be than the target before attacking.
    [SerializeField] float movementThreshold;  // How precise to be with our positioning.
    [SerializeField] float minLateralDistanceFromTarget;  // How far to keep away from the target along. the X axis.
    [SerializeField] float attackForce;  // How much force to apply to the rigidbody when we attack.
    [SerializeField] float attackCooldown;  // How long to wait after each attack before trying to attack again.


    [Space, Header("Debugging")]
    [SerializeField] bool debug;

    Vector2 directionToTarget;
    Rigidbody2D localRigidbody;
    Vector3 nextPositionTarget;  // The position we're currently navigating towards.
    bool isAttacking;

    private void Awake() {
        localRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (target == null)
            return;

        directionToTarget = target.position - transform.position;  

        if (!isAttacking) {
            RotateTowardsTarget();
            MoveToTarget();

            // If we're at our destination we should begin an attack.
            if ((nextPositionTarget - transform.position).magnitude <= movementThreshold) {
                StartCoroutine(PerformAttack());
            }
        }
    }

    private void FixedUpdate() {
        // localRigidbody.velocity = directionToTarget.normalized * moveSpeed; 
    }

    private bool CanAttackFromPosition(Vector3 position) { 
        
        // Can we see the target from the proposed position?
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position, 0.05f, target.position - position, Mathf.Infinity, sightLayerMask);
        RaycastHit2D[] orderedHits = hits.Where(hit => hit.collider != null).OrderBy(hit => hit.distance).ToArray();

        if (hits.Length != 0)
            Debug.DrawLine(position, hits[0].point, Color.red);

        if (hits.Length == 0 || hits[0].collider.gameObject != target.gameObject)
            return false;

        // Is the proposed position within the limits of the attack box?
        float[] limits = GetAttackLimits();
        float topLimit = limits[0];
        float bottomLimit = limits[1];
        float leftLimit = limits[2];
        float rightLimit = limits[3];
        
        if (position.x > rightLimit || position.x < leftLimit) 
            return false;
        if (position.y > topLimit || position.y < bottomLimit)
            return false;

        // If we've made it this far the position is good.
        return true;
    }

    private IEnumerator PerformAttack() {
        // Set attacking bool, wind up for duration, then launch towards the 
        // target and unset the attacking bool when we're finished.
        isAttacking = true;
        animator.SetTrigger("Windup");
        localRigidbody.velocity = Vector2.zero;

        Vector2 targetPosition = target.position; // Cache this at the start of the attack.

        // TODO: Force windup time to stretch to fit windup duration.
        yield return new WaitForSeconds(windUpDuration);

        // TODO: Maybe disable main collider when attacking so we can pass through the tether...
        Vector2 force = ((Vector2)targetPosition - (Vector2)transform.position).normalized * attackForce;
        localRigidbody.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    private void MoveToTarget() {
        // Pick a point near the target that we can see. It should be on the same side of the target
        // as we already are so we don't have to cross the tether. The point should also be within
        // the height differential and not *too* close to the x-position of the target.

        // If the current target is an acceptable attack position, move towards it.
        if (CanAttackFromPosition(nextPositionTarget)) {
            localRigidbody.velocity = (nextPositionTarget - transform.position).normalized * moveSpeed;
            if (debug) {
                Debug.DrawLine(transform.position, nextPositionTarget, Color.blue);
            }
            return;
        }

        // If the current target is not an acceptable attack position, pick a new one.
        // Do this by creating a box around the target using our attack parameters and 
        // picking a random point in that box. We'll see if it's a good pick in the next frame.

        // Get the limits of the box.
        float[] limits = GetAttackLimits();
        float topLimit = limits[0];
        float bottomLimit = limits[1];
        float leftLimit = limits[2];
        float rightLimit = limits[3];

        // Pick a random point in the box.
        nextPositionTarget = new Vector3(
            Random.Range(leftLimit, rightLimit),
            Random.Range(bottomLimit, topLimit),
            0.0f
        );

        // Draw the box if we're debugging.
        if (debug) {
            Debug.DrawLine(new Vector3(leftLimit, topLimit), new Vector3(rightLimit, topLimit), Color.red);  // Top border.
            Debug.DrawLine(new Vector3(leftLimit, bottomLimit), new Vector3(rightLimit, bottomLimit), Color.blue);  // Bottom border.
            Debug.DrawLine(new Vector3(leftLimit, topLimit), new Vector3(leftLimit, bottomLimit), Color.yellow);  // Left border.
            Debug.DrawLine(new Vector3(rightLimit, topLimit), new Vector3(rightLimit, bottomLimit), Color.green);  // Right border.
        }
    }

    private float[] GetAttackLimits() {
        float topLimit = target.position.y + maxPositionDifferenceForAttack.y;
        float bottomLimit = target.position.y - maxPositionDifferenceForAttack.y;
        float leftLimit = directionToTarget.x < 0 ? target.position.x + minLateralDistanceFromTarget : target.position.x - maxPositionDifferenceForAttack.x;
        float rightLimit = directionToTarget.x > 0 ? target.position.x - minLateralDistanceFromTarget : target.position.x + maxPositionDifferenceForAttack.x;
        return new float[4]{ topLimit, bottomLimit, leftLimit, rightLimit };
    }

    private void RotateTowardsTarget() {

        // TODO: Align body with target up to a maximum allowable angle.

        // Flip the sprite around if we need to face the target.
        Vector3 scale = spriteObj.transform.localScale;
        scale.x = -Mathf.Sign(directionToTarget.x);
        spriteObj.transform.localScale = scale;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(nextPositionTarget, 0.25f);
    }
}
