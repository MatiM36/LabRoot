using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mati36.Vinyl;

namespace Mati36.VinylEditor
{
    public class AudioAssetImporter : AssetPostprocessor
    {
        void OnPostprocessAudio(AudioClip clip)
        {
            if (!VinylConfig.Current.autocreateVinylAssets) return;

            AudioClip clipAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioClip)) as AudioClip;

            if (clipAsset == null) { EditorApplication.update += TryNextFrame; return; }
            CreateAsset(clipAsset);
        }

        private void TryNextFrame()
        {
            EditorApplication.update -= TryNextFrame;

            AudioClip clipAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioClip)) as AudioClip;

            if (clipAsset == null) { Debug.Log("Clip asset is null, reimport."); return; }
            CreateAsset(clipAsset);
        }

        private void CreateAsset(AudioClip clip)
        {
            foreach (var usedClip in VinylConfig.CurrentlyUsedClips)
            {
                if (usedClip == clip) return;
            }

            VinylSerializationUtility.CreateDefaultAsset(clip);
        }
    }
}