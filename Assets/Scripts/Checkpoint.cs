using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform spawnPoint;
    public Hurtbox hurtbox;

    private void Awake()
    {
        hurtbox.OnDamageReceived += OnPlayerEnter;
    }

    private void OnPlayerEnter()
    {
        LevelManager.Instance.OnEnterCheckpoint(this);
    }
}
