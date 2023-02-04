using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public string nextLevel = string.Empty;

    public Hurtbox hurtbox;

    private void Awake()
    {
        hurtbox.OnDamageReceived += OnPlayerReach;
    }

    private void OnPlayerReach()
    {
        LevelManager.Instance.GoToNextLevel(nextLevel);
        hurtbox.OnDamageReceived += OnPlayerReach;
    }
}
