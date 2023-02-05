using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mati36.VinylExtensions;

namespace Mati36.Vinyl
{
    [ExecuteInEditMode]
    static public class VinylManager
    {
        const string MANAGER_NAME = "Vinyl Manager";

        static private AudioSource globalAudioSource;
        static private VinylSourcesPool audioSourcePool;

        const string GLOBALSOURCENAME = "GlobalAudioSource";
        const string POOLNAME = "VinylSrcPool";

        static private AudioSource GlobalAudioSource
        {
            get
            {
                if (globalAudioSource == null)
                {
                    var existingSource = GameObject.Find(GLOBALSOURCENAME);
                    if (existingSource != null)
                        globalAudioSource = existingSource.GetComponent<AudioSource>();
                    else
                    {
                        Debug.Log((MANAGER_NAME.ToUpper() + " // Creating " + GLOBALSOURCENAME + "...").Bold());
                        globalAudioSource = new GameObject(GLOBALSOURCENAME, typeof(AudioSource)).GetComponent<AudioSource>();
                        globalAudioSource.gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                        globalAudioSource.playOnAwake = false;
                        globalAudioSource.spatialBlend = 0;
                    }
                }
                return globalAudioSource;
            }
        }

        static private VinylSourcesPool VinylSrcPool
        {
            get
            {
                if (audioSourcePool == null)
                {
                    var existingPool = GameObject.Find(POOLNAME);
                    if (existingPool != null)
                    {
                        GameObject.Destroy(existingPool.gameObject);
                        //audioSourcePool = existingPool.GetComponent<VinylSourcesPool>();
                        //audioSourcePool.Initialize();
                    }

                    Debug.Log((MANAGER_NAME.ToUpper() + " // CreatingPool...").Bold());
                    audioSourcePool = new GameObject(POOLNAME, typeof(VinylSourcesPool)).GetComponent<VinylSourcesPool>();
                    //audioSourcePool.gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                    audioSourcePool.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    GameObject.DontDestroyOnLoad(audioSourcePool.gameObject);
                    audioSourcePool.Initialize();
                }
                return audioSourcePool;
            }
        }

        /// <summary>
        /// Initialize manually the AudioSources
        /// </summary>
        static public void InitializeSources()
        {
            if (GlobalAudioSource == null) { Debug.Log("Can't create GlobalAudioSource"); return; }
            if (VinylSrcPool == null) { Debug.Log("Can't create AudioSourcesPool"); return; }
        }

        //PLAYBACK
        static public VinylAudioSource PlaySound(VinylAsset sound)
        {
            var source = VinylSrcPool.GetSource();
            source.PlayAudio(sound, SoundMode.Mode2D);
            return source;
        }

        static public VinylAudioSource PlaySound(VinylAsset sound, float overridePitch)
        {
            var source = VinylSrcPool.GetSource();
            source.PlayAudio(sound, SoundMode.Mode2D, overridePitch);
            return source;
        }

        static public void PlayOneShotSound(VinylAsset sound)
        {
            GlobalAudioSource.PlayOneShot(sound.Clip, sound.vol);
        }

        static public VinylAudioSource PlaySoundAt(VinylAsset sound, Vector3 position)
        {
            var source = VinylSrcPool.GetSource();
            source.transform.position = position;
            source.PlayAudio(sound, SoundMode.Mode3D);
            return source;
        }

        static public VinylAudioSource PlaySoundAt(VinylAsset sound, Vector3 position, float overridePitch)
        {
            var source = VinylSrcPool.GetSource();
            source.transform.position = position;
            source.PlayAudio(sound, SoundMode.Mode3D, overridePitch);
            return source;
        }


        static public VinylAudioSource CrossFadeTo(this VinylAudioSource from, VinylAsset sound, float crossfadeLength)
        {
            from.FadeOut(crossfadeLength);
            var to = PlaySound(sound);
            to.FadeIn(crossfadeLength);
            return to;
        }

        static public void CrossFadeTo(ref VinylAudioSource source, VinylAsset toAsset, float crossfadeLength)
        {
            source.FadeOut(crossfadeLength);
            var to = PlaySound(toAsset);
            to.FadeIn(crossfadeLength);
            source = to;
        }

        //
        //GLOBAL
        //
        static public void StopAll()
        {
            GlobalAudioSource.Stop();
            VinylSrcPool.ApplyToActiveSources(src => src.StopSource());
        }

        static public void PauseAll()
        {
            GlobalAudioSource.Pause();
            VinylSrcPool.ApplyToActiveSources(src => src.PauseSource());
        }

        static public void UnPauseAll()
        {
            GlobalAudioSource.UnPause();
            VinylSrcPool.ApplyToActiveSources(src => src.UnPauseSource());
        }
        static public void FadeOutAll(float fadeDuration)
        {
            GlobalAudioSource.Stop();
            VinylSrcPool.ApplyToActiveSources(src => src.FadeOut(fadeDuration));
        }
        static public void ApplyToAll(Action<VinylAudioSource> action)
        {
            VinylSrcPool.ApplyToActiveSources(action);
        }
        //
        //CATEGORY
        //
        static public void StopByCategory(string categoryName)
        {
            VinylSrcPool.ApplyToActiveSources(src => { if (VinylConfig.FindCategoryByName(categoryName) == src.CurrentAsset.category) src.StopSource(); });
        }

        static public void PauseByCategory(string categoryName)
        {
            VinylSrcPool.ApplyToActiveSources(src => { if (VinylConfig.FindCategoryByName(categoryName) == src.CurrentAsset.category) src.PauseSource(); });
        }

        static public void UnPauseByCategory(string categoryName)
        {
            VinylSrcPool.ApplyToActiveSources(src => { if (VinylConfig.FindCategoryByName(categoryName) == src.CurrentAsset.category) src.UnPauseSource(); });
        }

        //
        //SPEED
        //
        static public void ModifySpeed(float speed)
        {
            if (speed == 0)
                GlobalAudioSource.Pause();
            else
            {
                GlobalAudioSource.UnPause();
                GlobalAudioSource.pitch = speed;
            }

            VinylSrcPool.ApplyToActiveSources(
                (src) =>
                {
                    if (speed == 0)
                        src.PauseSource();
                    else
                    {
                        src.UnPauseSource();
                        src.ModifyPitchRelative(speed);
                    }
                }
                );
        }
    }
}