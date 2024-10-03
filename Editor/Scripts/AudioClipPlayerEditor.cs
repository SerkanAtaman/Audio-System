using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioClipPlayer))]
    public class AudioClipPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AudioClipPlayer player = (AudioClipPlayer)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Container"), new GUIContent("Container", "The Audio Clip Container this player will use"));
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
    }
}