using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioClipPlayer))]
    public class AudioClipPlayerEditor : UnityEditor.Editor
    {
        private double _lastIdentifierCheckTime;
        private bool _lastIdentifierValidationResult;
        private bool _eventsFoldout = false;

        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            _lastIdentifierCheckTime = EditorApplication.timeSinceStartup - 10;
            return base.CreateInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            if (targets.Length > 1) return;

            AudioClipPlayer player = (AudioClipPlayer)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"), new GUIContent("Play Type", "Decide how this audio player chooses it's container to use"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayMode"), new GUIContent("Play Mode", "Decide if this player should start playing automatically"));

            EditorGUI.BeginChangeCheck();
            var volumeProperty = serializedObject.FindProperty("_volume");
            EditorGUILayout.Slider(volumeProperty, 0f, 1f, new GUIContent("Volume", "The current volume multiplier"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                player.Volume = volumeProperty.floatValue;
            }

            var typeEnumIndex = serializedObject.FindProperty("Type").enumValueIndex;

            if (typeEnumIndex == 0) // Container
            {
                serializedObject.FindProperty("ContainerIDs").ClearArray();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Containers"), new GUIContent("Containers", "The Audio Clip Containers this player will use"));
            }
            else if (typeEnumIndex == 1) // ContainerWithID
            {
                serializedObject.FindProperty("Containers").ClearArray();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ContainerIDs"), new GUIContent("Container IDs", "The array of Unique ids of audio clip container"));

                if (!CheckIdentifiers(player))
                {
                    EditorGUILayout.HelpBox("Invalid Identifier", MessageType.Warning);
                }
            }
            else if (typeEnumIndex == 2) // RandomByTag
            {
                serializedObject.FindProperty("ContainerIDs").ClearArray();
                serializedObject.FindProperty("Containers").ClearArray();
                var tags = AudioSystemEditorUtils.GetAllTagNames();

                var tagID = serializedObject.FindProperty("TagID").uintValue;
                var selectedTag = EditorGUILayout.Popup(new GUIContent("Tag", "The tag of the container"), (int)tagID, tags);
                serializedObject.FindProperty("TagID").uintValue = (uint)selectedTag;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Effects"),
                new GUIContent("Effects", "Add any amount of audio clip effect you wish to apply when this player starts playing"));

            //Events
            _eventsFoldout = EditorGUILayout.Foldout(_eventsFoldout, new GUIContent("Events"));
            if (_eventsFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStateUpdated"),
                new GUIContent("On State Updated", "Called every time the state of the player is changed"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("SyncAudioSourceTransform"),
                new GUIContent("Sync Audio Source Transform", "Decide whether the audio source that will be used to play this clip should be in the same position with this player"));

            var allowSimultaneousPlayProperty = serializedObject.FindProperty("AllowSimultaneousPlay");
            EditorGUILayout.PropertyField(allowSimultaneousPlayProperty, 
                new GUIContent("Allow Simultaneous Play", "Whether the player should play multiple clips simultaneously or play them on after another"));
            if (allowSimultaneousPlayProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ChooseClipsRespectively"),
                new GUIContent("Choose Clips Respectively", "Decide whether the clips from collection should be choosen randomly or respectively"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("DestroyPlayerWhenStopped"),
                new GUIContent("Destroy Player When Stopped", "Decide if the player gameobject should be destroyed when it's clip is stopped playing"));

            EditorGUILayout.Space(10);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_state"),
                new GUIContent("Current State"));
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

        private bool CheckIdentifiers(AudioClipPlayer player)
        {
            if (EditorApplication.timeSinceStartup - _lastIdentifierCheckTime < 1f) return _lastIdentifierValidationResult;

            _lastIdentifierCheckTime = EditorApplication.timeSinceStartup;

            bool result = true;

            if (player.ContainerIDs == null)
            {
                _lastIdentifierValidationResult = true;
                return _lastIdentifierValidationResult;
            }

            foreach(var id in player.ContainerIDs)
            {
                if (!AudioContainerLibraryEditorUtils.LibraryContainsIdentifier(id))
                {
                    result = false;
                    break;
                }
            }

            _lastIdentifierValidationResult = result;
            return _lastIdentifierValidationResult;
        }
    }
}