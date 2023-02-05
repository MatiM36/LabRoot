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
    public class ManageCategoryWindow : EditorWindow
    {
        static ManageCategoryWindow window;

        //static private SerializedObject currentVinylAsset;
        //static private SerializedProperty categoryProperty;

        static VinylCategory currentCategory;
        static SerializedObject currentCategorySerializedObj;

        [MenuItem("Vinyl/Manage Categories")]
        public static void CreateSelectWindow()
        {
            if (window != null)
                window.Close();
            window = GetWindow<ManageCategoryWindow>("Manage Categories") as ManageCategoryWindow;
            window.Show();
            window.maxSize = new Vector2(LEFT_WINDOW_WIDTH + RIGHT_WINDOW_WIDTH + SEPARATOR_WIDTH + 16, 500);
            window.minSize = window.maxSize;
        }

        [SerializeField] TreeViewState state;
        CategoryTreeView tree;

        private void OnEnable()
        {
            if (state == null)
                state = new TreeViewState();

            var list = new List<CategoryTreeElement>();
            list.Add(new CategoryTreeElement("ROOT", -1, -1));
            foreach (var baseCat in VinylConfig.Current.baseCategories)
                AddCategoryRecursive(ref list, baseCat);

            var model = new TreeModel<CategoryTreeElement>(list);

            tree = new CategoryTreeView(state, model);
        }

        private void AddCategoryRecursive(ref List<CategoryTreeElement> list, VinylCategory currentCat, int depth = 0)
        {
            list.Add(new CategoryTreeElement(currentCat, currentCat.GetInstanceID(), depth));
            foreach (var childCat in currentCat.Childs)
                AddCategoryRecursive(ref list, childCat, depth + 1);
        }

        const int LEFT_WINDOW_WIDTH = 300;
        const int RIGHT_WINDOW_WIDTH = 500;
        const int SEPARATOR_WIDTH = 50;

        private void OnGUI()
        {
            if (categoryElementStyle == null)
                CreateStyles();

            var col = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            GUI.Box(new Rect(0, 0, LEFT_WINDOW_WIDTH + RIGHT_WINDOW_WIDTH + SEPARATOR_WIDTH + 16, position.height), "");
            GUI.backgroundColor = col;


            GUILayout.BeginArea(new Rect(0, 0, LEFT_WINDOW_WIDTH + RIGHT_WINDOW_WIDTH + SEPARATOR_WIDTH, position.height), windowContainerStyle);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawCategoriesTree();
            //if (GUILayout.Button("Accept"))
            //    Accept();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label("");
            GUILayout.EndVertical();
            #region RIGHT_WINDOW
            GUILayout.BeginVertical(GUILayout.Width(RIGHT_WINDOW_WIDTH - 16));
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


            GUILayout.Space(16);
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(300));
            var boxRect = GUILayoutUtility.GetLastRect();
            var titleRect = new Rect(boxRect);
            titleRect.height = 16;
            GUI.Box(titleRect, GUIContent.none);
            GUI.Label(titleRect, "Child Categories", EditorStyles.boldLabel);

            foreach (var subCat in currentCategory.Childs)
            {
                titleRect.position += new Vector2(0, 16);
                GUI.Label(titleRect, subCat.name);
            }

            if (GUILayout.Button("Add Child Category"))
                AddCategory();


            currentCategorySerializedObj.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();
        }

        private void SetCategory(VinylCategory category)
        {
            currentCategory = category;

            currentCategorySerializedObj?.Dispose();
            if (category != null)
                currentCategorySerializedObj = new SerializedObject(category);
        }

        private void AddCategory()
        {
            var addCatWindow = AddCategoryWindow.CreateInstance<AddCategoryWindow>();
            addCatWindow.titleContent = new GUIContent("Create Sub-Category of " + currentCategory.name);
            addCatWindow.SetCategoryParent(currentCategory);
            addCatWindow.ShowUtility();
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