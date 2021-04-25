// Original can be found here. This version has been altered to suit my needs.
// https://github.com/dci05049/Verlet-Rope-Unity/blob/master/Tutorial%20Verlet%20Rope/Assets/Rope.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletRope : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] Transform nozzle;
    [SerializeField] Transform target;
    [SerializeField] Transform averageRopeTarget;  // A transform meant to be kept at the average position of the rope. Used for AI targeting.

    [Space, Header("Rope Properties")]

    [SerializeField] float ropeWidth = 0.1f;
    [SerializeField] int segmentCount = 35;
    [SerializeField] float ropeLength;

    LineRenderer lineRenderer;
    EdgeCollider2D edgeCollider;
    List<RopeSegment> ropeSegments = new List<RopeSegment>();
    float segmentLength;

    // Use this for initialization
    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        edgeCollider = this.GetComponent<EdgeCollider2D>();
        Vector3 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        segmentLength = ropeLength / segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= segmentLength;
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.DrawRope();
        this.UpdateAverageRopeTransform();
    }

    private void FixedUpdate()
    {
        this.Simulate();
    }

    private void UpdateAverageRopeTransform() {
        Vector2 allPoints = Vector2.zero;
        for (int i = 0; i < ropeSegments.Count; i++) {
            allPoints += ropeSegments[i].posNow;
        }
        averageRopeTarget.position = allPoints / ropeSegments.Count;
    }

    private void Simulate()
    {
        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1.5f);

        for (int i = 1; i < this.segmentCount; i++)
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            this.ropeSegments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        if (nozzle != null) {
            RopeSegment firstSegment = this.ropeSegments[0];
            firstSegment.posNow = nozzle.position;
            this.ropeSegments[0] = firstSegment;
        }

        if (target != null) {
            RopeSegment lastSegment = this.ropeSegments[segmentCount - 1];
            lastSegment.posNow = target.position;
            this.ropeSegments[segmentCount - 1] = lastSegment;
        }

        for (int i = 0; i < this.segmentCount - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.segmentLength);
            Vector2 changeDir = Vector2.zero;

            if (dist > segmentLength)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            } else if (dist < segmentLength)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    private void DrawRope()
    {
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;

        Vector3[] ropePositions3D = new Vector3[this.segmentCount];
        Vector2[] ropePositions2D = new Vector2[this.segmentCount];
        for (int i = 0; i < this.segmentCount; i++)
        {
            ropePositions3D[i] = this.ropeSegments[i].posNow;
            ropePositions2D[i] = this.ropeSegments[i].posNow;
        }

        lineRenderer.positionCount = ropePositions3D.Length;
        lineRenderer.SetPositions(ropePositions3D);

        // I'm not certain why the offset tweak is necessary but without 
        // it the edge collider moves far away from the rope.
        edgeCollider.offset = transform.position * -1.0f;  
        edgeCollider.edgeRadius = ropeWidth / 2.0f;
        edgeCollider.points = ropePositions2D;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == GlobalVariables.FISH_TAG) {
            // TODO: Cut the rope.
        }
    }

    private void CutRope() {
        // Create two new ropes. The first will have the nozzle position set, the second will have the target position set.
        // The number of segments to create will depend on the position at which the rope was cut.
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}