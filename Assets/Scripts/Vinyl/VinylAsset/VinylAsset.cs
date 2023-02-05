using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using Random = UnityEngine.Random;

//#pragma warning disable 0649
namespace Mati36.Vinyl
{
    //[CreateAssetMenu(menuName = "Vinyl/" + VinylConstants.ASSET_NAME, fileName = "New " + VinylConstants.ASSET_NAME),]
    public class VinylAsset : ScriptableObject
    {
        public VinylCategory category;
        public AudioClip Clip
        {
            get
            {
                Debug.Assert(randomClips.Count > 0, "Vinyl Asset " + name + " doesn't have any AudioClips");
                if (randomClips.Count == 0)
                    return null;
                if (randomClips.Count == 1)
                {
                    Debug.Assert(randomClips[0] != null, "Vinyl Asset " + name + " has an empty AudioClip");
                    return randomClips[0].clip;
                }

                float totalChance = 0f;
                foreach (var clip in randomClips)
                    totalChance += clip.chance;

                float randomValue = Random.value;
                if (totalChance == 0) randomValue = -2;
                IEnumerator<WeightedAudioClip> e = randomClips.GetEnumerator();
                while (randomValue > -1)
                {
                    if (!e.MoveNext()) { e.Reset(); e.MoveNext(); };

                    if (randomClips.Count == 2 && e.Current.chance == 0)
                    { _lastClip = e.Current.clip; continue; }
                    if (avoidClipRepeat && e.Current.clip == _lastClip) continue;

                    float currentChance = e.Current.chance / totalChance;
                    if (currentChance > randomValue)
                    {
                        _lastClip = e.Current.clip;
                        e.Dispose();
                        return e.Current.clip;
                    }
                    randomValue -= currentChance;
                }
                e.Dispose();
                Debug.LogWarning("Random algorithm failed");
                return randomClips[Random.Range(0, randomClips.Count)].clip;
            }
        }

        [NonSerialized]
        private AudioClip _lastClip;

        [SerializeField]
        private List<WeightedAudioClip> randomClips = new List<WeightedAudioClip>();
        public List<WeightedAudioClip> PossibleClips { get { return randomClips; } }
        [SerializeField]
        private bool avoidClipRepeat = true;

        [Header("Sound Parameters")]
        public bool loop;

        [SerializeField]
        public float vol = 1f;
        [SerializeField]
        private bool volRandomBetweenRanges = false;
        [SerializeField]
        public Vector2 volRange = new Vector2(1f, 1f);

        [SerializeField]
        private float pitch = 1f;
        [SerializeField]
        private bool pitchRandomBetweenRanges = false;
        [SerializeField]
        private Vector2 pitchRange = new Vector2(1f, 1f);

        public float Volume
        { get { return volRandomBetweenRanges ? Random.Range(volRange.x, volRange.y) : vol; } }
        public float Pitch { get { return pitchRandomBetweenRanges ? Random.Range(pitchRange.x, pitchRange.y) : pitch; } }

        public AudioMixerGroup MixerGroup { get { return category == null ? null : category.OutputMixerGroup; } }

        public VinylAudioSource Play()
        {
            return VinylManager.PlaySound(this);
        }

        public VinylAudioSource PlayAt(Vector3 pos)
        {
            return VinylManager.PlaySoundAt(this, pos);
        }


        [Serializable]
        public class WeightedAudioClip
        {
            public AudioClip clip;
            public float chance = 1f;
        }
    }
}