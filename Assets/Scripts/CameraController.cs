using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController target;
    public Vector2 targetPos;
    public float distance = -10f;
    public float smoothTime = 0.5f;
    public float yOffset = 0f;
    public float forwardOffset = 0f;

    private Vector3 currentVel;

    private void LateUpdate()
    {
        if (target == null) return;

        if (target.isHooked)
            targetPos = target.currentHook.transform.position;
        else
        {
            targetPos.x = target.transform.position.x + target.rb2d.velocity.x * forwardOffset;

            if (target.isOnFloor )
                targetPos.y = target.transform.position.y + yOffset;
        }
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(targetPos.x, targetPos.y , distance), ref currentVel, smoothTime);
    }
}
