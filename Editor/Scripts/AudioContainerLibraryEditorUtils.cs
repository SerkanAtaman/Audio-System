using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SeroJob.AudioSystem.Editor
{
    public static class AudioContainerLibraryEditorUtils
    {
        public static AudioContainerLibrary GetLibrary(bool createIfNotExist = true)
        {
            var path = "Assets/Resources/Serojob-AudioSystem/AudioContainerLibrary.asset";
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var asset = AssetDatabase.LoadAssetAtPath<AudioContainerLibrary>(path);

            if (asset == null)
            {
                if (!createIfNotExist) return null;

                return CreateLibraryAsset();
            }

            return asset;
        }

        public static AudioContainerLibrary CreateLibraryAsset()
        {
            var path = "Assets/Resources/Serojob-AudioSystem/AudioContainerLibrary.asset";
            var directory = Path.GetDirectoryName(path);

            if (File.Exists(path))
            {
                return AssetDatabase.LoadAssetAtPath<AudioContainerLibrary>(path);
            }

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var asset = ScriptableObject.CreateInstance<AudioContainerLibrary>();
            asset.Containers = new();

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<AudioContainerLibrary>(path);
        }

        [MenuItem("SeroJob/AudioSystem/FindAllContainers")]
        public static void FindAllContainers()
        {
            EditorUtility.DisplayProgressBar("Audio System", "Searching project for every single audio container", 0.1f);

            var library = GetLibrary();
            library.Containers.Clear();

            var assets = AssetDatabase.FindAssets("t:AudioClipContainer");

            for (int i = 0; i < assets.Length; i++)
            {
                library.Containers.Add(AssetDatabase.LoadAssetAtPath<AudioClipContainer>(AssetDatabase.GUIDToAssetPath(assets[i])));
            }

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssetIfDirty(library);

            EditorUtility.ClearProgressBar();

            Debug.Log($"{assets.Length} amount of AudioClipContainers have been found and added to Audio System Library");
        }

        public static void Update(AudioClipContainer clipContainer)
        {
            var library = GetLibrary();
            if (!library.Containers.Contains(clipContainer))
            {
                library.Containers.Add(clipContainer);
                EditorUtility.SetDirty(library);
                AssetDatabase.SaveAssetIfDirty(library);
            }
        }

        public static void RemoveDeletedContainers(int deletedCount)
        {
            if (deletedCount < 1) return;
            var library = GetLibrary();
            List<AudioClipContainer> result = new();
            foreach (var container in library.Containers)
            {
                if (container == null) continue;
                result.Add(container);
            }
            library.Containers = result;
        }

        public static bool LibraryContainsIdentifier(string identifier)
        {
            var library = GetLibrary();
            return library.ContainsIdentifier(identifier);
        }

        public static bool ContainsIdentifier(this AudioContainerLibrary library, string identifier)
        {
            foreach (var item in library.Containers)
            {
                if (string.Equals(identifier, item.Identifier)) return true;
            }

            return false;
        }
    }
}