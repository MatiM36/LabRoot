using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mati36.Vinyl
{
    public class VinylHelper : MonoBehaviour
    {
        public VinylAsset sound;

        public void PlaySound()
        {
            sound.Play();
        }

        public void PlaySoundAtObjPos()
        {
            sound.PlayAt(transform.position);
        }
    }
}
