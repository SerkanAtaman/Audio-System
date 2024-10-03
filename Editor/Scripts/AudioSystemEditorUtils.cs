using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    public static class AudioSystemEditorUtils
    {
        [MenuItem("Assets/Create/AudioContainer", true)]
        public static bool CanCreateAudioContainer()
        {
            if (Selection.count == 0) return false;

            foreach (var obj in Selection.objects)
            {
                if (obj == null) return false;
                if (obj.GetType() != typeof(AudioClip)) return false;
            }

            return true;
        }

        [MenuItem("Assets/Create/AudioContainer", priority = 2)]
        public static void CreateAudioContainer()
        {
            if (Selection.count == 0) return;

            List<Object> newContainers = new List<Object>();

            foreach (var obj in Selection.objects)
            {
                if (obj == null) continue;
                newContainers.Add(CreateAudioContainer((AudioClip)obj));
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            Selection.objects = newContainers.ToArray();
        }

        public static AudioClipContainer CreateAudioContainer(AudioClip clip)
        {
            var path = AssetDatabase.GetAssetPath(clip);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            fileName = "audioContainer_" + fileName + ".asset";
            var containerPath = Path.Combine(directory, fileName);
            var container = ScriptableObject.CreateInstance<AudioClipContainer>();
            container.ReceiveEditorData(clip);
            AssetDatabase.CreateAsset(container, containerPath);
            return container;
        }

        public static AudioSystemSettings GetSettings(bool createIfNotExist = true)
        {
            var path = "Assets/Resources/Serojob-AudioSystem/AudioSystemSettings.asset";
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var asset = AssetDatabase.LoadAssetAtPath<AudioSystemSettings>(path);

            if (asset == null)
            {
                if (!createIfNotExist) return null;

                return CreateSettingsAsset();
            }

            return asset;
        }

        public static AudioSystemSettings CreateSettingsAsset()
        {
            var path = "Assets/Resources/Serojob-AudioSystem/AudioSystemSettings.asset";
            var directory = Path.GetDirectoryName(path);

            if (File.Exists(path))
            {
                return AssetDatabase.LoadAssetAtPath<AudioSystemSettings>(path);
            }

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var asset = ScriptableObject.CreateInstance<AudioSystemSettings>();
            asset.Categories = new string[0];

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<AudioSystemSettings>(path);
        }

        public static string[] GetAllCategoryNames()
        {
            var settings = GetSettings();
            string[] result = new string[settings.Categories.Length + 1];

            for (int i = 0; i < settings.Categories.Length; i++)
            {
                result[i + 1] = settings.Categories[i];
            }

            result[0] = "None";

            return result;
        }

        public static int GetCategoryNameIndex(string[] categories, string name)
        {
            int result = 0;

            for (int i = 0; i < categories.Length; i++)
            {
                if (string.Equals(categories[i], name))
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public static void AddCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                Debug.LogError("Cannot add empty category");
                return;
            }

            var settings = GetSettings();
            foreach (var category in settings.Categories)
            {
                if(string.Equals(category, categoryName))
                {
                    Debug.LogError("Given Category: " + categoryName + " is already exist in Audio System");
                    return;
                }
            }

            settings.Categories = ExpandCategories(settings, categoryName);
        }

        private static string[] ExpandCategories(AudioSystemSettings settings, string category)
        {
            string[] result = new string[settings.Categories.Length + 1];
            for (int i = 0; i < settings.Categories.Length; i++)
            {
                result[i] = settings.Categories[i];
            }

            result[^1] = category;
            return result;
        }
    }
}