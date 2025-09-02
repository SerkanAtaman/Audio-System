using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioClipPlayer))]
    [CanEditMultipleObjects]
    public class AudioClipPlayerEditor : UnityEditor.Editor
    {
        SerializedProperty _typeProperty;
        SerializedProperty _playModeProperty;
        SerializedProperty _volumeProperty;
        SerializedProperty _customAudioSourceProperty;
        SerializedProperty _containersIdsProperty;
        SerializedProperty _containersProperty;
        SerializedProperty _tagIDProperty;
        SerializedProperty _effectsProperty;
        SerializedProperty _onPlayStartedProperty;
        SerializedProperty _onPausedProperty;
        SerializedProperty _onStoppedProperty;
        SerializedProperty _onPlayFinishedProperty;
        SerializedProperty _syncAudioSourceTransformProperty;
        SerializedProperty _allowSimultaneousPlayProperty;
        SerializedProperty _limitMaxAudioSourceProperty;
        SerializedProperty _maxAudioSourceProperty;
        SerializedProperty _chooseClipsRespectivelyProperty;
        SerializedProperty _clearRespectiveDataWhenStoppedProperty;
        SerializedProperty _destroyPlayerWhenStoppedProperty;
        SerializedProperty _stateProperty;
        SerializedProperty _respectiveContainersProperty;

        AudioContainerLibrary _library;
        AudioSystemSettings _settings;

        private double _lastIdentifierCheckTime;
        private bool _lastIdentifierValidationResult;
        private static bool _eventsFoldout = false;
        private static bool _debugFoldout = false;

        private void OnEnable()
        {
            _typeProperty = serializedObject.FindProperty("Type");
            _playModeProperty = serializedObject.FindProperty("PlayMode");
            _volumeProperty = serializedObject.FindProperty("_volume");
            _customAudioSourceProperty = serializedObject.FindProperty("CustomAudioSource");
            _containersIdsProperty = serializedObject.FindProperty("ContainerIDs");
            _containersProperty = serializedObject.FindProperty("Containers");
            _tagIDProperty = serializedObject.FindProperty("TagID");
            _effectsProperty = serializedObject.FindProperty("Effects");
            _onPlayStartedProperty = serializedObject.FindProperty("OnPlayStarted");
            _onPausedProperty = serializedObject.FindProperty("OnPaused");
            _onStoppedProperty = serializedObject.FindProperty("OnStopped");
            _onPlayFinishedProperty = serializedObject.FindProperty("OnPlayFinished");
            _syncAudioSourceTransformProperty = serializedObject.FindProperty("SyncAudioSourceTransform");
            _allowSimultaneousPlayProperty = serializedObject.FindProperty("AllowSimultaneousPlay");
            _limitMaxAudioSourceProperty = serializedObject.FindProperty("LimitMaxAudioSource");
            _maxAudioSourceProperty = serializedObject.FindProperty("MaxAudioSource");
            _chooseClipsRespectivelyProperty = serializedObject.FindProperty("ChooseClipsRespectively");
            _clearRespectiveDataWhenStoppedProperty = serializedObject.FindProperty("ClearRespectiveDataWhenStopped");
            _destroyPlayerWhenStoppedProperty = serializedObject.FindProperty("DestroyPlayerWhenStopped");
            _stateProperty = serializedObject.FindProperty("_state");
            _respectiveContainersProperty = serializedObject.FindProperty("_respectiveContainers");

            _library = AudioContainerLibraryEditorUtils.GetLibrary();
            _settings = AudioSystemEditorUtils.GetSettings();
        }

        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            _lastIdentifierCheckTime = EditorApplication.timeSinceStartup - 10;
            return base.CreateInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_typeProperty, new GUIContent("Play Type", "Decide how this audio player chooses it's container to use"));
            EditorGUILayout.PropertyField(_playModeProperty, new GUIContent("Play Mode", "Decide if this player should start playing automatically"));
            EditorGUILayout.Slider(_volumeProperty, 0f, 1f, new GUIContent("Volume", "The current volume multiplier"));

            if (targets.Length == 1)
            {
                EditorGUILayout.PropertyField(_customAudioSourceProperty,
                    new GUIContent("Custom Audio Source", "The audio source that will be playing the container"));
            }

            var typeEnumIndex = _typeProperty.enumValueIndex;
            if (typeEnumIndex == 0) // Container
            {
                _containersIdsProperty.ClearArray();
                EditorGUILayout.PropertyField(_containersProperty, new GUIContent("Containers", "The Audio Clip Containers this player will use"));
            }
            else if (typeEnumIndex == 1 && targets.Length == 1) // ContainerWithID
            {
                _containersProperty.ClearArray();

                EditorGUILayout.PropertyField(_containersIdsProperty, new GUIContent("Container IDs", "The array of Unique ids of audio clip container"));

                if (!CheckIdentifiers((AudioClipPlayer)target))
                {
                    EditorGUILayout.HelpBox("Invalid Identifier", MessageType.Warning);
                }
            }
            else if (typeEnumIndex == 2) // RandomByTag
            {
                _containersIdsProperty.ClearArray();
                _containersProperty.ClearArray();
                var tags = AudioSystemEditorUtils.GetAllTagNames(_settings);

                var tagID = _tagIDProperty.uintValue;
                var selectedTag = EditorGUILayout.Popup(new GUIContent("Tag", "The tag of the container"), (int)tagID, tags);
                _tagIDProperty.uintValue = (uint)selectedTag;
            }

            if (targets.Length == 1)
            {
                EditorGUILayout.PropertyField(_effectsProperty,
                new GUIContent("Effects", "Add any amount of audio clip effect you wish to apply when this player starts playing"));
            }

            //Events
            if (targets.Length == 1)
            {
                _eventsFoldout = EditorGUILayout.Foldout(_eventsFoldout, new GUIContent("Events"));

                if (_eventsFoldout)
                {
                    EditorGUILayout.PropertyField(_onPlayStartedProperty,
                    new GUIContent("On Play Started", "Called when the player starts or resumes playing a clip"));
                    EditorGUILayout.PropertyField(_onPausedProperty,
                        new GUIContent("On Paused", "Called when the player pauses playing it's clips"));
                    EditorGUILayout.PropertyField(_onStoppedProperty,
                        new GUIContent("On Stopped", "Called when player is being forced to play it's clips"));
                    EditorGUILayout.PropertyField(_onPlayFinishedProperty,
                        new GUIContent("On Play Finished", "Called when player finishes playing all of it's clips without any intervention"));
                }
            }

            EditorGUILayout.PropertyField(_syncAudioSourceTransformProperty,
                new GUIContent("Sync Audio Source Transform", "Decide whether the audio source that will be used to play this clip should be in the same position with this player"));

            EditorGUILayout.PropertyField(_allowSimultaneousPlayProperty, 
                new GUIContent("Allow Simultaneous Play", "Whether the player should play multiple clips simultaneously or play them on after another"));
            if (_allowSimultaneousPlayProperty.boolValue)
            {
                EditorGUILayout.PropertyField(_limitMaxAudioSourceProperty,
                    new GUIContent("limit Max Audio Sources", "Enable if the player should not play infinite amout of sources at the same time"));

                if (_limitMaxAudioSourceProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(_maxAudioSourceProperty,
                        new GUIContent("Max Audio Source", "The maximum amount of audio sources the player can play at the same time"));
                }

                EditorGUILayout.PropertyField(_chooseClipsRespectivelyProperty,
                    new GUIContent("Choose Clips Respectively", "Decide whether the clips from collection should be choosen randomly or respectively"));

                if (_chooseClipsRespectivelyProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(_clearRespectiveDataWhenStoppedProperty,
                        new GUIContent("Clear Respective Data When Stopped", "Should player keep it's respective containers data after its stopped"));
                }
            }

            EditorGUILayout.PropertyField(_destroyPlayerWhenStoppedProperty,
                new GUIContent("Destroy Player When Stopped", "Decide if the player gameobject should be destroyed when it's clip is stopped playing"));

            EditorGUILayout.Space(10);

            if (targets.Length == 1)
            {
                var player = (AudioClipPlayer)target;
                _debugFoldout = EditorGUILayout.Foldout(_debugFoldout, new GUIContent("Debug"));

                if (_debugFoldout)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(_stateProperty, new GUIContent("Current State"));
                    if (_allowSimultaneousPlayProperty.boolValue && _chooseClipsRespectivelyProperty.boolValue && EditorApplication.isPlaying)
                    {
                        EditorGUILayout.PropertyField(_respectiveContainersProperty, new GUIContent("Respective Containers"));
                    }
                    GUI.enabled = true;
                }

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
                if (!AudioContainerLibraryEditorUtils.LibraryContainsIdentifier(id, _library))
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