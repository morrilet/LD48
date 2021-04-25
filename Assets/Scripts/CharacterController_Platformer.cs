using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(InputManager))]
public class CharacterController_Platformer : MonoBehaviour
{
    [Header("Gameplay")]
    public float maxSpeed;  // The max speed the controller can move on the ground.
    public float maxAirSpeed;  // The max speed the controller can move in the air.
    public float terminalVelocity;  // The max speed the controller can move when falling.
    public float terminalSlideVelocity;  // The max speed the controller can move when wall sliding.
    public float jumpVelocity;  // The velocity applied to the character when the jump button is pressed.
    public AnimationCurve jumpGravityScaleFalloff;  // How forceful gravity is immediately after a jump.
    public float jumpGravityScaleDuration;  // How long the gravity scale is altered after a jump.
    public AnimationCurve slideVelocityScaleFalloff;  // How forceful gravity is while wall-sliding.
    public float slideVelocityScaleDuration;  // How long the gravity scale is altered during a slide.

    [Space, Header("Technical Details")]

    [SerializeField] LayerMask collisionLayers;  // The layers this controller collides with.
    [SerializeField] float skinWidth;  // How much to inset the start of the raycasts so they can't start inside another object.
    [SerializeField, Range(2, 64)] int verticalRaycastCount = 2;  // The number of rays to cast from the top / bottom of the controller.
    [SerializeField, Range(2, 64)] int horizontalRaycastCount = 2;  // The number of rays to case from the left / right of the controller.

    [Space, Header("Animation")]
    [SerializeField] GameObject playerSpriteObj;
    [SerializeField] Animator characterAnimator;

    [Space, Header("Debugging")]

    [SerializeField] bool debug;

    Collider2D localCollider;
    InputManager localInputManager;
    Vector2 storedVelocity;
    (List<RaycastHit2D>, List<RaycastHit2D>) storedHits;
    bool inAir;
    bool jumping;
    float jumpGravityFalloffTimer;
    float slideVelocityFalloffTimer;
    float gravityScale = 1.0f;
    bool touchingWall;
    bool canJump;
    bool wallSliding;
    float wallSlideDirection;  // -1 or 1, depending on the direction we were moving when we initiated the slide.
    float currentTerminalVelocity;
    float currentDirection = 1;

    private void Awake()
    {
        localCollider = GetComponent<BoxCollider2D>();
        localInputManager = GetComponent<InputManager>();

        storedVelocity = Vector2.zero;
        storedHits = PerformRaycasts(storedVelocity);

        currentTerminalVelocity = terminalVelocity;
    }

    private void Update()
    {
        InputManager.InputData input = localInputManager.GetInput();
        Vector2 velocity = Vector2.zero;

# region Specialized Collision Detection
        // Stored velocity used *after* `TryMoveDirection()` is actually the *current* velocity after it has been applied.
        // TODO: We probably need some extra logic here so we don't count as grounded when colliding with the top but still count as grounded when we're falling.
        inAir = true;
        for (int i = 0; i < verticalRaycastCount; i++)
            if (storedHits.Item2[i].collider) {
                inAir = false;
                break;
            }

        touchingWall = false;
        for (int i = 0; i < horizontalRaycastCount; i++)
            if (storedHits.Item1[i].collider) {
                touchingWall = true;
                break;
            }
# endregion

# region Jump State Logic        
        canJump = !inAir && !jumping;

        // Reset `jumping` once we've landed.
        if (!inAir && jumping) {
            jumping = false;
            gravityScale = 1.0f;
        }

        if (jumping) {
            float jumpGravityPercent = Mathf.Clamp01(jumpGravityFalloffTimer / jumpGravityScaleDuration);
            gravityScale = jumpGravityScaleFalloff.Evaluate(jumpGravityPercent);

            jumpGravityFalloffTimer += Time.deltaTime;
            
            if (jumpGravityFalloffTimer >= jumpGravityScaleDuration)
                gravityScale = 1.0f;
        }
# endregion

# region Wall Slide State Logic
        // Initial state stuff. Keep in mind that `touchingWall` will 
        // only ever be true if we're actively pushing into a wall.
        if (touchingWall && !wallSliding && inAir && storedVelocity.y <= 0) {
            wallSliding = true;
            wallSlideDirection = Mathf.Sign(storedVelocity.x);
            slideVelocityFalloffTimer = 0.0f;
            currentTerminalVelocity = terminalSlideVelocity;
        }

        // Stop wall sliding if we're not touching the wall.
        if (!touchingWall && wallSliding)
            wallSliding = false;

        // Stop wall sliding if we touch the ground.
        if (!inAir)
            wallSliding = false;

        if (wallSliding) {
            float slideGravityPercent = Mathf.Clamp01(slideVelocityFalloffTimer / slideVelocityScaleDuration);
            currentTerminalVelocity = Mathf.Lerp(
                terminalSlideVelocity, terminalVelocity,
                slideVelocityScaleFalloff.Evaluate(slideGravityPercent)
            );

            // Apply a small amount of velocity towards the wall to keep `touchingWall` accurate.
            velocity.x = wallSlideDirection * 0.01f;

            slideVelocityFalloffTimer += Time.deltaTime;
            
            if (slideVelocityFalloffTimer >= slideVelocityScaleDuration)
                currentTerminalVelocity = terminalVelocity;
        } else {
            currentTerminalVelocity = terminalVelocity;
        }
# endregion
        
        velocity += TryMoveDirection(input.movementInput.x);
        velocity += TryJump(input.jumpButtonDown);
        velocity += ApplyGravity();

        (List<RaycastHit2D>, List<RaycastHit2D>) hits = PerformRaycasts(velocity * Time.deltaTime);
        transform.position += (Vector3)PerformMove(hits, velocity * Time.deltaTime);

        storedVelocity = velocity;
        storedHits = hits;

        UpdateAnimator();
    }

    private void UpdateAnimator() {
        // Animate the character based on state. Triggers are set elsewhere.
        characterAnimator.SetFloat("Speed", Mathf.Abs(storedVelocity.x));
        characterAnimator.SetBool("InAir", inAir);
        characterAnimator.SetBool("WallSliding", wallSliding);

        // Flip the character based on movement direction.
        if (storedVelocity.x != 0)
            currentDirection = Mathf.Sign(storedVelocity.x);
        Vector3 scale = playerSpriteObj.transform.localScale;
        scale.x = currentDirection;
        playerSpriteObj.transform.localScale = scale;
    }

    // Create a velocity vector containing input-based movement along the Y-axis.
    public Vector2 TryJump(bool jumpButtonDown) {
        Vector2 velocity = Vector2.zero;
        if (jumpButtonDown && canJump) {
            velocity.y = jumpVelocity;

            // State stuff.
            jumping = true;
            jumpGravityFalloffTimer = 0.0f;
            characterAnimator.SetTrigger("Jump");
        }
        return velocity;
    }

    // Create a velocity vector containing input-based movement along the X-axis.
    public Vector2 TryMoveDirection(float xInput) {
        Vector3 newPos = transform.position;
        Vector2 velocity = new Vector2(xInput * (inAir ? maxAirSpeed : maxSpeed), 0.0f);
        return velocity;
    }

    // Create a velocity vector containing gravity. Note that we add to `storedVelocity` here.
    public Vector2 ApplyGravity() {
        Vector2 velocity = new Vector2(
            0.0f, Mathf.Clamp(
                storedVelocity.y + (GlobalVariables.GRAVITY * gravityScale), 
                -currentTerminalVelocity, Mathf.Infinity
            )
        );
        return velocity;
    }

    /// <summary>
    /// Performs the checks to limit player movement along a given velocity based on raycast hits
    /// from `PerformRaycasts`. Returns the velocity the player can actaully move based on collisions.
    /// </summary>
    /// <param name="hits">Hits from `PerformRaycasts()`</param>
    /// <param name="velocity">Desired velocity.</param>
    /// <returns>Velocity adjusted for collisions.</returns>
    private Vector2 PerformMove((List<RaycastHit2D>, List<RaycastHit2D>) hits, Vector2 velocity) {
        float maxHorizDist = Mathf.Abs(velocity.x);
        float maxVertDist = Mathf.Abs(velocity.y);

        for (int i = 0; i < horizontalRaycastCount; i++)
            if (hits.Item1[i].collider && hits.Item1[i].distance - skinWidth < maxHorizDist)
                maxHorizDist = hits.Item1[i].distance - skinWidth;

        for (int i = 0; i < verticalRaycastCount; i++)
            if (hits.Item2[i].collider && hits.Item2[i].distance - skinWidth < maxVertDist)
                maxVertDist = hits.Item2[i].distance - skinWidth;

        // Alter velocity by as much as we can along each input direction.
        velocity.x = Mathf.Sign(velocity.x) * maxHorizDist;
        velocity.y = Mathf.Sign(velocity.y) * maxVertDist;

        return velocity;
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

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, Mathf.Abs(velocity.y) + skinWidth, collisionLayers);
            vertHits.Add(hit);

            if (debug)
            {
                if (hit)
                    Debug.DrawLine(origin, hit.point, Color.red);
                else
                    Debug.DrawRay(origin, new Vector2(0.0f, velocity.y), Color.yellow);
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

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, Mathf.Abs(velocity.x) + skinWidth, collisionLayers);
            horizHits.Add(hit);

            if (debug)
            {
                if (hit)
                    Debug.DrawLine(origin, hit.point, Color.red);
                else
                    Debug.DrawRay(origin, new Vector2(velocity.x, 0.0f), Color.yellow);
            }
        }

        return (horizHits, vertHits);
    }
}
