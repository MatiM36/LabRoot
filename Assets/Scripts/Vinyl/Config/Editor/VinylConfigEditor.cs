using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mati36.Vinyl
{
    [CustomEditor(typeof(VinylConfig))]
    public class VinylConfigEditor : Editor
    {
        VinylConfig t;
        ReorderableList categoryList;

        private void OnEnable()
        {
            t = (VinylConfig)target;
            categoryList = new ReorderableList(serializedObject, serializedObject.FindProperty("categoryNames"), true, true, true, true);
            categoryList.drawElementCallback = DrawListElement;
            categoryList.drawHeaderCallback = DrawListHeader;
            categoryList.onCanRemoveCallback = CanRemove;
            //categoryList.onAddCallback = OnAdd;
        }

        //private void OnAdd(ReorderableList list)
        //{
        //    list.serializedProperty.InsertArrayElementAtIndex(list.count);
        //    list.serializedProperty.GetArrayElementAtIndex(list.count - 1).stringValue = "No Name";
        //}

        //public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();

        //    if (GUILayout.Button("Reset to Default"))
        //    { Undo.RecordObject(t, "SoundConfig Reset"); t.ReturnToDefault(); EditorUtility.SetDirty(t); }

        //    categoryList.DoLayoutList();
        //    serializedObject.ApplyModifiedProperties();
        //}

        private void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Categories");
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = categoryList.serializedProperty.GetArrayElementAtIndex(index);
            if (isActive)
                element.stringValue = EditorGUI.TextField(rect, element.stringValue);
            else
                EditorGUI.LabelField(rect, element.stringValue);
        }

        private bool CanRemove(ReorderableList list)
        {
            return list.count > 1  && list.index != 0;
        }
    }
}