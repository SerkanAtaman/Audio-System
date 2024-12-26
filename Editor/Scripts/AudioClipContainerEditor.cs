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
            AudioClipContainer audioClipContainer = (AudioClipContainer)target;

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            AudioContainerLibraryEditorUtils.Update(audioClipContainer);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_id"), new GUIContent("ID", "The unique identifier of container"));
            if (!AudioContainerLibraryEditorUtils.IsIdentifierValid(serializedObject.FindProperty("_id").uintValue))
            {
                serializedObject.FindProperty("_id").uintValue = AudioContainerLibraryEditorUtils.GenerateUniqueIdentifier();
                AudioContainerLibraryEditorUtils.Update(audioClipContainer);
            }

            GUI.enabled = true;

            if (GUILayout.Button(new GUIContent("Copy ID", "")))
            {
                GUIUtility.systemCopyBuffer = serializedObject.FindProperty("_id").uintValue.ToString();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClip"), new GUIContent("Audio Clip", "The clip that will be played"));

            EditorGUI.BeginChangeCheck();

            var currentVolumeProperty = serializedObject.FindProperty("_currentVolume");
            var maxVolumeProperty = serializedObject.FindProperty("_maxVolume");

            EditorGUILayout.PropertyField(currentVolumeProperty, new GUIContent("Volume", "The volume of the clip"));
            EditorGUILayout.PropertyField(maxVolumeProperty, new GUIContent("Max Volume", "The max possible volume of the clip"));

            if (EditorGUI.EndChangeCheck())
            {
                if (currentVolumeProperty.floatValue >= maxVolumeProperty.floatValue) currentVolumeProperty.floatValue = maxVolumeProperty.floatValue;
                if (Application.isPlaying) audioClipContainer.RefreshVolume(AudioSystemEditorUtils.GetSettings());
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pitch"), new GUIContent("Pitch", "The default pitch of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_spatialBlend"), 
                new GUIContent("Spatial Blend", "Sets how much the audio source is treated as a 3D source.3D sources are affected by spatial position and spread. " +
                                    "if 3D pan level is 0, all spatial attenuation is ignored"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"), new GUIContent("Loop", "Wheter the clip should be looped or not"));

            var categories = AudioSystemEditorUtils.GetAllCategoryNames();

            var categoryIndex = AudioSystemEditorUtils.GetCategoryNameIndex(categories, serializedObject.FindProperty("_category").stringValue);
            var selectedCat = EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), (int)categoryIndex, categories);
            serializedObject.FindProperty("_category").stringValue = categories[selectedCat];

            var tags = AudioSystemEditorUtils.GetAllTagNames();

            var tagIndex = AudioSystemEditorUtils.GetTagNameIndex(tags, serializedObject.FindProperty("_tag").stringValue);
            var selectedTag = EditorGUILayout.Popup(new GUIContent("Tag", "The tag of the clip"), (int)tagIndex, tags);
            serializedObject.FindProperty("_tag").stringValue = tags[selectedTag];

            GUI.enabled = false;
            var selectedCategory = AudioSystemEditorUtils.GetCategoryByName(categories[selectedCat]);
            if (selectedCategory != null)
                serializedObject.FindProperty("_categoryID").uintValue = selectedCategory.Value.ID;
            else
                serializedObject.FindProperty("_categoryID").uintValue = 0;

            serializedObject.FindProperty("_tagID").uintValue = (uint)selectedTag;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_categoryID"), new GUIContent("Category ID", "The int id of the selected category"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_tagID"), new GUIContent("Tag ID", "The int id of the selected tag"));
            GUI.enabled = true;

            EditorGUILayout.Space(10f);

            if (GUILayout.Button(new GUIContent("Edit Categories", "")))
            {
                var wnd = EditorWindow.GetWindow<EditCategoriesWindow>();
                wnd.titleContent = new GUIContent("Categories Window");
            }
            if (GUILayout.Button(new GUIContent("Edit Tags", "")))
            {
                var wnd = EditorWindow.GetWindow<EditTagsWindow>();
                wnd.titleContent = new GUIContent("Tags Window");
            }

            EditorApplication.delayCall += () =>
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssetIfDirty(target);
            };
        }

        private void DrawMultipleInspectors()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_currentVolume"), new GUIContent("Volume", "The volume of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxVolume"), new GUIContent("Max Volume", "The max possible volume of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pitch"), new GUIContent("Pitch", "The default pitch of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"), new GUIContent("Loop", "Wheter the clip should be looped or not"));

            var categories = AudioSystemEditorUtils.GetAllCategoryNames();

            var categoryIndex = AudioSystemEditorUtils.GetCategoryNameIndex(categories, serializedObject.FindProperty("_category").stringValue);
            var selected = EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), (int)categoryIndex, categories);
            serializedObject.FindProperty("_category").stringValue = categories[selected];

            var tags = AudioSystemEditorUtils.GetAllTagNames();

            var tagIndex = AudioSystemEditorUtils.GetTagNameIndex(tags, serializedObject.FindProperty("_tag").stringValue);
            var selectedTag = EditorGUILayout.Popup(new GUIContent("Tag", "The tag of the clip"), (int)tagIndex, tags);
            serializedObject.FindProperty("_tag").stringValue = tags[selectedTag];

            EditorApplication.delayCall += () =>
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssetIfDirty(target);
            };
        }
    }
}