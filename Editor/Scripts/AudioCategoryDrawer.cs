using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    [CustomPropertyDrawer(typeof(AudioCategory))]
    public class AudioCategoryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                var nameRect = foldoutRect;
                nameRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("Name"), new GUIContent("Name", "The name of the category"));

                var idRect = nameRect;
                idRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var idProperty = property.FindPropertyRelative("ID");
                EditorGUI.PropertyField(idRect, idProperty, new GUIContent("ID", "The unique id of the category"));
                if (idProperty.intValue < 1) idProperty.uintValue = 999;

                var volumeRect = idRect;
                volumeRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var volumeProperty = property.FindPropertyRelative("Volume");
                var volumeLabel = new GUIContent("Volume", "The volume multiplier of the category");
                EditorGUI.Slider(volumeRect, volumeProperty, 0f, 1f, volumeLabel);

                var muteRect = volumeRect;
                muteRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(muteRect, property.FindPropertyRelative("Muted"), new GUIContent("Muted", "Is the category muted"));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing * 4;
        }
    }
}