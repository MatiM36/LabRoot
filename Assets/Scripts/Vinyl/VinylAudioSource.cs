using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Mati36.Vinyl
{
    public class VinylAudioSource : MonoBehaviour
    {
        public VinylAsset CurrentAsset { get; private set; }

        public bool IsPaused { get; private set; }
        public bool IsPlaying { get { return _audioSource.isPlaying; } }
        public bool IsLocked { get; private set; }

        private bool SoundEnded { get; set; }

        private AudioSource _audioSource;
        private float _initialVol, _initialPitch;

        public float Volume { get { return _audioSource.volume; } set { _audioSource.volume = value; } }
        public float Pitch { get { return _audioSource.pitch; } set { _audioSource.pitch = value; } }

        public event Action<VinylAudioSource> e_OnEndSound = delegate { };
        public event Action<VinylAudioSource> e_OnSoundReturnToPool = delegate { };

        public void Initialize()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _audioSource.minDistance = 5;
            _audioSource.maxDistance = 30;
        }

        public void Clear()
        {
            CurrentAsset = null;
            _audioSource.clip = null;
        }

        public void PlayAudio(VinylAsset soundAsset, SoundMode mode)
        {
            CurrentAsset = soundAsset;
            PlayAudio(CurrentAsset.Clip, CurrentAsset.MixerGroup, mode, CurrentAsset.Volume, CurrentAsset.Pitch, CurrentAsset.loop);
        }

        public void PlayAudio(VinylAsset soundAsset, SoundMode mode, float overridePitch)
        {
            CurrentAsset = soundAsset;
            PlayAudio(CurrentAsset.Clip, CurrentAsset.MixerGroup, mode, CurrentAsset.Volume, overridePitch, CurrentAsset.loop);
        }

        private void PlayAudio(AudioClip clip, AudioMixerGroup group, SoundMode mode, float vol, float pitch, bool loop = false)
        {
            _audioSource.Stop();
            _audioSource.spatialBlend = mode == SoundMode.Mode2D ? 0 : 1;
            _audioSource.spatialize = mode == SoundMode.Mode2D ? false : true;
            _audioSource.clip = clip;
            _audioSource.outputAudioMixerGroup = group;
            _initialVol = vol;
            Volume = vol;
            _initialPitch = pitch;
            Pitch = pitch;
            _audioSource.loop = loop;
            _audioSource.Play();
            IsPaused = false;
            IsLocked = false;
            SoundEnded = false;
        }

        private void Update()
        {
            if (!IsPaused && !_audioSource.isPlaying && !SoundEnded)
                StopSource();
        }
        //
        //PLAYBACK
        //
        public void PauseSource()
        {
            _audioSource.Pause();
            IsPaused = true;
        }

        public void UnPauseSource()
        {
            _audioSource.UnPause();
            IsPaused = false;
        }

        public void RestartSource()
        {
            SoundEnded = false;
            _audioSource.Stop();
            _audioSource.Play();
            //_audioSource.timeSamples = 0;
        }

        public void StopSource()//vuelve al pool
        {
            SoundEnded = true;
            _audioSource.Stop();
            StopAllCoroutines();
            e_OnEndSound(this);
            if (!IsLocked)
                e_OnSoundReturnToPool(this);
        }
        //
        //LOCK
        //
        public void Lock()
        {
            IsLocked = true;
        }
        public void Unlock()
        {
            IsLocked = false;
            if (SoundEnded)
                e_OnSoundReturnToPool(this);
        }
        //
        //RELATIVE
        //
        public void ModifyPitchRelative(float value)
        {
            Pitch = _initialPitch * value;
        }

        public void ModifyVolumeRelative(float value)
        {
            Volume = _initialVol * value;
        }
        //
        //FADE
        //
        public void FadeIn(float duration)
        {
            if (!gameObject.activeSelf) return;
            if (currentVolumeRoutine != null)
                StopCoroutine(currentVolumeRoutine);
            currentVolumeRoutine = StartCoroutine(LerpToVolume(0f, Volume, duration));
        }
        public void FadeOut(float duration)
        {
            if (!gameObject.activeSelf) return;
            if (currentVolumeRoutine != null)
                StopCoroutine(currentVolumeRoutine);
            currentVolumeRoutine = StartCoroutine(LerpToVolume(Volume, 0f, duration, StopSource));
        }

        public void FadeTo(float destinationVolume, float duration)
        {
            if (!gameObject.activeSelf) return;
            if (currentVolumeRoutine != null)
                StopCoroutine(currentVolumeRoutine);
            currentVolumeRoutine = StartCoroutine(LerpToVolume(Volume, destinationVolume, duration));
        }

        private Coroutine currentVolumeRoutine = null;

        private IEnumerator LerpToVolume(float fromVolume, float toVolume, float lerpDuration, Action onFinishCallback = null)
        {
            float t = 0;
            while (t < 1)
            {
                if (!IsPaused)
                {
                    Volume = Mathf.Lerp(fromVolume, toVolume, t);
                    t += Time.deltaTime / lerpDuration;
                }
                yield return null;
            }
            Volume = toVolume;
            onFinishCallback?.Invoke();
        }
    }

    public enum SoundMode { Mode2D, Mode3D }
}