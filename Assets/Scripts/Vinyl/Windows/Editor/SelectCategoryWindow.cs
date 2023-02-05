using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;

namespace Mati36.Vinyl
{
    public class SelectCategoryWindow : ScriptableWizard
    {
        static SelectCategoryWindow window;

        static private SerializedObject currentVinylAsset;
        static private SerializedProperty categoryProperty;

        static VinylCategory currentCategory;
        static SerializedObject currentCategorySerializedObj;

        public static SelectCategoryWindow CreateSelectWindow(SerializedObject selectedAsset)
        {
            if (window != null)
                window.Close();
            window = DisplayWizard<SelectCategoryWindow>("Select Category") as SelectCategoryWindow;

            currentVinylAsset = selectedAsset;
            categoryProperty = selectedAsset.FindProperty("category");

            var cat = categoryProperty != null ? categoryProperty.objectReferenceValue as VinylCategory : null;
            window.SetCategory(cat);

            window.maxSize = new Vector2(LEFT_WINDOW_WIDTH + RIGHT_WINDOW_WIDTH + SEPARATOR_WIDTH + 16, 500);
            window.minSize = window.maxSize;


            return window;
        }

        [SerializeField] TreeViewState state;
        CategoryTreeView tree;

        private void OnEnable()
        {
            if (state == null)
                state = new TreeViewState();

            var list = new List<CategoryTreeElement>();
            list.Add(new CategoryTreeElement("ROOT", -1, -1));

            var serializedConfig = new SerializedObject(VinylConfig.Current);
            serializedConfig.Update();
            serializedConfig.ApplyModifiedProperties();

            var catList = new List<VinylCategory>();

            var catProp = serializedConfig.FindProperty("baseCategories");
            for (int i = 0; i < catProp.arraySize; i++)
            {
                var element = catProp.GetArrayElementAtIndex(i);
                if (element == null || element.objectReferenceValue == null) continue;
                catList.Add(element.objectReferenceValue as VinylCategory);
            }


            foreach (var baseCat in catList)
                AddCategoryRecursive(ref list, baseCat);

            var model = new TreeModel<CategoryTreeElement>(list);

            tree = new CategoryTreeView(state, model);
            tree.e_OnDoubleClickedItem += (index) => { SetCategory(model.Find(index).vinylCategory); Accept(); };
        }

        private void AddCategoryRecursive(ref List<CategoryTreeElement> list, VinylCategory currentCat, int depth = 0)
        {
            if (currentCat == null) return;
            list.Add(new CategoryTreeElement(currentCat, currentCat.GetInstanceID(), depth));
            foreach (var childCat in currentCat.Childs)
                AddCategoryRecursive(ref list, childCat, depth + 1);
        }

        const int LEFT_WINDOW_WIDTH = 300;
        const int RIGHT_WINDOW_WIDTH = 500;
        const int SEPARATOR_WIDTH = 50;

        private void OnGUI()
        {
            //if (window == null) { this.Close(); return; } 

            if (categoryElementStyle == null) CreateStyles();

            var col = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            GUI.Box(new Rect(0, 0, LEFT_WINDOW_WIDTH + RIGHT_WINDOW_WIDTH + SEPARATOR_WIDTH + 16, position.height), "");
            GUI.backgroundColor = col;

            GUILayout.BeginArea(new Rect(0, 0, LEFT_WINDOW_WIDTH + RIGHT_WINDOW_WIDTH + SEPARATOR_WIDTH, position.height), windowContainerStyle);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawCategoriesTree();
            if (GUILayout.Button("Select"))
                Accept();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label("");
            //GUILayout.Label("", EditorStyles.helpBox, GUILayout.Width(SEPARATOR_WIDTH), GUILayout.ExpandHeight(true));
            GUILayout.EndVertical();

            #region RIGHT_WINDOW
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawCategoryProperties();
            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }


        Vector2 scrollPos_left, scrollPos_right;
        private void DrawCategoriesTree()
        {
            GUILayout.Label("Categories", EditorStyles.whiteBoldLabel);
            var treeRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            //var treeRect = GUILayoutUtility.GetRect(LEFT_WINDOW_WIDTH - 32, 16, categoryElementStyle);
            tree.OnGUI(treeRect);
            EditorGUILayout.EndVertical();

            if (tree.GetSelection().Count == 0)
                SetCategory(null);
            else
            {
                var selected = tree.treeModel.Find(tree.GetSelection().First());
                SetCategory(selected == null ? null : selected.vinylCategory);
            }
        }

        private void DrawCategoryProperties()
        {

            GUILayout.Label("Category Parameters", EditorStyles.whiteBoldLabel);
            var propertiesRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            if (currentCategory == null) { GUILayout.Label("No category selected", EditorStyles.helpBox); return; }

            currentCategorySerializedObj.Update();

            scrollPos_right = EditorGUILayout.BeginScrollView(scrollPos_right, EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            string newName;
            newName = EditorGUILayout.DelayedTextField("Name", currentCategory.name);
            if (EditorGUI.EndChangeCheck())
                VinylSerializationUtility.RenameCategory(currentCategory, newName);

            if (currentCategory.Parent != null)
            {
                EditorGUI.BeginChangeCheck();
                currentCategory.overrideParent = EditorGUILayout.Toggle("Override Parent", currentCategory.overrideParent);
                if (EditorGUI.EndChangeCheck())
                {
                    //TODO: Pasar a serializableObject
                    EditorUtility.SetDirty(currentCategory);
                    //AssetDatabase.SaveAssets();
                }
            }

            GUI.enabled = currentCategory.overrideParent || currentCategory.Parent == null;
            EditorGUILayout.PropertyField(currentCategorySerializedObj.FindProperty("_outputMixerGroup"));
            GUI.enabled = true;

            currentCategorySerializedObj.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }


        private void Accept()
        {
            categoryProperty.objectReferenceValue = currentCategory;
            currentVinylAsset.ApplyModifiedProperties();
            currentVinylAsset.Update();
            window.Close();
        }

        private void SetCategory(VinylCategory category)
        {
            currentCategory = category;

            currentCategorySerializedObj?.Dispose();
            if (category != null)
                currentCategorySerializedObj = new SerializedObject(category);
        }


        //HELPER
        private string GetCategoryDisplayName(string category)
        {
            string[] catLevels = category.Split('/');
            if (catLevels.Length == 0)
                return "";
            else if (catLevels.Length == 1)
                return category;

            string displayName = "";
            for (int i = 0; i < catLevels.Length; i++)
            {
                if (i < catLevels.Length - 1)
                    displayName += "               ";
                else
                    displayName += "∟";
            }

            displayName += catLevels[catLevels.Length - 1];
            return displayName;
        }

        private string GetSpaces(int spaces)
        {
            string spacesStr = "";
            for (int i = 0; i < spaces; i++)
                spacesStr += "         ∟";
            return spacesStr;
        }

        static GUIStyle categoryElementStyle;
        static GUIStyle categoryElementNameStyle, categoryElementNameStyleSelected;
        static GUIStyle windowContainerStyle;
        static GUIStyle leftContainerStyle, rightContainerStyle;
        static private void CreateStyles()
        {
            categoryElementStyle = new GUIStyle(GUI.skin.box);
            categoryElementStyle.alignment = TextAnchor.MiddleLeft;

            categoryElementNameStyle = new GUIStyle();
            categoryElementNameStyle.padding = new RectOffset(16, 16, 0, 0);

            categoryElementNameStyleSelected = new GUIStyle();
            categoryElementNameStyleSelected.active.textColor = Color.red;
            categoryElementNameStyleSelected.normal.textColor = Color.red;
            categoryElementNameStyleSelected.padding = new RectOffset(16, 16, 0, 0);


            windowContainerStyle = new GUIStyle();
            //windowContainerStyle.padding = new RectOffset(20, 20, 20, 20);
            windowContainerStyle.alignment = TextAnchor.MiddleCenter;
            windowContainerStyle.margin = new RectOffset(0, 0, 0, 0);
            windowContainerStyle.border = new RectOffset(0, 0, 0, 0);


            rightContainerStyle = new GUIStyle(EditorStyles.helpBox);
            //rightContainerStyle.padding = new RectOffset(16, 16, 0, 0);
        }
    }
}