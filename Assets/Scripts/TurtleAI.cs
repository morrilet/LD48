using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurtleAI : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float terminalVelocity;
    [SerializeField] float searchDistanceX;
    [SerializeField] float searchDistanceY;
    [SerializeField] float skinWidth;
    [SerializeField] LayerMask sensorMask;
    [SerializeField] Animator animator;
    [SerializeField] float attackDuration;

    [Space, Header("Debugging")]

    [SerializeField] bool debug;

    BoxCollider2D localCollider;
    Rigidbody2D localRigidbody;
    float direction;
    Vector2 velocity;
    bool isAttacking;
    Vector3 startScale;

    private void Awake() {
        localCollider = GetComponent<BoxCollider2D>();
        localRigidbody = GetComponent<Rigidbody2D>();
        direction = Mathf.Sign(Random.Range(-1, 2));  // `Sign()` isn't required here but it gives me some peace of mind.
        startScale = transform.localScale;
    }

    private void Update() {

        if (isAttacking)
            return;

        if (OnEdge() || IsTouchingWall())
            direction = direction *= -1.0f;

        velocity.x = direction * moveSpeed;
        velocity.y = velocity.y + GlobalVariables.GRAVITY;
        velocity.y = Mathf.Clamp(velocity.y, -terminalVelocity, Mathf.Infinity);
        localRigidbody.velocity = velocity;

        // TODO: Add some delay when changing direction.

        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        
        Vector3 scale = startScale;
        scale.x *= direction * -1.0f;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (isAttacking)
            return;

        if (other.gameObject == GameManager.instance.player)
            StartCoroutine(Attack());
    }

    private IEnumerator Attack() {
        localRigidbody.velocity = Vector2.zero;
        animator.SetTrigger("Attack");
        isAttacking = true;

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    private bool OnEdge() {
        float leftExtent = localCollider.bounds.extents.x * -1.0f;
        float rightExtent = localCollider.bounds.extents.x;

        Vector2 origin = localCollider.bounds.center;
        origin.x += direction > 0 ? rightExtent - skinWidth : leftExtent + skinWidth;
        origin.y -= localCollider.bounds.extents.y - skinWidth;
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, searchDistanceY + skinWidth, sensorMask);
        hits = hits.Where(hit => hit.collider != null).OrderBy(hit => hit.distance).ToArray();

        if (debug) {
            Debug.DrawRay(origin, Vector2.down * (searchDistanceY + skinWidth), Color.yellow);
        }

        if (hits.Length == 0)
            return true;
        return false;
    }

    private bool IsTouchingWall() {
        float leftExtent = localCollider.bounds.extents.x * -1.0f;
        float rightExtent = localCollider.bounds.extents.x;

        Vector2 origin = localCollider.bounds.center;
        origin.x += direction > 0 ? rightExtent - skinWidth : leftExtent + skinWidth;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.right * direction, searchDistanceX + skinWidth, sensorMask);
        hits = hits.Where(hit => hit.collider != null).OrderBy(hit => hit.distance).ToArray();

        if (debug) {
            Debug.DrawRay(origin, Vector2.right * direction * (searchDistanceX + skinWidth), Color.yellow);
        }

        if (hits.Length != 0)
            return true;
        return false;
    }
}
