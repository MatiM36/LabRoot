using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public LayerMask damageLayer;

    public event Action OnDamageReceived;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            OnDamageReceived?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            OnDamageReceived?.Invoke();
        }
    }
}
