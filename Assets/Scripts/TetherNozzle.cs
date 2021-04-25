using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherNozzle : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float tetherDistance;

    void Update()
    {
        Vector3 newPos = transform.position;

        newPos.x = Mathf.Lerp(newPos.x, target.position.x, Time.deltaTime);
        if (Mathf.Abs(newPos.y - target.position.y) > tetherDistance)
            newPos.y = target.position.y + tetherDistance;
        
        transform.position = newPos;
    }
}
