using Mati36.Vinyl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public interface IReseteable { public void ResetObject(); }

public class LevelManager : MonoBehaviour
{
    static public LevelManager Instance { get; private set; }

    public Transform startPoint;
    public CameraController cameraController;
    public Canvas canvas;

    public CollectablesUI specialCollectableCountPanel;
    public CollectablesUI commonCollectableCountPanel;

    public Image fadeImage;
    public float fadeDuration = 0.5f;
    public VinylAsset levelMusic;
    private PlayerController player;

    private Coroutine currentDeathRoutine = null;
    private Coroutine currentLevelRoutine = null;

    public HashSet<IReseteable> reseteableObjects = new HashSet<IReseteable>();

    public HashSet<CollectableItem> collectedItemsThisLife = new HashSet<CollectableItem>();
    public HashSet<CollectableItem> collectedAndSavedItems = new HashSet<CollectableItem>();
    public HashSet<CollectableItem> allLevelCollectables = new HashSet<CollectableItem>();

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    private IEnumerator Start()
    {
        string unlockKey = SceneManager.GetActiveScene().name + MenuController.UNLOCKED_SUFIX;
        if (!PlayerPrefs.HasKey(unlockKey))
            PlayerPrefs.SetInt(unlockKey, 1);

        VinylManager.FadeOutAll(0.5f);
        levelMusic?.Play();

        canvas.worldCamera = cameraController.camera;

        yield return FadeInRoutine();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Exit"))
            SceneManager.LoadScene("MainMenu");
    }

    public void RegisterLevelObject(IReseteable reseteable)
    {
        reseteableObjects.Add(reseteable);
    }

    public void UnregisterLevelObject(IReseteable reseteable)
    {
        reseteableObjects.Remove(reseteable);
    }

    public void RegisterPlayer(PlayerController playerController)
    {
        player = playerController;
        player.hurtbox.OnDamageReceived += OnPlayerDeath;
        player.transform.position = startPoint.position;

        cameraController.SetTarget(player);
        cameraController.ForcePosition();
    }

    public void OnEnterCheckpoint(Checkpoint checkpoint)
    {
        startPoint = checkpoint.spawnPoint;
        foreach (var item in collectedItemsThisLife)
            collectedAndSavedItems.Add(item);
    }
    public void RegisterCollectableItem(CollectableItem item)
    {
        allLevelCollectables.Add(item);
    }
    public void OnCollectItem(CollectableItem item)
    {
        collectedItemsThisLife.Add(item);
        if (item.type == CollectableItem.CollectableType.Special)
            UpdateSpecialCollectablesPanel();
        else if (item.type == CollectableItem.CollectableType.Common)
            UpdateCommonCollectablesPanel();
    }

    private void UpdateSpecialCollectablesPanel()
    {
        specialCollectableCountPanel.OnCollectiblePick(collectedItemsThisLife.Count(c => c.type == CollectableItem.CollectableType.Special), allLevelCollectables.Count(c => c.type == CollectableItem.CollectableType.Special));
    }
    private void UpdateCommonCollectablesPanel()
    {
        commonCollectableCountPanel.OnCollectiblePick(collectedItemsThisLife.Count(c => c.type == CollectableItem.CollectableType.Common), allLevelCollectables.Count(c => c.type == CollectableItem.CollectableType.Common));
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

        yield return new WaitForSeconds(0.5f);

        yield return FadeOutRoutine();

        player.transform.position = startPoint.position;
        player.ShowPlayer(true);
        cameraController.ForcePosition();

        ResetAllElements();

        yield return FadeInRoutine();

        player.EnableControls(true);

        currentDeathRoutine = null;
    }

    private IEnumerator FadeOutRoutine()
    {
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
    }
    private IEnumerator FadeInRoutine()
    {
        float t = 1f;
        var color = fadeImage.color;
        fadeImage.enabled = true;
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
    }

    private void ResetAllElements()
    {
        foreach (var r in reseteableObjects)
            r.ResetObject();

        collectedItemsThisLife.Clear();
        foreach (var collectedItem in collectedAndSavedItems)
        {
            collectedItem.gameObject.SetActive(false);
            collectedItemsThisLife.Add(collectedItem);
        }

        UpdateCommonCollectablesPanel();
        UpdateSpecialCollectablesPanel();
    }

    public void UnregisterPlayer()
    {
        if (player != null)
        {
            player.hurtbox.OnDamageReceived -= OnPlayerDeath;
            player = null;
        }
    }

    public void GoToNextLevel(string levelName)
    {
        if (currentLevelRoutine != null) return;
        currentLevelRoutine = StartCoroutine(NextLevelRoutine(levelName));
    }

    private IEnumerator NextLevelRoutine(string levelName)
    {
        player.EnableControls(false);
        yield return FadeOutRoutine();
        SceneManager.LoadScene(levelName);
    }
}
