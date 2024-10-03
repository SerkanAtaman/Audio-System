using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioClipPlayer))]
    public class AudioClipPlayerEditor : UnityEditor.Editor
    {
        private double _lastIdentifierCheckTime;
        private bool _lastIdentifierValidationResult;

        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            _lastIdentifierCheckTime = EditorApplication.timeSinceStartup - 10;
            return base.CreateInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            if (targets.Length > 1) return;

            AudioClipPlayer player = (AudioClipPlayer)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseContainerID"), new GUIContent("Use Container ID", "Decide whether to use the container if or the container asset reference"));

            if (serializedObject.FindProperty("UseContainerID").boolValue)
            {
                serializedObject.FindProperty("Container").objectReferenceValue = null;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ContainerID"), new GUIContent("Container Identifier", "The Unique Identifier of audio clip container"));

                if (!CheckIdentifier(serializedObject.FindProperty("ContainerID").stringValue))
                {
                    EditorGUILayout.HelpBox("Invalid Identifier", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Container"), new GUIContent("Container", "The Audio Clip Container this player will use"));
                serializedObject.FindProperty("ContainerID").stringValue = string.Empty;
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SyncAudioSourceTransform"), 
                new GUIContent("Sync Audio Source Transform", "Decide whether the audio source that will be used to play this clip should be in the same position with this player"));

            EditorGUILayout.Space(10);

            GUI.enabled = false;
            EditorGUILayout.TextField("Is Playing", player.IsPlaying.ToString());
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Play", "")) && Application.isPlaying)
            {
                player.Play();
            }
            if (GUILayout.Button(new GUIContent("Pause", "")) && Application.isPlaying)
            {
                player.Pause();
            }
            if (GUILayout.Button(new GUIContent("Resume", "")) && Application.isPlaying)
            {
                player.Resume();
            }
            if (GUILayout.Button(new GUIContent("Stop", "")) && Application.isPlaying)
            {
                player.Stop();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool CheckIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                _lastIdentifierValidationResult = false;
                return _lastIdentifierValidationResult;
            }
            if(identifier.Length < 5)
            {
                _lastIdentifierValidationResult = false;
                return _lastIdentifierValidationResult;
            }
            if (EditorApplication.timeSinceStartup - _lastIdentifierCheckTime < 2f) return _lastIdentifierValidationResult;

            _lastIdentifierCheckTime = EditorApplication.timeSinceStartup;
            _lastIdentifierValidationResult = AudioContainerLibraryEditorUtils.LibraryContainsIdentifier(identifier);
            return _lastIdentifierValidationResult;
        }
    }
}