using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb2d;
    public LineRenderer hair;

    [Header("Movement")]
    public float maxSpeed = 1f;
    public float accel = 0.5f;
    public float floorDrag = 0.01f;
    public float airDrag = 0.005f;

    [Header("Jump")]
    public Transform floorPoint;
    public LayerMask floorLayer;
    public float floorPointRadius = 0.1f;
    public float jumpForce = 5f;
    public float jumpCd = 0.5f;
    public float fakeGravity = 10f;

    [Header("Hair")]
    public FixedJoint2D rootJoint;
    public SpringJoint2D[] nodeJoints;

    [Header("Readonly")]
    public Vector2 input;

    private bool isOnFloor;
    private Collider2D[] overlapResult = new Collider2D[1];

    private float jumpTimer = 0f;

    private bool hasReleasedJump = true;

    private void Update()
    {
        CheckFloor();
        HandleMovement();
        UpdateHair();
    }

    private void UpdateHair()
    {
        hair.positionCount = 1 + nodeJoints.Length;
        hair.SetPosition(0, rootJoint.transform.localPosition);
        for (int i = 0; i < nodeJoints.Length; i++)
            hair.SetPosition(i + 1, nodeJoints[i].transform.localPosition);
    }

    private void CheckFloor()
    {
        isOnFloor = Physics2D.OverlapCircleNonAlloc(floorPoint.position, floorPointRadius,overlapResult, floorLayer) > 0;
    }

    private void HandleMovement()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool jumpPressed = (input.y > 0.5f || Input.GetButton("Fire1"));
        if (!jumpPressed)
            hasReleasedJump = true;

        rb2d.velocity += new Vector2(input.x * Time.deltaTime * accel, 0f);
        if (Mathf.Abs(rb2d.velocity.x) > maxSpeed) rb2d.velocity = new Vector2(maxSpeed * Mathf.Sign(rb2d.velocity.x), rb2d.velocity.y);

        rb2d.velocity *= new Vector2(isOnFloor ? (1f - floorDrag) : (1f - airDrag) ,1f);
        if ((!jumpPressed || rb2d.velocity.y < 0f) && !isOnFloor)
            rb2d.velocity += new Vector2(0f, -fakeGravity * Time.deltaTime);

        if (jumpTimer <= 0f)
        {
            if (isOnFloor && jumpPressed && hasReleasedJump)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
                jumpTimer = jumpCd;
                hasReleasedJump = false;
            }
        }
        else
            jumpTimer -= Time.deltaTime;

        
    }

    private void OnDrawGizmos()
    {
        if(floorPoint != null)
        {
            Gizmos.color = isOnFloor ? Color.green : Color.red;
            Gizmos.DrawWireSphere(floorPoint.position, floorPointRadius);
        }
    }
}
