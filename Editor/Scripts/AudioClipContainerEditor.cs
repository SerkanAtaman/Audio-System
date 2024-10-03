using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioClipContainer))]
    public class AudioClipContainerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClip"), new GUIContent("Audio Clip", "The clip that will be played"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_volume"), new GUIContent("Volume", "The volume of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"), new GUIContent("Loop", "Wheter the clip should be looped or not"));

            var categories = AudioSystemEditorUtils.GetAllCategoryNames();

            var categoryProperty = serializedObject.FindProperty("_category");
            var categoryIndex = AudioSystemEditorUtils.GetCategoryNameIndex(categories, categoryProperty.stringValue);
            EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), categoryIndex, categories);

            EditorGUILayout.Space(10f);

            if(GUILayout.Button(new GUIContent("Edit Categories", "")))
            {
                var wnd = EditorWindow.GetWindow<EditCategoriesWindow>();
                wnd.titleContent = new GUIContent("Categories Window");
            }

            EditorGUILayout.Space(10f);

            if (GUILayout.Button(new GUIContent("Play", "")))
            {
                if (Application.isPlaying)
                {
                    AudioSystemManager.Instance.Play((AudioClipContainer)target);
                }
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(target);
        }
    }
}