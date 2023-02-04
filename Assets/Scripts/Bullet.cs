using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public float speed = 1f;
    public float maxDuration = 5f;

    public Hurtbox hurtbox;

    public event Action<Bullet> OnBulletDestroyed;

    private float currentLife = 0f;

    private void Awake()
    {
        hurtbox.OnDamageReceived += DestroyBullet;
    }

    private void DestroyBullet()
    {
        OnBulletDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        rb2d.velocity = transform.right * speed;
    }

    private void Update()
    {
        currentLife += Time.deltaTime;
        if (currentLife > maxDuration)
            DestroyBullet();
    }
}
