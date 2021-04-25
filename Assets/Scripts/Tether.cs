using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tether : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] EdgeCollider2D edgeCollider;
    [SerializeField] GameObject tetherPointPrefab;
    [SerializeField] Transform target;  // The transform to connect the end of the joint array to.
    [SerializeField] Rigidbody2D tetherTarget;  // The tether target object used to track the connection target.
    [SerializeField] Transform tetherOrigin;
    [SerializeField] float tetherDistance;  // How far we keep the tether origin from the player.
    [SerializeField] int jointCount;
    [SerializeField] float tetherLength;

    Transform[] tetherPoints;
    Rigidbody2D endJointRb;
    Rigidbody2D startJointRb;
    FixedJoint2D targetJoint;
    Rigidbody2D localRigidbody;

    private void Awake() {
        localRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        tetherPoints = new Transform[jointCount];
        DistanceJoint2D[] distJoints = new DistanceJoint2D[jointCount];

        GameObject tempObj;
        Rigidbody2D tempRigidbody;
        float distanceBetweenlinks = tetherLength / jointCount;

        for (int i = 0; i < jointCount; i++) {
            float distanceFromOrigin = i * distanceBetweenlinks;
            tempObj = GameObject.Instantiate(
                tetherPointPrefab, 
                tetherOrigin.position + Vector3.down * distanceFromOrigin, 
                Quaternion.identity,
                null
            );

            tetherPoints[i] = tempObj.transform;
            distJoints[i] = tempObj.GetComponent<DistanceJoint2D>();

            if (i > 0) {  // Interior joint. Connect to previous joint.
                tempRigidbody = distJoints[i - 1].GetComponent<Rigidbody2D>();
            } else {  // Start joint. Connect to origin.
                tempRigidbody = tetherOrigin.GetComponent<Rigidbody2D>();
            }
            
            // Do the connecting.
            distJoints[i].connectedBody = tempRigidbody;
            distJoints[i].distance = i == 0 ? 0.005f : distanceBetweenlinks;
            distJoints[i].autoConfigureDistance = i == 0;
        }

        // Set up start / end joints.
        startJointRb = distJoints[0].attachedRigidbody;
        endJointRb = distJoints[jointCount - 1].attachedRigidbody;

        targetJoint = distJoints[jointCount - 1].gameObject.AddComponent<FixedJoint2D>();
        targetJoint.autoConfigureConnectedAnchor = false;
        targetJoint.connectedAnchor = Vector2.zero;
        targetJoint.connectedBody = tetherTarget;
        targetJoint.dampingRatio = 0;

        tetherTarget.transform.parent = null;
    }

    private void Update() {
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, target.position.x, Time.deltaTime);
        if (Mathf.Abs(newPos.y - target.position.y) > tetherDistance)
            newPos.y = target.position.y + tetherDistance;
        localRigidbody.MovePosition(newPos);

        DrawTether();
    }

    private void FixedUpdate() {
        tetherTarget.MovePosition(target.transform.position);
    }

    private void DrawTether() {
        Vector3[] positions3D = new Vector3[jointCount];  // Sloppy.
        Vector2[] positions2D = new Vector2[jointCount];

        for (int i = 0; i < positions3D.Length; i++) {
            positions3D[i] = tetherPoints[i].position;
            positions2D[i] = (Vector2)tetherPoints[i].position;
        }
        lineRenderer.positionCount = jointCount;
        lineRenderer.SetPositions(positions3D);

        edgeCollider.points = positions2D;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (tetherPoints != null)
            for (int i = 0; i < tetherPoints.Length; i++) {
                Gizmos.DrawWireSphere(tetherPoints[i].position, 1f);
            }
    }
}
