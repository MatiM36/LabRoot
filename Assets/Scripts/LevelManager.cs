using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    static public LevelManager Instance { get; private set; }

    public Transform startPoint;
    public Image fadeImage;
    public float fadeDuration = 0.5f;
    private PlayerController player;

    private Coroutine currentDeathRoutine = null;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    public void RegisterPlayer(PlayerController playerController)
    {
        player = playerController;
        player.hurtbox.OnDamageReceived += OnPlayerDeath;
        player.transform.position = startPoint.position;
    }

    private void OnPlayerDeath()
    {
        if (currentDeathRoutine != null) return;

        currentDeathRoutine = StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        player.ShowPlayer(false);
        player.EnableControls(false);

        float t = 0f;
        var color = fadeImage.color;
        fadeImage.enabled = true;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            color.a = t;
            fadeImage.color = color;
            yield return null;
        }

        color.a = t = 1f;
        fadeImage.color = color;

        player.transform.position = startPoint.position;
        player.ShowPlayer(true);

        while (t > 0f)
        {
            t -= Time.deltaTime / fadeDuration;
            color.a = t;
            fadeImage.color = color;
            yield return null;
        }
        color.a = t = 0f;
        fadeImage.color = color;
        fadeImage.enabled = false;

        player.EnableControls(true);

        currentDeathRoutine = null;
    }

    public void UnregisterPlayer()
    {
        if (player != null)
        {
            player.hurtbox.OnDamageReceived -= OnPlayerDeath;
            player = null;
        }
    }
}
