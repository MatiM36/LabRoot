using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb2d;
    public LineRenderer hair;
    public SpriteRenderer spriteRend;

    [Header("Movement")]
    public float maxSpeed = 1f;
    public float accel = 0.5f;
    public float floorDrag = 0.01f;
    public float airDrag = 0.005f;
    public float deadzoneValue = 0.1f;

    [Header("Jump")]
    public Transform floorPoint;
    public LayerMask floorLayer;
    public float floorPointRadius = 0.1f;
    public float jumpForce = 5f;
    public float jumpCd = 0.5f;
    public float fakeGravity = 10f;

    [Header("Hair")]
    public Transform[] hairNodes;
    public float attackDistance = 3f;
    public float attackRadius = 0.5f;
    public float attackGrowSpeed = 2f;
    public float attackCd = 1f;
    public float attackWaitTime = 0.5f;
    public LayerMask attackLayer;
    public float rotationMinDistance = 1f;
    public float rotationMaxDistance = 2f;
    public float rotationAccel = 80;
    public float rotationMaxSpeed = 6;
    public float assistAngle = 45;

    [Header("Readonly")]
    public Vector2 input;
    public Vector2 normalizedInput;
    public Vector2 lastInput = new Vector2(1f, 0f);
    public bool facingRight = true;

    private bool isOnFloor;
    private Collider2D[] overlapResult = new Collider2D[10];
    private RaycastHit2D[] raycastResult = new RaycastHit2D[1];

    private float jumpTimer = 0f;

    private bool hasReleasedJump = true;

    private float currentAttackDistance = 0f;
    private Vector2 currentAttackDir = Vector2.zero;
    private bool isUsingHair = false;
    private float hairRecoveryTimer;
    private float hairWaitTimer;

    private bool isHooked = false;
    private HookPoint currentHook = null;

    private void Update()
    {
        CheckFloor();
        HandleMovement();
        UpdateHair();
    }

    private void UpdateHair()
    {
        bool pressedAttack = Input.GetButton("Fire2");


        var hairStartPos = hairNodes[0].position;

        if (isHooked)
        {
            if (!pressedAttack)
            {
                isHooked = false;
                isUsingHair = false;
                hairRecoveryTimer = attackCd;
                currentHook.Unhook();
                currentHook = null;
                hairWaitTimer = 0f;
                Jump();
            }
            else
            {
                var hairLastPos = hairNodes[hairNodes.Length - 1].position = currentHook.transform.position;
                for (int i = 1; i < hairNodes.Length - 1; i++)
                {
                    float t = i / (float)(hairNodes.Length - 1);
                    hairNodes[i].position = Vector2.Lerp(hairStartPos, hairLastPos, t);
                }
            }
        }
        else if (hairWaitTimer > 0f)
        {
            hairWaitTimer -= Time.deltaTime;

            if (Physics2D.CircleCastNonAlloc(hairStartPos, attackRadius, currentAttackDir, raycastResult, currentAttackDistance, attackLayer) > 0)
            {
                var hook = raycastResult[0].transform.GetComponent<HookPoint>();
                if (hook != null)
                {
                    isHooked = true;
                    currentHook = hook;
                    currentAttackDistance = Mathf.Clamp((hook.transform.position - hairStartPos).magnitude, rotationMinDistance, rotationMaxDistance);
                    hook.Hook(rb2d, currentAttackDistance);
                }
                else
                {
                    currentAttackDistance = raycastResult[0].distance;
                    isUsingHair = false;
                    hairRecoveryTimer = attackCd;
                }
            }

            var hairLastPos = hairNodes[hairNodes.Length - 1].position = (Vector2)hairStartPos + currentAttackDir * currentAttackDistance;
            for (int i = 1; i < hairNodes.Length - 1; i++)
            {
                float t = i / (float)(hairNodes.Length - 1);
                hairNodes[i].position = Vector2.Lerp(hairStartPos, hairLastPos, t);
            }
        }
        else if (hairRecoveryTimer <= 0f && pressedAttack)
        {
            if (!isUsingHair)
            {
                isUsingHair = true;

                if (input.magnitude > deadzoneValue)
                    currentAttackDir = normalizedInput;
                else
                    currentAttackDir = lastInput;
                var overlapCount = Physics2D.OverlapCircleNonAlloc(hairStartPos, attackDistance, overlapResult, attackLayer);
                HookPoint closestHook = null;
                float closestDist = float.PositiveInfinity;
                for (int i = 0; i < overlapCount; i++)
                {
                    var hook = overlapResult[i].transform.GetComponent<HookPoint>();
                    if (hook == null) continue;
                    var hookDir = (overlapResult[i].transform.position - hairStartPos);
                    float dist = hookDir.sqrMagnitude;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestHook = hook;
                    }
                }
                if(closestHook != null)
                    currentAttackDir = (closestHook.transform.position - hairStartPos).normalized;
                currentAttackDistance = 0.5f;
            }

            currentAttackDistance += attackGrowSpeed * Time.deltaTime;


            if (currentAttackDistance > attackDistance)
            {
                currentAttackDistance = attackDistance;
                isUsingHair = false;
                hairRecoveryTimer = attackCd;
                hairWaitTimer = attackWaitTime;
            }
            else if (Physics2D.CircleCastNonAlloc(hairStartPos, attackRadius, currentAttackDir, raycastResult, currentAttackDistance, attackLayer) > 0)
            {
                var hook = raycastResult[0].transform.GetComponent<HookPoint>();
                if (hook != null)
                {
                    isHooked = true;
                    currentHook = hook;
                    currentAttackDistance = Mathf.Clamp((hook.transform.position - hairStartPos).magnitude, rotationMinDistance, rotationMaxDistance);
                    hook.Hook(rb2d, currentAttackDistance);
                }
                else
                {
                    currentAttackDistance = raycastResult[0].distance;
                    isUsingHair = false;
                    hairRecoveryTimer = attackCd;
                    hairWaitTimer = attackWaitTime;
                }
            }

            var hairLastPos = hairNodes[hairNodes.Length - 1].position = (Vector2)hairStartPos + currentAttackDir * currentAttackDistance;
            for (int i = 1; i < hairNodes.Length - 1; i++)
            {
                float t = i / (float)(hairNodes.Length - 1);
                hairNodes[i].position = Vector2.Lerp(hairStartPos, hairLastPos, t);
            }
        }
        else
        {
            if (isUsingHair)
            {
                isUsingHair = false;
                hairRecoveryTimer = attackCd;
                hairWaitTimer = attackWaitTime;
            }

            if (hairRecoveryTimer > 0)
                hairRecoveryTimer -= Time.deltaTime;
        }

        hair.positionCount = hairNodes.Length;
        for (int i = 0; i < hairNodes.Length; i++)
            hair.SetPosition(i, hairNodes[i].localPosition);
    }

    private void CheckFloor()
    {
        isOnFloor = Physics2D.OverlapCircleNonAlloc(floorPoint.position, floorPointRadius,overlapResult, floorLayer) > 0;
    }

    private void HandleMovement()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        normalizedInput = input.normalized;

        if (input.magnitude > deadzoneValue)
            lastInput = input.normalized;

        if (Mathf.Abs(input.x) >= deadzoneValue)
        {
            facingRight = input.x >= 0f;
            spriteRend.flipX = !facingRight;
        }

        bool jumpPressed = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetButton("Fire1"));
        if (!jumpPressed)
            hasReleasedJump = true;

        if (!isHooked)
        {
            rb2d.velocity += new Vector2(input.x * Time.deltaTime * accel, 0f);
            if (Mathf.Abs(rb2d.velocity.x) > maxSpeed) rb2d.velocity = new Vector2(maxSpeed * Mathf.Sign(rb2d.velocity.x), rb2d.velocity.y);
        }
        else
        {
            Vector2 tangent = (currentHook.transform.position - hairNodes[0].position).normalized;
            tangent = new Vector2(tangent.y, -tangent.x);
            rb2d.velocity += tangent * input.x * Time.deltaTime * rotationAccel;
            rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, rotationMaxSpeed);
        }

        if(!isHooked)
            rb2d.velocity *= new Vector2(isOnFloor ? (1f - floorDrag) : (1f - airDrag) ,1f);

        if ((!jumpPressed || rb2d.velocity.y < 0f) && !isOnFloor)
            rb2d.velocity += new Vector2(0f, -fakeGravity * Time.deltaTime);

        if (jumpTimer <= 0f)
        {
            if (isOnFloor && jumpPressed && hasReleasedJump)
            {
                Jump();
                jumpTimer = jumpCd;
                hasReleasedJump = false;
            }
        }
        else
            jumpTimer -= Time.deltaTime;

        
    }

    private void Jump()
    {
        rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
    }

    private void OnDrawGizmos()
    {
        if(floorPoint != null)
        {
            Gizmos.color = isOnFloor ? Color.green : Color.red;
            Gizmos.DrawWireSphere(floorPoint.position, floorPointRadius);
        }

        if(isUsingHair)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere((Vector2)hairNodes[0].position + currentAttackDir * currentAttackDistance, attackRadius);
        }
    }
}
