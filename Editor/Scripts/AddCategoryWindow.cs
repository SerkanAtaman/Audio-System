using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    public class AddCategoryWindow : EditorWindow
    {
        private string _categoryName;

        private void CreateGUI()
        {
            _categoryName = "";
        }

        private void OnGUI()
        {
            _categoryName = EditorGUILayout.TextField("Category Name", _categoryName);

            EditorGUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Add", "")))
            {
                AudioSystemEditorUtils.AddCategory(_categoryName);
                Close();
            }
        }
    }
}