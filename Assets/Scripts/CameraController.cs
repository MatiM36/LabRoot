using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private PlayerController target;
    public Camera camera;
    public Vector2 targetPos;
    public float distance = -10f;
    public float smoothTime = 0.5f;
    public float yOffset = 0f;
    public float forwardOffset = 0f;
    public float maxYDistance = 3f;

    private Vector3 currentVel;

    public void SetTarget(PlayerController player)
    {
        target = player;
    }

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
        float yDistance = (target.transform.position.y - transform.position.y);
        if(Mathf.Abs(yDistance) > maxYDistance)
        {
            transform.position += new Vector3(0f, yDistance - (maxYDistance * Mathf.Sign(yDistance)), 0f);
        }
    }

    public void ForcePosition()
    {
        transform.position = target.transform.position + yOffset * Vector3.up;
    }
}
