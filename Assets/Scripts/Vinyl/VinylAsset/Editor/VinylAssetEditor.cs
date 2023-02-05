using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mati36.Vinyl;
using System;
using UnityEditorInternal;
using Mati36.VinylExtensions;

namespace Mati36.VinylEditor
{
    [CustomEditor(typeof(VinylAsset)), CanEditMultipleObjects]
    public class VinylAssetEditor : Editor
    {
        VinylAsset t;

        SerializedProperty clipProperty, catProperty;
        SerializedProperty avoidRepeatProperty;
        SerializedProperty loopProperty, volRandomProperty, pitchRandomProperty;
        SerializedProperty volProperty, volRangeProperty, pitchProperty, pitchRangeProperty;
        SerializedProperty modeProperty;

        VinylAudioSource previewSource;

        private void OnEnable()
        {

            t = target as VinylAsset;

            clipProperty = serializedObject.FindProperty("clip");
            catProperty = serializedObject.FindProperty("category");
            avoidRepeatProperty = serializedObject.FindProperty("avoidClipRepeat");
            loopProperty = serializedObject.FindProperty("loop");

            volProperty = serializedObject.FindProperty("vol");
            volRandomProperty = serializedObject.FindProperty("volRandomBetweenRanges");
            volRangeProperty = serializedObject.FindProperty("volRange");

            pitchProperty = serializedObject.FindProperty("pitch");
            pitchRandomProperty = serializedObject.FindProperty("pitchRandomBetweenRanges");
            pitchRangeProperty = serializedObject.FindProperty("pitchRange");

            modeProperty = serializedObject.FindProperty("soundMode");

            clipList = new ReorderableList(serializedObject, serializedObject.FindProperty("randomClips"), true, true, true, true);
            clipList.drawHeaderCallback = ClipListHeaderGUI;
            clipList.drawElementCallback = ClipListElementGUI;

            clipList.onAddCallback = ClipListAddCallback;
            clipList.onRemoveCallback = ClipListRemoveCallback;
            clipList.onCanRemoveCallback = ClipListCanRemoveCallback;


            clipList.drawFooterCallback += ClipListFooterGUI;
            //CreateStyles();

            EditorApplication.update += UpdatePreviewSource;
        }

        private void UpdatePreviewSource()
        {
            if (previewSource != null)
            {
                if (!previewSource.IsPaused && !previewSource.IsPlaying)
                    previewSource.StopSource();
            }
        }

        public override void OnInspectorGUI()
        {
            if (categoryLabelStyle == null) CreateStyles();

            var color = GUI.backgroundColor;

            if (!isPlayingPreview)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Preview Sound".Bold().Colored(Color.white), previewBtnStyle))
                {
                    if (previewSource == null)
                    {
                        previewSource = new GameObject("Preview Vinyl Source", typeof(VinylAudioSource)).GetComponent<VinylAudioSource>();
                        previewSource.gameObject.hideFlags = HideFlags.HideAndDontSave;
                        previewSource.Initialize();
                        previewSource.e_OnEndSound += (src) => { isPlayingPreview = false; Repaint(); };
                    }
                    isPlayingPreview = true;
                    previewSource.PlayAudio(target as VinylAsset, SoundMode.Mode2D);
                }
            }
            else
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Stop Sound".Bold().Colored(Color.white), previewBtnStyle))
                {
                    if (previewSource != null)
                        previewSource.StopSource();
                    isPlayingPreview = false;
                }
            }
            GUI.backgroundColor = color;
            GUILayout.Space(16);

            EditorGUI.BeginDisabledGroup(isPlayingPreview);

            DrawClips();

            DrawCategory();
            DrawSoundParams();

            EditorGUI.EndDisabledGroup();

            //DrawPropertiesExcluding(serializedObject, "m_Script", "clip", "category");

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private bool isPlayingPreview = false;

        private void OnDisable()
        {
            if (previewSource != null)
                DestroyImmediate(previewSource.gameObject);
            EditorApplication.update -= UpdatePreviewSource;
        }
        //
        //CLIP LIST
        //
        ReorderableList clipList;
        private void DrawClips()
        {
            clipList.DoLayoutList();
            if (clipList.serializedProperty.arraySize > 1)
                EditorGUILayout.PropertyField(avoidRepeatProperty);
            GUILayout.Space(16);
        }
        private void ClipListHeaderGUI(Rect rect)
        {
            Rect clipTitle = new Rect(rect);
            clipTitle.x += 16;
            clipTitle.width = (rect.width * 0.7f);
            EditorGUI.LabelField(clipTitle, "Clip");

            Rect chanceTitle = new Rect(rect);
            chanceTitle.width -= clipTitle.width - 8;
            chanceTitle.x = clipTitle.width + 16 + 8;
            EditorGUI.LabelField(chanceTitle, "Chance");
        }
        private void ClipListElementGUI(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = clipList.serializedProperty.GetArrayElementAtIndex(index);
            var clip = element.FindPropertyRelative("clip");
            var chance = element.FindPropertyRelative("chance");
            EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width * 0.7f, EditorGUIUtility.singleLineHeight), clip, GUIContent.none);
            if (clipList.serializedProperty.arraySize > 1)
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), chance, GUIContent.none);
            else
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), "Always");
        }
        private void ClipListAddCallback(ReorderableList list)
        {
            list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
            var lastElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            lastElement.FindPropertyRelative("chance").floatValue = 1;
            list.serializedProperty.serializedObject.ApplyModifiedProperties();

            list.displayRemove = list.serializedProperty.arraySize > 1;
        }

        private void ClipListRemoveCallback(ReorderableList list)
        {
            int indexToRemove = clipList.index == -1 ? list.serializedProperty.arraySize - 1 : list.index;

            list.serializedProperty.DeleteArrayElementAtIndex(indexToRemove);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();

            list.displayRemove = !(list.serializedProperty.arraySize == 1);
        }

        private bool ClipListCanRemoveCallback(ReorderableList list)
        {
            return list.serializedProperty.arraySize > 1;
        }

        private void ClipListFooterGUI(Rect rect)
        {
            float rightEdge = rect.xMax;
            float leftEdge = rightEdge - 8f;
            if (clipList.displayAdd)
                leftEdge -= 25;
            if (clipList.displayRemove)
                leftEdge -= 25;
            rect = new Rect(leftEdge, rect.y, rightEdge - leftEdge, rect.height);
            Rect addRect = new Rect(leftEdge + 4, rect.y - 3, 25, 13);
            Rect removeRect = new Rect(rightEdge - 29, rect.y - 3, 25, 13);
            if (Event.current.type == EventType.Repaint)
            {
                ReorderableList.defaultBehaviours.footerBackground.Draw(rect, false, false, false, false);
            }
            if (clipList.displayAdd)
            {
                using (new EditorGUI.DisabledScope(
                    clipList.onCanAddCallback != null && !clipList.onCanAddCallback(clipList)))
                {
                    if (GUI.Button(addRect, clipList.onAddDropdownCallback != null ? ReorderableList.defaultBehaviours.iconToolbarPlusMore : ReorderableList.defaultBehaviours.iconToolbarPlus, ReorderableList.defaultBehaviours.preButton))
                    {
                        if (clipList.onAddDropdownCallback != null)
                            clipList.onAddDropdownCallback(addRect, clipList);
                        else if (clipList.onAddCallback != null)
                            clipList.onAddCallback(clipList);
                        else
                            ReorderableList.defaultBehaviours.DoAddButton(clipList);

                        if (clipList.onChangedCallback != null)
                            clipList.onChangedCallback(clipList);
                    }
                }
            }
            if (clipList.displayRemove)
            {
                using (new EditorGUI.DisabledScope(
                    (clipList.onCanRemoveCallback != null && !clipList.onCanRemoveCallback(clipList))))
                {
                    if (GUI.Button(removeRect, ReorderableList.defaultBehaviours.iconToolbarMinus, ReorderableList.defaultBehaviours.preButton))
                    {
                        if (clipList.onRemoveCallback == null)
                            ReorderableList.defaultBehaviours.DoRemoveButton(clipList);
                        else
                            clipList.onRemoveCallback(clipList);

                        if (clipList.onChangedCallback != null)
                            clipList.onChangedCallback(clipList);
                    }
                }
            }
        }

        //

        private void DrawCategory()
        {
            string categoryText = "";
            if (t.category == null)
                categoryText = "None".Bold();
            else
            {
                string[] splittedText = t.category.CategoryPath.Split('/');
                int i;
                for (i = 0; i < splittedText.Length - 2; i++)
                {
                    categoryText += splittedText[i] + "/";
                }
                categoryText += splittedText[i].Bold();

            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" Category");

            if (GUILayout.Button(categoryText, categoryLabelStyle))
                SelectCategoryWindow.CreateSelectWindow(serializedObject);
            GUILayout.EndHorizontal();
        }

        float minVol, maxVol;
        private void DrawSoundParams()
        {
            EditorGUILayout.PropertyField(loopProperty);
            DrawVolume();
            GUILayout.Space(16);
            DrawPitch();
        }


        private void DrawVolume()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (volRandomProperty.boolValue)
            {
                minVol = volRangeProperty.vector2Value.x; maxVol = volRangeProperty.vector2Value.y;
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("VolumeRange");
                minVol = EditorGUILayout.FloatField(minVol);
                maxVol = EditorGUILayout.FloatField(maxVol);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                volRandomProperty.boolValue = EditorGUILayout.ToggleLeft("Random", volRandomProperty.boolValue, GUILayout.MaxWidth(120));
                EditorGUILayout.MinMaxSlider(ref minVol, ref maxVol, 0f, 1f);
                volRangeProperty.vector2Value = new Vector2(minVol, maxVol);

                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Volume");
                volProperty.floatValue = EditorGUILayout.FloatField(volProperty.floatValue);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                volRandomProperty.boolValue = EditorGUILayout.ToggleLeft("Random", volRandomProperty.boolValue, GUILayout.MaxWidth(120));
                var r = GUILayoutUtility.GetRect(50, 500, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                volProperty.floatValue = GUI.HorizontalSlider(r, volProperty.floatValue, 0f, 1f);

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawPitch()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (pitchRandomProperty.boolValue)
            {
                minVol = pitchRangeProperty.vector2Value.x; maxVol = pitchRangeProperty.vector2Value.y;
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("PitchRange");
                minVol = EditorGUILayout.FloatField(minVol);
                maxVol = EditorGUILayout.FloatField(maxVol);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                pitchRandomProperty.boolValue = EditorGUILayout.ToggleLeft("Random", pitchRandomProperty.boolValue, GUILayout.MaxWidth(120));
                EditorGUILayout.MinMaxSlider(ref minVol, ref maxVol, 0f, 3f);
                pitchRangeProperty.vector2Value = new Vector2(minVol, maxVol);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Pitch");
                pitchProperty.floatValue = EditorGUILayout.FloatField(pitchProperty.floatValue);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                pitchRandomProperty.boolValue = EditorGUILayout.ToggleLeft("Random", pitchRandomProperty.boolValue, GUILayout.MaxWidth(120));
                var r = GUILayoutUtility.GetRect(50, 500, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                pitchProperty.floatValue = GUI.HorizontalSlider(r, pitchProperty.floatValue, 0f, 3f);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        GUIStyle categoryLabelStyle;
        GUIStyle previewBtnStyle;
        private void CreateStyles()
        {
            categoryLabelStyle = new GUIStyle(EditorStyles.toolbarButton);
            categoryLabelStyle.richText = true;
            categoryLabelStyle.alignment = TextAnchor.MiddleLeft;

            previewBtnStyle = new GUIStyle(categoryLabelStyle);
            previewBtnStyle.alignment = TextAnchor.MiddleCenter;
        }
    }
}