using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioClipContainer))]
    [CanEditMultipleObjects]
    public class AudioClipContainerEditor : UnityEditor.Editor
    {
        private AudioContainerLibrary _library;
        private AudioSystemSettings _settings;
        private int _selectedMultipleCategoryIndex;
        private int _selectedMultipleTagIndex;

        private void OnEnable()
        {
            _selectedMultipleCategoryIndex = 0;
            _selectedMultipleTagIndex = 0;
            _library = AudioContainerLibraryEditorUtils.GetLibrary();
            _settings = AudioSystemEditorUtils.GetSettings();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (targets.Length == 1) DrawSingleInspector();
            else DrawMultipleInspectors();

            serializedObject.ApplyModifiedProperties();

            EditorApplication.delayCall += () =>
            {
                AssetDatabase.SaveAssetIfDirty(target);
            };
        }

        private void DrawSingleInspector()
        {
            AudioClipContainer audioClipContainer = (AudioClipContainer)target;

            AudioContainerLibraryEditorUtils.Update(audioClipContainer, _library);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_id"), new GUIContent("ID", "The unique identifier of container"));
            if (!AudioContainerLibraryEditorUtils.IsIdentifierValid(serializedObject.FindProperty("_id").uintValue, _library))
            {
                serializedObject.FindProperty("_id").uintValue = AudioContainerLibraryEditorUtils.GenerateUniqueIdentifier(_library);
                AudioContainerLibraryEditorUtils.Update(audioClipContainer, _library);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(new GUIContent("Copy ID", "")))
            {
                GUIUtility.systemCopyBuffer = serializedObject.FindProperty("_id").uintValue.ToString();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClip"), new GUIContent("Audio Clip", "The clip that will be played"));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_baseVolume"), new GUIContent("Base Volume", "The base volume of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pitch"), new GUIContent("Pitch", "The default pitch of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_spatialBlend"),
                new GUIContent("Spatial Blend", "Sets how much the audio source is treated as a 3D source.3D sources are affected by spatial position and spread. " +
                                    "if 3D pan level is 0, all spatial attenuation is ignored"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"), new GUIContent("Loop", "Wheter the clip should be looped or not"));

            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    audioClipContainer.RefreshAliveDatas(_settings);
                };
            }

            var categories = AudioSystemEditorUtils.GetAllCategoryNames(_settings);
            var categoryIndex = AudioSystemEditorUtils.GetCategoryNameIndex(categories, serializedObject.FindProperty("_category").stringValue);
            var selectedCat = EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), (int)categoryIndex, categories);

            serializedObject.FindProperty("_category").stringValue = categories[selectedCat];

            var tags = AudioSystemEditorUtils.GetAllTagNames(_settings);
            var tagIndex = AudioSystemEditorUtils.GetTagNameIndex(tags, serializedObject.FindProperty("_tag").stringValue);
            var selectedTag = EditorGUILayout.Popup(new GUIContent("Tag", "The tag of the clip"), (int)tagIndex, tags);

            serializedObject.FindProperty("_tag").stringValue = tags[selectedTag];

            EditorGUI.BeginDisabledGroup(true);

            var selectedCategory = AudioSystemEditorUtils.GetCategoryByName(categories[selectedCat], _settings);
            if (selectedCategory != null)
                serializedObject.FindProperty("_categoryID").uintValue = selectedCategory.Value.ID;
            else
                serializedObject.FindProperty("_categoryID").uintValue = 0;

            serializedObject.FindProperty("_tagID").uintValue = (uint)selectedTag;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_categoryID"), new GUIContent("Category ID", "The int id of the selected category"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_tagID"), new GUIContent("Tag ID", "The int id of the selected tag"));

            EditorGUI.EndDisabledGroup();

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
        }

        private void DrawMultipleInspectors()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_baseVolume"), new GUIContent("Base Volume", "The base volume of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pitch"), new GUIContent("Pitch", "The default pitch of the clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_spatialBlend"),
                new GUIContent("Spatial Blend", "Sets how much the audio source is treated as a 3D source.3D sources are affected by spatial position and spread. " +
                                    "if 3D pan level is 0, all spatial attenuation is ignored"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"), new GUIContent("Loop", "Wheter the clip should be looped or not"));

            var categories = AudioSystemEditorUtils.GetAllCategoryNames(_settings);
            var categoryProperty = serializedObject.FindProperty("_category");

            if (!categoryProperty.hasMultipleDifferentValues)
            {
                var categoryIndex = AudioSystemEditorUtils.GetCategoryNameIndex(categories, serializedObject.FindProperty("_category").stringValue);
                var selected = EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), (int)categoryIndex, categories);
                categoryProperty.stringValue = categories[selected];

                EditorGUI.BeginDisabledGroup(true);
                var selectedCategory = AudioSystemEditorUtils.GetCategoryByName(categories[selected], _settings);

                if (selectedCategory != null)
                    serializedObject.FindProperty("_categoryID").uintValue = selectedCategory.Value.ID;
                else
                    serializedObject.FindProperty("_categoryID").uintValue = 0;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_categoryID"), new GUIContent("Category ID", "The int id of the selected category"));
                EditorGUI.BeginDisabledGroup(true);
            }
            else
            {
                _selectedMultipleCategoryIndex = EditorGUILayout.Popup(new GUIContent("Category", "The category of the clip"), _selectedMultipleCategoryIndex, categories);
                if (_selectedMultipleCategoryIndex != 0)
                {
                    categoryProperty.stringValue = categories[_selectedMultipleCategoryIndex];
                }
            }

            var tags = AudioSystemEditorUtils.GetAllTagNames(_settings);
            var tagProperty = serializedObject.FindProperty("_tag");

            if (!tagProperty.hasMultipleDifferentValues)
            {
                var tagIndex = AudioSystemEditorUtils.GetTagNameIndex(tags, serializedObject.FindProperty("_tag").stringValue);
                var selectedTag = EditorGUILayout.Popup(new GUIContent("Tag", "The tag of the clip"), (int)tagIndex, tags);
                tagProperty.stringValue = tags[selectedTag];

                var tagIdProperty = serializedObject.FindProperty("_tagID");

                EditorGUI.BeginDisabledGroup(true);
                tagIdProperty.uintValue = (uint)selectedTag;
                EditorGUILayout.PropertyField(tagIdProperty, new GUIContent("Tag ID", "The int id of the selected tag"));
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                _selectedMultipleTagIndex = EditorGUILayout.Popup(new GUIContent("Tag", "The tag of the clip"), _selectedMultipleTagIndex, tags);
                if (_selectedMultipleTagIndex != 0)
                {
                    tagProperty.stringValue = tags[_selectedMultipleTagIndex];
                }
            }
        }
    }
}