using Mati36.Vinyl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMusic : MonoBehaviour
{
    public VinylAsset bgMusic;

    private VinylAudioSource currentMusic;
    private void Start()
    {
        currentMusic = bgMusic.Play();
    }
    private void OnDestroy()
    {
        currentMusic?.StopSource();
    }
}
