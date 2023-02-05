using Mati36.Vinyl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public PlayerController player;
    public Animator animator;

    public ParticleSystem deathParticles;

    public string idleAnimation = "Idle";
    public string runAnimation = "Run";
    public string jumpAnimation = "Jump";
    public string fallAnimation = "Fall";
    public string hookAnimation = "Hook";

    public VinylAsset deathSound;
    public VinylAsset attackSound;
    public VinylAsset jumpSound;

    private void Awake()
    {
        player.hurtbox.OnDamageReceived += OnDeath;
        player.OnPlayerJump += OnJump;
        player.OnPlayerAttack += OnAttack;
    }

    private void OnAttack()
    {
        attackSound?.PlayAt(player.transform.position);
    }

    private void OnJump()
    {
        jumpSound?.PlayAt(player.transform.position);
    }

    private void OnDeath()
    {
        if (deathParticles != null)
            Instantiate(deathParticles, player.transform.position, Quaternion.identity, null);

        deathSound?.PlayAt(player.transform.position);
    }

    private void Update()
    {
        if (player.isHooked)
            animator.Play(hookAnimation);
        else if (player.isOnFloor)
        {
            if (Mathf.Abs(player.input.x) > player.deadzoneValue)
                animator.Play(runAnimation);
            else
                animator.Play(idleAnimation);
        }
        else
        {
            if (player.rb2d.velocity.y > 0)
                animator.Play(jumpAnimation);
            else
                animator.Play(fallAnimation);
        }
    }
}
