using Mati36.Vinyl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour, IReseteable
{
    public enum CollectableType { Special, Common }

    public Hurtbox hurtbox;
    public ParticleSystem pickParticle;
    public VinylAsset pickSound;
    public CollectableType type;

    private void Awake()
    {
        hurtbox.OnDamageReceived += OnPlayerCollected;
    }

    private void Start()
    {
        LevelManager.Instance.RegisterLevelObject(this);
        LevelManager.Instance.RegisterCollectableItem(this);
    }

    private void OnDestroy()
    {
        LevelManager.Instance.UnregisterLevelObject(this);
    }

    public void ResetObject()
    {
        gameObject.SetActive(true);
    }

    public void OnPlayerCollected()
    {
        if(pickParticle != null)
            Instantiate(pickParticle, transform.position, Quaternion.identity, null);
        pickSound?.PlayAt(transform.position);
        gameObject.SetActive(false);
        LevelManager.Instance.OnCollectItem(this);
    }
}
