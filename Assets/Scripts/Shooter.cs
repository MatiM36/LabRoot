using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour, IReseteable
{
    public Bullet bulletPrefab;
    public Transform shootPoint;

    public float waveCd = 3f;
    public float bulletCd = 0.5f;
    public int bulletsPerWave = 1;

    private float currentWaveTimer = 0f;
    private float currentBulletTimer = 0f;
    private int currentWaveBulletCount = 0;

    private List<Bullet> bullets = new List<Bullet>();

    private void Start()
    {
        currentWaveTimer = waveCd;
        LevelManager.Instance.RegisterLevelObject(this);
    }

    private void OnDestroy()
    {
        LevelManager.Instance.UnregisterLevelObject(this);
    }


    private void Update()
    {
        if (currentWaveTimer > 0f)
            currentWaveTimer -= Time.deltaTime;
        else
        {
            if (currentBulletTimer > 0f)
                currentBulletTimer -= Time.deltaTime;
            else
            {
                var bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity, null);
                bullet.transform.right = transform.right;
                bullet.OnBulletDestroyed += OnBulletDestroyed;

                bullets.Add(bullet);

                currentWaveBulletCount++;
                if(currentWaveBulletCount >= bulletsPerWave)
                {
                    currentWaveBulletCount = 0;
                    currentWaveTimer = waveCd;
                }
                else
                {
                    currentBulletTimer = bulletCd;
                }
            }
        }
    }

    private void OnBulletDestroyed(Bullet bullet)
    {
        bullets.Remove(bullet);
    }

    public void ResetObject()
    {
        foreach (var b in bullets)
            Destroy(b.gameObject);
        bullets.Clear();
    }
}
