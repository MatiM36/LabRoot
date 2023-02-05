using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mati36.Vinyl
{
    static public class VinylSerializationUtility
    {

        /// 
        /// CATEGORIES
        /// 
        static public VinylCategory CreateCategory(string name, VinylCategory parent)
        {
            var newCategory = VinylCategory.CreateInstance<VinylCategory>();
            newCategory.name = name;
            if (parent != null)
                newCategory.Parent = parent;


            var configPath = AssetDatabase.GetAssetPath(VinylConfig.Current);
            var split = configPath.Split('/');
            configPath = "";
            for (int i = 0; i < split.Length - 2; i++)
                configPath += split[i] + "/";

            configPath += "Categories/Data/" + name + ".asset";

            AssetDatabase.CreateAsset(newCategory, AssetDatabase.GenerateUniqueAssetPath(configPath));
            AssetDatabase.SaveAssets();

            if (parent != null)
                AddChild(parent, newCategory);
            else
                AddBaseCategory(newCategory);

            return newCategory;
        }

        //TODO: MOVE THIS TO SERIALIZABLEHELPER CLASS
        static private void RemoveChild(VinylCategory category, VinylCategory child)
        {
            if (category == null) return;
            var serializedCat = new SerializedObject(category);
            var childsProp = serializedCat.FindProperty("_childs");
            for (int i = 0; i < childsProp.arraySize; i++)
            {
                var element = childsProp.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == child)
                {
                    element.objectReferenceValue = null;
                    childsProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            serializedCat.ApplyModifiedProperties();
        }

        static private void AddChild(VinylCategory category, VinylCategory child)
        {
            if (category == null) return;
            var serializedCat = new SerializedObject(category);
            var childsProp = serializedCat.FindProperty("_childs");
            childsProp.InsertArrayElementAtIndex(childsProp.arraySize);
            serializedCat.ApplyModifiedProperties();

            var element = childsProp.GetArrayElementAtIndex(childsProp.arraySize - 1);
            element.objectReferenceValue = child;

            serializedCat.ApplyModifiedProperties();
            serializedCat.Update();
        }

        static private void ReparentCategory(VinylCategory category, VinylCategory parent)
        {
            var serializedCat = new SerializedObject(category);
            serializedCat.FindProperty("_parent").objectReferenceValue = parent;
            serializedCat.ApplyModifiedProperties();
        }

        static public int RemoveCategory(VinylCategory category, bool keepChilds = false) //TODO: REVISAR
        {
            int opt;
            if (category.Childs.Count != 0)
                opt = EditorUtility.DisplayDialogComplex("Removing Category " + category.name, "Are you sure you wanna remove this category?\nThis process is destructive", "Yes", "No", "Yes but keep children");
            else
                opt = EditorUtility.DisplayDialog("Removing Category " + category.name, "Are you sure you wanna remove this category?\nThis process is destructive", "Yes", "No") ? 0 : 1;

            if (opt == 1) return 1;
            if (opt == 0) keepChilds = false;
            else keepChilds = true;

            var parent = category.Parent;

            if (parent == null)
                RemoveBaseCategory(category);

            if (keepChilds)
            {
                foreach (var child in category.Childs)
                    ReparentCategory(child, parent);

                if (parent != null)
                {
                    foreach (var child in category.Childs)
                        AddChild(parent, child);
                    RemoveChild(parent, category);
                }
            }
            else
            {
                if (parent != null)
                    RemoveChild(parent, category);

                foreach (var child in category.Childs)
                    RemoveCategory(child, false);
            }
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(category));

            return opt;
        }

        static public void AddBaseCategory(VinylCategory category)
        {
            var serializedConfig = new SerializedObject(VinylConfig.Current);
            var baseCatProp = serializedConfig.FindProperty("baseCategories");
            baseCatProp.InsertArrayElementAtIndex(baseCatProp.arraySize);
            serializedConfig.ApplyModifiedProperties();
            baseCatProp.GetArrayElementAtIndex(baseCatProp.arraySize - 1).objectReferenceValue = category;
            serializedConfig.ApplyModifiedProperties();
        }

        static public void RemoveBaseCategory(VinylCategory category)
        {
            var serializedConfig = new SerializedObject(VinylConfig.Current);
            var baseCatProp = serializedConfig.FindProperty("baseCategories");
            for (int i = 0; i < baseCatProp.arraySize; i++)
            {
                var element = baseCatProp.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == category)
                {
                    element.objectReferenceValue = null;
                    baseCatProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            serializedConfig.ApplyModifiedProperties();
        }

        static public void RenameCategory(VinylCategory category, string newName)
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(category), newName);
        }

        /// 
        /// ASSETS
        /// 

        static public VinylAsset CreateDefaultAsset(AudioClip clip, string path = "")
        {
            VinylAsset newAsset = ScriptableObject.CreateInstance<VinylAsset>();

            if (clip != null)
            {
                var serializedObj = new SerializedObject(newAsset);
                var clipListProp = serializedObj.FindProperty("randomClips");
                clipListProp.InsertArrayElementAtIndex(0);
                serializedObj.ApplyModifiedProperties();
                clipListProp.GetArrayElementAtIndex(0).FindPropertyRelative("clip").objectReferenceValue = clip;
                clipListProp.GetArrayElementAtIndex(0).FindPropertyRelative("chance").floatValue = 1f;
                serializedObj.ApplyModifiedProperties();
            }

            if (!AssetDatabase.IsValidFolder("Assets/Data")) //TODO: Hacer modificable la ruta
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Sounds"))
                AssetDatabase.CreateFolder("Assets/Data", "Sounds");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Sounds/Default"))
                AssetDatabase.CreateFolder("Assets/Data/Sounds", "Default");

            string assetPath = "";
            if (path == "")
                assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/Sounds/Default/" + (clip != null ? clip.name : ("New " + VinylConstants.ASSET_NAME)) + ".asset");
            else
                assetPath = AssetDatabase.GenerateUniqueAssetPath(path + (clip != null ? clip.name : ("New " + VinylConstants.ASSET_NAME)) + ".asset");

            AssetDatabase.CreateAsset(newAsset, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created " + newAsset.name + " VinylAsset.", newAsset);
            return newAsset;
        }
    }
}