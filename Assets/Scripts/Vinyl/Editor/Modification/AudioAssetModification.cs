using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mati36.Vinyl;

namespace Mati36.VinylEditor
{
    using AssetModificationProcessor = UnityEditor.AssetModificationProcessor;

    public class AudioAssetModification : AssetModificationProcessor
    {
        //static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        //{
        //    var obj = AssetDatabase.LoadMainAssetAtPath(path);
        //    if (obj == null) return AssetDeleteResult.DidNotDelete;
        //    if (obj.GetType() == typeof(VinylAsset))
        //        VinylConfig.Current.OnDeleteVinylAsset((VinylAsset)obj);
        //    else if(obj.GetType() == typeof(AudioClip))
        //        VinylConfig.Current.OnDeleteClip((AudioClip)obj);
        //    return AssetDeleteResult.DidNotDelete;
        //}
    }
}