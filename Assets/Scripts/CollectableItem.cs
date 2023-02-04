using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour, IReseteable
{
    public Hurtbox hurtbox;
    public ParticleSystem pickParticle;

    private void Awake()
    {
        hurtbox.OnDamageReceived += OnPlayerCollected;
    }

    private void Start()
    {
        LevelManager.Instance.RegisterLevelObject(this);
    }

    private void OnDestroy()
    {
        LevelManager.Instance.UnregisterLevelObject(this);
    }

    public void ResetObject()
    {
        gameObject.SetActive(true);
    }

    private void OnPlayerCollected()
    {
        if(pickParticle != null)
            Instantiate(pickParticle, transform.position, Quaternion.identity, null);
        gameObject.SetActive(false);
    }
}
