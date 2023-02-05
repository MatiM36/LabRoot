using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassFloor : MonoBehaviour, IReseteable
{
    public SpriteRenderer spriteRend;
    public Animator animator;
    public string idleAnimation = "Glass_Idle";
    public string breakAnimation = "Glass_Break";

    public Hurtbox hurtbox;

    public float timeBeforeBreak = 1f;
    public float timeToRegenerate = 4f;

    public ParticleSystem breakParticles;

    public bool breaking;
    public bool regenerating;
    public float breakTimer;
    public float regenerateTimer;


    private void Awake()
    {
        hurtbox.OnDamageReceived += OnPlayerStep;
    }

    private void Start()
    {
        animator.Play(idleAnimation, 0, 1f);
    }

    public void EnableFloor(bool value)
    {
        hurtbox.gameObject.SetActive(value);
        spriteRend.enabled = value;
    }

    private void Update()
    {
        if(regenerating)
        {
            if (regenerateTimer > 0f)
                regenerateTimer -= Time.deltaTime;
            else
            {
                regenerating = false;
                EnableFloor(true);
                animator.Play(idleAnimation, 0);
            }
        }
        else if (breaking)
        {
            if (breakTimer > 0f)
                breakTimer -= Time.deltaTime;
            else
            {
                EnableFloor(false);
                regenerating = true;
                breaking = false;
                regenerateTimer = timeToRegenerate;
                if (breakParticles != null)
                    Instantiate(breakParticles, transform.position, Quaternion.identity, null);
            }
        }
    }

    private void OnPlayerStep()
    {
        if (breaking) return;

        breakTimer = timeBeforeBreak;
        breaking = true;
        regenerating = false;
        animator.Play(breakAnimation);
    }

    public void ResetObject()
    {
        animator.Play(idleAnimation,0,1f);
        breaking = false;
        regenerating = false;
        EnableFloor(true);
    }
}
