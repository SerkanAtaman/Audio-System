using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;

namespace SeroJob.AudioSystem.Editor
{
    public static class AudioContainerLibraryEditorUtils
    {
        public static AudioContainerLibrary GetLibrary(bool createIfNotExist = true)
        {
            AudioContainerLibrary result = null;
            var libraryGuids = AssetDatabase.FindAssets("t:AudioContainerLibrary");

            if (libraryGuids == null || libraryGuids.Length < 1)
            {
                if (createIfNotExist) result = CreateLibraryAsset();
            }
            else
            {
                for (int i = 1; i < libraryGuids.Length; i++)
                {
                    Debug.LogWarning("Pleaes delete the extra Audio Container Library asset at: " + AssetDatabase.GUIDToAssetPath(libraryGuids[i]));
                }

                result = AssetDatabase.LoadAssetAtPath<AudioContainerLibrary>(AssetDatabase.GUIDToAssetPath(libraryGuids[0]));
            }

            if (result != null) AssignLibraryAssetToAddressables(result);

            return result;
        }

        public static AudioContainerLibrary CreateLibraryAsset()
        {
            var path = "Assets/Serojob-AudioSystem/AudioContainerLibrary.asset";
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

        public static void AssignLibraryAssetToAddressables(AudioContainerLibrary library)
        {
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (addressableSettings == null)
            {
                Debug.LogError("addressable settings is nul");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(library);
            var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var currentEntry = addressableSettings.FindAssetEntry(assetGuid);

            if (currentEntry != null)
            {
                AssignLibraryAddressableLabel(currentEntry);
                return;
            }

            var targetGroupName = "Serojob-Audio-System";
            var group = addressableSettings.FindGroup(targetGroupName);

            if (group == null)
            {
                group = addressableSettings.CreateGroup(targetGroupName,
                    false, true, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            currentEntry = addressableSettings.CreateOrMoveEntry(assetGuid, group, true);

            AssignLibraryAddressableLabel(currentEntry);

            AssetDatabase.SaveAssetIfDirty(addressableSettings);
        }

        public static void AssignLibraryAddressableLabel(AddressableAssetEntry addressableEntry)
        {
            if (addressableEntry == null || addressableEntry.labels.Contains("SerojobAudioSystemLibrary")) return;

            addressableEntry.SetLabel("SerojobAudioSystemLibrary", true, true);

            EditorUtility.SetDirty(addressableEntry.parentGroup);
            EditorUtility.SetDirty(addressableEntry.parentGroup.Settings);
            AssetDatabase.SaveAssetIfDirty(addressableEntry.parentGroup);
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

        public static void Update(AudioClipContainer clipContainer, AudioContainerLibrary library = null, bool saveAfter = true)
        {
            if (library == null) library = GetLibrary();

            if (!library.Containers.Contains(clipContainer))
            {
                library.Containers.Add(clipContainer);
                EditorUtility.SetDirty(library);
                if (saveAfter) AssetDatabase.SaveAssetIfDirty(library);
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
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssetIfDirty(library);
        }

        public static bool IsIdentifierValid(uint identifier, AudioContainerLibrary library = null)
        {
            if (library == null) library = GetLibrary();
            return library.SameIdentifierCount(identifier) <= 1;
        }

        public static bool LibraryContainsIdentifier(uint identifier, AudioContainerLibrary library = null)
        {
            if (library == null) library = GetLibrary();
            return library.ContainsIdentifier(identifier);
        }

        public static bool ContainsIdentifier(this AudioContainerLibrary library, uint identifier)
        {
            foreach (var item in library.Containers)
            {
                if (item == null) continue;
                if (item.ID == identifier) return true;
            }

            return false;
        }

        public static int SameIdentifierCount(this AudioContainerLibrary library, uint identifier)
        {
            int result = 0;
            foreach (var item in library.Containers)
            {
                if (item == null) continue;
                if (item.ID == identifier) result++;
            }

            return result;
        }

        public static uint GenerateUniqueIdentifier(AudioContainerLibrary library = null)
        {
            if (library == null) library = GetLibrary();

            var randomBytes = new byte[32];
            bool idGenerated = false;
            uint result = 0;

            using (var rng = RandomNumberGenerator.Create())
            {
                while (!idGenerated)
                {
                    rng.GetBytes(randomBytes);
                    result = BitConverter.ToUInt32(randomBytes, 0);
                    idGenerated = !LibraryContainsIdentifier(result, library);
                }
            }

            return result;
        }
    }
}