using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(InputManager))]
public class CharacterController_TopDown : MonoBehaviour
{
    [Header("Gameplay")]
    public float maxSpeed;  // The max speed the controller can move while uninhibited.

    [Space, Header("Technical Details")]

    [SerializeField] LayerMask collisionLayers;  // The layers this controller collides with.
    [SerializeField] float skinWidth;  // How much to inset the start of the raycasts so they can't start inside another object.
    [SerializeField, Range(2, 64)] int verticalRaycastCount = 2;  // The number of rays to cast from the top / bottom of the controller.
    [SerializeField, Range(2, 64)] int horizontalRaycastCount = 2;  // The number of rays to case from the left / right of the controller.

    [Space, Header("Debugging")]

    [SerializeField] bool debug;

    Collider2D localCollider;
    InputManager localInputManager;

    private void Awake()
    {
        localCollider = GetComponent<BoxCollider2D>();
        localInputManager = GetComponent<InputManager>();
    }

    private void Update()
    {
        InputManager.InputData input = localInputManager.GetInput();
        TryMoveDirection(input.movementInput);
    }

    // Attempts to move the controller in the given direction. If this 
    // is impossible, move as much as possible in that direction.
    public void TryMoveDirection(Vector2 dir) {

        Vector3 newPos = transform.position;
        Vector2 velocity = (Vector3)dir * maxSpeed;

        (List<RaycastHit2D>, List<RaycastHit2D>) hits = PerformRaycasts(velocity * Time.deltaTime);
        float maxHorizDist = Mathf.Abs(velocity.x);
        float maxVertDist = Mathf.Abs(velocity.y);

        for (int i = 0; i < horizontalRaycastCount; i++)
            if (hits.Item1[i].collider && hits.Item1[i].distance < maxHorizDist)
                maxHorizDist = hits.Item1[i].distance;

        for (int i = 0; i < verticalRaycastCount; i++)
            if (hits.Item2[i].collider && hits.Item2[i].distance < maxVertDist)
                maxVertDist = hits.Item2[i].distance;

        velocity.x = Mathf.Sign(velocity.x) * maxHorizDist;
        velocity.y = Mathf.Sign(velocity.y) * maxVertDist;

        Vector3 oldPos = transform.position;
        transform.position = oldPos + (Vector3)velocity * Time.deltaTime;
    }

    private (List<RaycastHit2D>, List<RaycastHit2D>) PerformRaycasts(Vector2 velocity)
    {
        List<RaycastHit2D> horizHits = new List<RaycastHit2D>();
        List<RaycastHit2D> vertHits = new List<RaycastHit2D>();

        Vector2 position = (Vector2)transform.position + localCollider.offset;
        Vector2 extents = localCollider.bounds.extents;
        Vector2 origin = position;
        Vector2 dir = Vector2.zero;
        float width = (extents.x * 2.0f) - (skinWidth * 2.0f);
        float height = (extents.y * 2.0f) - (skinWidth * 2.0f);
        float leftSide = position.x - extents.x + skinWidth;
        float topSide = position.y + extents.y - skinWidth;

        // Vertical (Top / Bottom) based on speed.
        for (int i = 0; i < verticalRaycastCount; i++)
        {
            dir = velocity.y > 0 ? Vector2.up : Vector2.down;
            origin.x = leftSide + (i * width / (verticalRaycastCount - 1));
            origin.y = velocity.y > 0 ?
                position.y + extents.y - skinWidth :
                position.y - extents.y + skinWidth;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, Mathf.Abs(velocity.y), collisionLayers);
            vertHits.Add(hit);

            if (debug)
            {
                if (hit)
                    Debug.DrawLine(origin + velocity, hit.point, Color.red);
                else
                    Debug.DrawRay(origin + velocity, new Vector2(0.0f, velocity.y), Color.yellow);
            }
        }

        // Horizontal (Left / Right) based on speed.
        for (int i = 0; i < horizontalRaycastCount; i++)
        {
            dir = velocity.x > 0 ? Vector2.right : Vector2.left;
            origin.x = velocity.x > 0 ?
                position.x + extents.x - skinWidth :
                position.x - extents.x + skinWidth;
            origin.y = topSide - (i * height / (horizontalRaycastCount - 1));

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, Mathf.Abs(velocity.x), collisionLayers);
            horizHits.Add(hit);

            if (debug)
            {
                if (hit)
                    Debug.DrawLine(origin + velocity, hit.point, Color.red);
                else
                    Debug.DrawRay(origin + velocity, new Vector2(velocity.x, 0.0f), Color.yellow);
            }
        }

        return (horizHits, vertHits);
    }
}
