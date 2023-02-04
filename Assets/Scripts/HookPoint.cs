using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    public DistanceJoint2D distanceJoint;
    public float minDistance = 2f, maxDistance = 3f;

    public void Hook(Rigidbody2D rb2d, float distance)
    {
        distanceJoint.enabled = true;
        distanceJoint.distance = distance;
        distanceJoint.connectedBody = rb2d;
    }

    public void Unhook()
    {
        distanceJoint.enabled = false;
        distanceJoint.connectedBody = null;
    }

    private void OnDrawGizmos()
    {
        var color = Color.green;
        color.a = 0.2f;
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
