using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mati36.Vinyl
{
    public class AddCategoryWindow : EditorWindow
    {

        private void OnEnable()
        {

        }

        string catName;
        private void OnGUI()
        {
            if (!active) { Close(); return; }

            GUI.SetNextControlName("CatNameField");
            catName = EditorGUILayout.TextField("Category Name", catName);

            EditorGUI.FocusTextInControl("CatNameField");

            GUI.enabled = (catName != null && catName != "");
            
            if (GUILayout.Button("Create"))
            {
                VinylSerializationUtility.CreateCategory(catName, parentCat);
                Close();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }

        private void OnDisable()
        {
            active = false;
        }

        private VinylCategory parentCat;
        static private bool active = false;

        public void SetCategoryParent(VinylCategory cat)
        {
            parentCat = cat;
            active = true;
        }
    }
}
