using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public LayerMask damageLayer;

    public event Action OnDamageReceived;

    public bool showDebug = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(showDebug)
            Debug.Log("ON TRIGG " + collision.gameObject.name, collision.gameObject);
        if (((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            OnDamageReceived?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(showDebug)
            Debug.Log("ON COL " + collision.gameObject.name, collision.gameObject);
        if (((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            OnDamageReceived?.Invoke();
        }
    }
}
