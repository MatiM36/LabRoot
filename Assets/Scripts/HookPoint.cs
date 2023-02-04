using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    public DistanceJoint2D distanceJoint;

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
}
