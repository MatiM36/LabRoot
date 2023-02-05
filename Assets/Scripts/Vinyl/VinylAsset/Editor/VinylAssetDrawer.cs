using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mati36.Vinyl;
using System;
using System.Linq;

namespace Mati36.VinylEditor
{
    [CustomPropertyDrawer(typeof(VinylAsset))]
    public class VinylAssetDrawer : PropertyDrawer
    {
        int catIndex;
        public SerializedProperty serializedProp;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            serializedProp = property;

            if (property.isInstantiatedPrefab && property.prefabOverride)
                EditorGUI.PrefixLabel(position, label, EditorStyles.boldLabel);
            else
                EditorGUI.PrefixLabel(position, label);
            Rect popupRect = position;
            popupRect.x += EditorGUIUtility.labelWidth;
            popupRect.width -= EditorGUIUtility.labelWidth;
            popupRect.height = EditorGUIUtility.singleLineHeight;



            GUIContent content = new GUIContent(property.objectReferenceValue != null ? property.objectReferenceValue.name : "None");
            if (EditorGUI.DropdownButton(popupRect, content, FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();
                AddMenuItem(menu, "None", null);
                int itemCount = 0;
                foreach(var asset in VinylConfig.VinylAssets)
                {
                    //Debug.Log("ASSET " + asset.name + " FOUND", asset);
                    AddMenuItem(menu, asset.category?.CategoryPath + asset.name, asset);
                    itemCount++;
                }
                if (itemCount == 0)
                    AddMenuItem(menu, "No VinylAssets found", null);
                menu.DropDown(popupRect);
            }

            HandleEvents(position, property, label);
        }

        private void HandleEvents(Rect position, SerializedProperty property, GUIContent label)
        {
            Event e = Event.current;

            if (!position.Contains(e.mousePosition)) return;

            if (e.type == EventType.MouseDown && e.button == 1)
            {
                GenericMenu contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("Clear Field"), false, () => { serializedProp.objectReferenceValue = null; serializedProp.serializedObject.ApplyModifiedProperties(); });
                contextMenu.AddItem(new GUIContent("Select VinylAsset"), false, () => Selection.activeObject = property.objectReferenceValue);
                contextMenu.AddItem(new GUIContent("Select Audio Clip"), false, () => Selection.activeObject = ((VinylAsset)property.objectReferenceValue).Clip);
                contextMenu.ShowAsContext();
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            if (e.type == EventType.DragPerform)
            {
                VinylAsset draggedAsset = DragAndDrop.objectReferences[0] as VinylAsset;
                if (draggedAsset != null)
                {
                    serializedProp.objectReferenceValue = draggedAsset;
                    serializedProp.serializedObject.ApplyModifiedProperties();
                }
            }

        }

        private void AddMenuItem(GenericMenu menu, string menuPath, VinylAsset asset)
        {
            menu.AddItem(new GUIContent(menuPath), false, OnItemSelected, asset);
        }

        private void OnItemSelected(object itemSelected)
        {
            serializedProp.objectReferenceValue = (UnityEngine.Object)itemSelected ?? null;
            serializedProp.serializedObject.ApplyModifiedProperties();
        }
    }
}