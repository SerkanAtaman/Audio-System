using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    public class EditTagsWindow : EditorWindow
    {
        private AudioSystemSettings _settings;
        private SerializedObject _settingsObject;

        private void CreateGUI()
        {
            _settings = AudioSystemEditorUtils.GetSettings();
            _settingsObject = new(_settings);
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(_settingsObject.FindProperty("Tags"), new GUIContent("Tags"));

            _settingsObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssetIfDirty(_settings);
        }
    }
}