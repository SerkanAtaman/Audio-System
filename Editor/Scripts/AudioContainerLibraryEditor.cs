using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(AudioContainerLibrary))]
    public class AudioContainerLibraryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;

            base.DrawDefaultInspector();

            GUI.enabled = true;
        }
    }
}