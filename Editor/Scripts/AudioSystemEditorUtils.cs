using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
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

            EditorUtility.DisplayProgressBar("Audio System", "Creating audio clip containers", 0.0f);

            List<Object> newContainers = new List<Object>();

            for (int i = 0; i < Selection.count; i++)
            {
                var obj = Selection.objects[i];
                if (obj == null) continue;
                newContainers.Add(CreateAudioContainer((AudioClip)obj));
                EditorUtility.DisplayProgressBar("Audio System", "Creating audio clip containers", i / Selection.count);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            Selection.objects = newContainers.ToArray();

            EditorUtility.ClearProgressBar();
        }

        public static AudioClipContainer CreateAudioContainer(AudioClip clip)
        {
            var path = AssetDatabase.GetAssetPath(clip);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            fileName = "audioContainer_" + fileName + ".asset";
            var containerPath = Path.Combine(directory, fileName);
            var container = ScriptableObject.CreateInstance<AudioClipContainer>();
            container.ReceiveEditorData(clip, AudioContainerLibraryEditorUtils.GenerateUniqueIdentifier());
            AssetDatabase.CreateAsset(container, containerPath);
            AudioContainerLibraryEditorUtils.Update(container);
            return container;
        }

        public static AudioSystemSettings GetSettings(bool createIfNotExist = true)
        {
            AudioSystemSettings result = null;
            var settingsGuids = AssetDatabase.FindAssets("t:AudioSystemSettings");

            if (settingsGuids == null || settingsGuids.Length < 1)
            {
                if (createIfNotExist) result = CreateSettingsAsset(); 
            }
            else
            {
                for (int i = 1; i < settingsGuids.Length; i++)
                {
                    Debug.LogWarning("Pleaes delete the extra Audio System Settings asset at: " + AssetDatabase.GUIDToAssetPath(settingsGuids[i]));
                }

                result = AssetDatabase.LoadAssetAtPath<AudioSystemSettings>(AssetDatabase.GUIDToAssetPath(settingsGuids[0]));
            }

            if (result != null) AssignSettingsAssetToAddressables(result);

            return result;
        }

        public static AudioSystemSettings CreateSettingsAsset()
        {
            var path = "Assets/Serojob-AudioSystem/AudioSystemSettings.asset";
            var directory = Path.GetDirectoryName(path);

            if (File.Exists(path))
            {
                return AssetDatabase.LoadAssetAtPath<AudioSystemSettings>(path);
            }

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var asset = ScriptableObject.CreateInstance<AudioSystemSettings>();
            asset.Categories = new AudioCategory[0];
            asset.Tags = new string[0];

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<AudioSystemSettings>(path);
        }

        public static void AssignSettingsAssetToAddressables(AudioSystemSettings settings)
        {
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (addressableSettings == null)
            {
                Debug.LogError("addressable settings is nul");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(settings);
            var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var currentEntry = addressableSettings.FindAssetEntry(assetGuid);

            if (currentEntry != null)
            {
                AssignSettingsAddressableLabel(currentEntry);
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

            AssignSettingsAddressableLabel(currentEntry);

            EditorUtility.SetDirty(addressableSettings);
            AssetDatabase.SaveAssetIfDirty(addressableSettings);
        }

        public static void AssignSettingsAddressableLabel(AddressableAssetEntry addressableEntry)
        {
            if (addressableEntry == null) return;

            if (addressableEntry.labels == null || addressableEntry.labels.Count < 1)
            {
                addressableEntry.SetLabel("SerojobAudioSystemSettings", true);
            }
            else if (addressableEntry.labels.Count > 1)
            {
                addressableEntry.labels.Clear();
                addressableEntry.SetLabel("SerojobAudioSystemSettings", true);
            }
            else if (!addressableEntry.labels.Contains("SerojobAudioSystemSettings"))
            {
                addressableEntry.labels.Clear();
                addressableEntry.SetLabel("SerojobAudioSystemSettings", true);
            }
            else
            {
                return;
            }

            EditorUtility.SetDirty(addressableEntry.parentGroup);
            EditorUtility.SetDirty(addressableEntry.parentGroup.Settings);
            AssetDatabase.SaveAssetIfDirty(addressableEntry.parentGroup);
        }

        public static string[] GetAllCategoryNames()
        {
            var settings = GetSettings();
            settings.Categories ??= new AudioCategory[0];
            string[] result = new string[settings.Categories.Length + 1];

            for (int i = 0; i < settings.Categories.Length; i++)
            {
                result[i + 1] = settings.Categories[i].Name;
            }

            result[0] = "None";

            return result;
        }

        public static AudioCategory? GetCategoryByName(string name)
        {
            var settings = GetSettings();
            foreach (var category in settings.Categories)
            {
                if (string.Equals(category.Name, name)) return category;
            }

            return null;
        }

        public static uint GetCategoryNameIndex(string[] categories, string name)
        {
            uint result = 0;

            for (int i = 0; i < categories.Length; i++)
            {
                if (string.Equals(categories[i], name))
                {
                    result = (uint)i;
                    break;
                }
            }

            return result;
        }

        public static string[] GetAllTagNames()
        {
            var settings = GetSettings();
            settings.Tags ??= new string[0];
            string[] result = new string[settings.Tags.Length + 1];

            for (int i = 0; i < settings.Tags.Length; i++)
            {
                result[i + 1] = settings.Tags[i];
            }

            result[0] = "None";

            return result;
        }

        public static uint GetTagNameIndex(string[] tags, string tag)
        {
            uint result = 0;

            for (int i = 0; i < tags.Length; i++)
            {
                if (string.Equals(tags[i], tag))
                {
                    result = (uint)i;
                    break;
                }
            }

            return result;
        }
    }
}