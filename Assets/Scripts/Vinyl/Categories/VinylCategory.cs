using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Mati36.Vinyl
{
    [CreateAssetMenu(menuName = "Vinyl/Base Category", fileName = "NewCategory")]
    public class VinylCategory : ScriptableObject
    {
        public string CategoryPath { get { return GetRecursivePath(this); } }

        private string GetRecursivePath(VinylCategory current)
        {
            if (current._parent == null)
                return current.name + "/";
            return GetRecursivePath(current._parent) + current.name + "/";
        }

        [SerializeField]
        private VinylCategory _parent;
        [SerializeField]
        private List<VinylCategory> _childs = new List<VinylCategory>();

        public VinylCategory Parent { get { return _parent; } set { _parent = value; } }
        public List<VinylCategory> Childs { get { return _childs; } private set { _childs = value; } }

        [SerializeField]
        private AudioMixerGroup _outputMixerGroup;
        public AudioMixerGroup OutputMixerGroup
        {
            get { return GetMixerGroup(this); }
            set { _outputMixerGroup = value; }
        }

        private AudioMixerGroup GetMixerGroup(VinylCategory current)
        {
            if (current.overrideParent == false || current._parent == null)
                return current._outputMixerGroup;
            return GetMixerGroup(current._parent);
        }
        public bool overrideParent;
    }
}