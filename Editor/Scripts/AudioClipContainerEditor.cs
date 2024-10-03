using System;
using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioClipContainer))]
    [CanEditMultipleObjects]
    public class AudioClipContainerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (targets.Length == 1) DrawSingleInspector();
            else DrawMultipleInspectors();
        }

        private void DrawSingleInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_identifier"), new GUIContent("Identifier", "The unique identifier of container"));

            if (string.IsNullOrEmpty(serializedObject.FindProperty("_identifier").stringValue))
                serializedObject.FindProperty("_identifier").stringValue = Guid.NewGuid().ToString();
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClip"), new GUIContent("Audio Clip", "The clip that will be played"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_volume"), new GUIContent("Volume", "The volume of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"), new GUIContent("Loop", "Wheter the clip should be looped or not"));

            var categories = AudioSystemEditorUtils.GetAllCategoryNames();

            var categoryIndex = AudioSystemEditorUtils.GetCategoryNameIndex(categories, serializedObject.FindProperty("_category").stringValue);
            var selected = EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), categoryIndex, categories);
            serializedObject.FindProperty("_category").stringValue = categories[selected];

            EditorGUILayout.Space(10f);

            if (GUILayout.Button(new GUIContent("Edit Categories", "")))
            {
                var wnd = EditorWindow.GetWindow<EditCategoriesWindow>();
                wnd.titleContent = new GUIContent("Categories Window");
            }
            if (GUILayout.Button(new GUIContent("Generate ID", "")))
            {
                serializedObject.FindProperty("_identifier").stringValue = Guid.NewGuid().ToString();
                AudioContainerLibraryEditorUtils.Update((AudioClipContainer)target);
            }
            if (GUILayout.Button(new GUIContent("Copy ID", "")))
            {
                GUIUtility.systemCopyBuffer = serializedObject.FindProperty("_identifier").stringValue;
            }

            EditorApplication.delayCall += () =>
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssetIfDirty(target);
            };
        }

        private void DrawMultipleInspectors()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_volume"), new GUIContent("Volume", "The volume of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"), new GUIContent("Loop", "Wheter the clip should be looped or not"));

            var categories = AudioSystemEditorUtils.GetAllCategoryNames();

            var categoryIndex = AudioSystemEditorUtils.GetCategoryNameIndex(categories, serializedObject.FindProperty("_category").stringValue);
            var selected = EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), categoryIndex, categories);
            serializedObject.FindProperty("_category").stringValue = categories[selected];

            EditorApplication.delayCall += () =>
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssetIfDirty(target);
            };
        }
    }
}