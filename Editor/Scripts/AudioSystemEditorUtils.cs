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
        public static async void CreateAudioContainer()
        {
            if (Selection.count == 0) return;

            var selectionCount = Selection.count;

            EditorUtility.DisplayProgressBar("Audio System", "Creating audio clip containers", 0.05f);

            List<Object> newContainers = new List<Object>();

            for (int i = 0; i < selectionCount; i++)
            {                
                var obj = Selection.objects[i];

                if (obj != null)
                {
                    newContainers.Add(CreateAudioContainer((AudioClip)obj));
                }

                EditorUtility.DisplayProgressBar("Audio System", "Creating audio clip containers", (i + 1) / (float)Selection.count);
                await System.Threading.Tasks.Task.Delay(16);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            if (newContainers.Count < 10)
                Selection.objects = newContainers.ToArray();
            else if (newContainers.Count > 0)
                Selection.objects = new Object[] { newContainers[0] };

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
            AudioContainerLibraryEditorUtils.Update(container, null, false);
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
            if (addressableEntry == null || addressableEntry.labels.Contains("SerojobAudioSystemSettings")) return;

            addressableEntry.SetLabel("SerojobAudioSystemSettings", true, true);

            EditorUtility.SetDirty(addressableEntry.parentGroup);
            EditorUtility.SetDirty(addressableEntry.parentGroup.Settings);
            AssetDatabase.SaveAssetIfDirty(addressableEntry.parentGroup);
        }

        public static string[] GetAllCategoryNames(AudioSystemSettings settings = null)
        {
            if (settings == null) settings = GetSettings();

            settings.Categories ??= new AudioCategory[0];
            string[] result = new string[settings.Categories.Length + 1];

            for (int i = 0; i < settings.Categories.Length; i++)
            {
                result[i + 1] = settings.Categories[i].Name;
            }

            result[0] = "None";

            return result;
        }

        public static AudioCategory? GetCategoryByName(string name, AudioSystemSettings settings = null)
        {
            if (settings == null) settings = GetSettings();

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

        public static string[] GetAllTagNames(AudioSystemSettings settings = null)
        {
            if (settings == null) settings = GetSettings();
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