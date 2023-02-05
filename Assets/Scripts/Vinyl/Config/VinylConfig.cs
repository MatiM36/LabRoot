using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mati36.Vinyl
{
    [CreateAssetMenu(menuName = "Vinyl/Config")]
    public class VinylConfig : ScriptableObject
    {
        static private VinylConfig _current;
        static public VinylConfig Current
        {
            get
            {
                if (_current == null)
                {
                    _current = Resources.Load("VinylConfig") as VinylConfig;
#if UNITY_EDITOR
                    if (_current == null)
                    { Debug.LogWarning("VinylConfig not found, creating default config..."); _current = CreateVinylConfigDefault(); }
#endif
                }
                return _current;
            }
        }

#if UNITY_EDITOR
        private static VinylConfig CreateVinylConfigDefault()
        {
            VinylConfig newConfig = CreateInstance<VinylConfig>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateAsset(newConfig, "Assets/Resources/VinylConfig.asset");
            AssetDatabase.SaveAssets();
            return newConfig;
        }
#endif

        public int poolDefaultSize = 20;
        public bool autoStopOnSceneChange = true;
        public bool autocreateVinylAssets = false;

        //CATEGORY
        public List<VinylCategory> baseCategories = new List<VinylCategory>();

        //UTILITY

        static public VinylCategory FindCategoryByName(string name)
        {
            VinylCategory findCat;
            foreach (var cat in Current.baseCategories)
            {
                findCat = FindCategoryByNameRecursive(cat, name);
                if (findCat != null) return cat;
            }
            return null;
        }

        static private VinylCategory FindCategoryByNameRecursive(VinylCategory current, string name)
        {
            if (current.name == name)
                return current;
            VinylCategory findCat;
            foreach (var child in current.Childs)
            {
                findCat = FindCategoryByNameRecursive(child, name);
                if (findCat != null)
                    return findCat;
            }
            return null;
        }

#if UNITY_EDITOR
        //SOUNDCLIPS

        static public IEnumerable<AudioClip> CurrentlyUsedClips {
            get
            {
                return VinylAssets.SelectMany(asset => asset.PossibleClips).Select(wc => wc.clip);
            }
        }

        //REF TO VINYLASSETS

        static public IEnumerable<string> VinylAssetsPaths
        {
            get
            {
                return AssetDatabase.FindAssets("t:" + VinylConstants.ASSET_NAME).Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        static public IEnumerable<VinylAsset> VinylAssets
        {
            get
            {
                return VinylAssetsPaths.Select(path => AssetDatabase.LoadAssetAtPath<VinylAsset>(path));
            }
        }

#endif //EDITOR
    }
}