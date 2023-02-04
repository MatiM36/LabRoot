using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    public Hurtbox hurtbox;
    public ParticleSystem pickParticle;

    private void Awake()
    {
        hurtbox.OnDamageReceived += OnPlayerCollected;
    }

    private void OnPlayerCollected()
    {
        if(pickParticle != null)
            Instantiate(pickParticle, transform.position, Quaternion.identity, null);
        Destroy(gameObject);
    }
}
