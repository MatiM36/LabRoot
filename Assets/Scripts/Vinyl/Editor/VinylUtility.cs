using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mati36.Vinyl
{
    static public class VinylUtility
    {
        [MenuItem("Vinyl/Open Configuration")]
        static public void OpenConfig()
        {
            Selection.activeObject = VinylConfig.Current;
        }

        [MenuItem("Assets/Create/Vinyl/" + VinylConstants.ASSET_NAME, priority = -1000)]
        static private void CreateAssetFromSelection()
        {
            VinylAsset defaultAsset = null;
            if (Selection.assetGUIDs.Length == 0) { defaultAsset = VinylSerializationUtility.CreateDefaultAsset(null); return; }


            if (Selection.assetGUIDs.Length == 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
                var type = File.GetAttributes(path);
                if ((type & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    defaultAsset = VinylSerializationUtility.CreateDefaultAsset(null, path + "/");
                    Selection.activeObject = defaultAsset;
                    EditorGUIUtility.PingObject(defaultAsset);
                    return;
                }
            }


            foreach (var selectedItem in Selection.assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(selectedItem);
                UnityEngine.Object objSelected = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));

                AudioClip selectedClip = objSelected as AudioClip;
                if (selectedClip == null)
                {
                    if (Selection.assetGUIDs.Length == 1)
                        EditorUtility.DisplayDialog("Not an AudioClip", "You must select an AudioClip", "Ok");
                    continue;
                }
                defaultAsset = VinylSerializationUtility.CreateDefaultAsset(selectedClip);
            }

            if (defaultAsset != null)
            {
                Selection.activeObject = defaultAsset;
                EditorGUIUtility.PingObject(defaultAsset);
            }
        }
    }
}