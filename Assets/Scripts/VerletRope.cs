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
    [SerializeField] GameObject tetherPrefab;
    [SerializeField] bool cuttable = true;

    LineRenderer lineRenderer;
    EdgeCollider2D edgeCollider;
    List<RopeSegment> ropeSegments = new List<RopeSegment>();
    float segmentLength;

    // Okay, I know nozzle vs. target is confusing. Think of it as start vs. end.

    // Use this for initialization
    void Start()
    {
        bool reverseRopeGeneration = target != null && nozzle == null;
        if (reverseRopeGeneration) {
            nozzle = target;
            target = null;
        }

        this.lineRenderer = this.GetComponent<LineRenderer>();
        edgeCollider = this.GetComponent<EdgeCollider2D>();
        Vector3 ropeStartPoint = nozzle.position;

        segmentLength = ropeLength / segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y = reverseRopeGeneration ? ropeStartPoint.y + segmentLength : ropeStartPoint.y - segmentLength;
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.DrawRope();

        if (averageRopeTarget != null)
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
        if (nozzle != null){
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

    // Can't really do this because the parent rigidbody of the fish means we're always going
    // to see the parent object, not the child, so we can't be sure we're hitting the spear.
    // private void OnCollisionEnter2D(Collision2D other) {
    //     Debug.Log(other.otherCollider.gameObject.tag);
    //     if (other.gameObject.tag == GlobalVariables.FISH_TAG && cuttable) {
    //         if (cuttable) {
    //             CutRope(other.GetContact(0).point);
    //         }
    //     }
    // }

    public void TryCutRope(Vector2 position) {
        // Create two new ropes. They'll each have their nozzle position set but no target.
        // The number of segments to create will depend on the position at which the rope was cut.

        if (!cuttable)
            return;

        int segmentsFromTop = 0;
        for (int i = 0; i < ropeSegments.Count - 1; i++) {
            if (ropeSegments[i].posNow.y > position.y && ropeSegments[i + 1].posNow.y < position.y) {
                segmentsFromTop = i;
                break;
            }
        }

        VerletRope newRopeTop = GameObject.Instantiate(tetherPrefab).GetComponent<VerletRope>();
        newRopeTop.ropeLength = segmentsFromTop * segmentLength;
        newRopeTop.segmentCount = segmentsFromTop;
        newRopeTop.averageRopeTarget = null;
        newRopeTop.cuttable = false;
        newRopeTop.nozzle = nozzle;
        newRopeTop.target = null;

        VerletRope newRopeBottom = GameObject.Instantiate(tetherPrefab).GetComponent<VerletRope>();
        newRopeBottom.ropeLength = ropeLength - (segmentsFromTop * segmentLength);
        newRopeBottom.segmentCount = segmentCount - segmentsFromTop;
        newRopeBottom.averageRopeTarget = null;
        newRopeBottom.cuttable = false;
        newRopeBottom.nozzle = null;
        newRopeBottom.target = target;

        GameObject.Destroy(this.gameObject);
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