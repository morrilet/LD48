using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherCutter : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other) {

        Debug.Log(this.gameObject.name);

        if (other.gameObject.tag == GlobalVariables.TETHER_TAG) {
            VerletRope tether = other.gameObject.GetComponent<VerletRope>();
            tether.TryCutRope(other.contacts[0].point);
        }
    }
}
