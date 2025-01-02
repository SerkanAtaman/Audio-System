using System.Collections.Generic;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public static class AudioSystemUtils
    {
        public static T GetRandomElement<T>(this IList<T> list)
        {
            if (list == null) return default;
            if (list.Count == 0) return default;
            if (list.Count == 1) return list[0];

            var randIndex = UnityEngine.Random.Range(0, list.Count);

            return list[randIndex];
        }

        public static AudioCategory? GetCategoryByID(this AudioSystemSettings settings, uint id)
        {
            if (id < 1) return null;

            foreach (var category in settings.Categories)
            {
                if (category.ID == id) return category;
            }

            return null;
        }

        public static AudioCategory? GetCategoryByName(this AudioSystemSettings settings, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            foreach (var category in settings.Categories)
            {
                if (string.Equals(category.Name, name)) return category;
            }

            return null;
        }

        public static void SetMuteState(this AudioCategory category, bool isMuted, AudioSystemSettings settings = null)
        {
            if (AudioSystemManager.Instance == null) return;

            if (settings == null) settings = AudioSystemManager.Instance.Settings;

            for (int i = 0; i < settings.Categories.Length; i++)
            {
                var cat = settings.Categories[i];
                if (cat.ID != category.ID) continue;
                cat.Muted = isMuted;
                settings.Categories[i] = cat;
            }

            settings.OnUpdated?.Invoke(settings);
        }

        public static void RefreshAliveDatas(this AudioClipContainer container, AudioSystemSettings settings = null)
        {
            var volume = GetTargetVolume(container);
            var alives = container.GetAllAliveAudioData();
            if (alives == null) return;
            if (settings == null) settings = AudioSystemManager.Instance.Settings;

            var category = settings.GetCategoryByID(container.CategoryID);

            foreach (var alive in alives)
            {
                alive.Source.volume = volume;
                alive.Source.mute = category != null && category.Value.Muted;
                alive.Source.loop = container.Loop;
                alive.Source.playOnAwake = false;
                alive.Source.pitch = container.Pitch;
                alive.Source.spatialBlend = container.SpatialBlend;
            }
        }

        public static void Refresh(this AliveAudioData aliveAudioData, AudioSystemSettings settings = null)
        {
            if (settings == null) settings = AudioSystemManager.Instance.Settings;

            var category = settings.GetCategoryByID(aliveAudioData.Container.CategoryID);
            var volume = GetTargetVolume(aliveAudioData.Container);

            aliveAudioData.Source.volume = volume;
            aliveAudioData.Source.mute = category != null && category.Value.Muted;
            aliveAudioData.Source.loop = aliveAudioData.Container.Loop;
            aliveAudioData.Source.playOnAwake = false;
            aliveAudioData.Source.pitch = aliveAudioData.Container.Pitch;
            aliveAudioData.Source.spatialBlend = aliveAudioData.Container.SpatialBlend;
        }

        public static float GetTargetVolume(this AudioClipContainer container, AudioSystemSettings settings = null)
        {
            if (settings == null) settings = AudioSystemManager.Instance.Settings;
            var category = settings.GetCategoryByID(container.CategoryID);
            var volume = container.BaseVolume;
            if (category != null)
            {
                volume *= category.Value.Volume;
            }

            volume *= settings.MasterVolumeMultiplier;
            return volume;
        }

        public static AliveAudioData GetLatestAliveAudioData(this AudioClipContainer container)
        {
            var alives = AudioSystemManager.Instance.GetAliveDatas(container);
            if (alives.Count == 0) return null;

            return alives[^1];
        }

        public static List<AliveAudioData> GetAllAliveAudioData(this AudioClipContainer container)
        {
            return AudioSystemManager.Instance.GetAliveDatas(container);
        }

        public static AudioClipContainer GetContainerByID(uint containerID)
        {
            return AudioSystemManager.Instance.Library.GetContainerFromID(containerID);
        }

        public static AliveAudioData Play(this AudioClipContainer container, AudioSource customSource = null)
        {
            if (customSource == null) return AudioSystemManager.Instance.Play(container);

            return AudioSystemManager.Instance.Play(container, customSource);
        }

        public static AliveAudioData Play(this AudioClipContainer container, IEnumerable<AudioClipEffect> effects)
        {
            var data = AudioSystemManager.Instance.Play(container);

            if (effects == null) return data;

            foreach (var effect in effects)
            {
                effect.Apply(container);
            }

            return data;
        }

        public static void Pause(this AliveAudioData aliveAudioData)
        {
            AudioSystemManager.Instance.Pause(aliveAudioData);
        }

        public static void Resume(this AliveAudioData aliveAudioData)
        {
            AudioSystemManager.Instance.Resume(aliveAudioData);
        }

        public static void Stop(this AliveAudioData aliveAudioData)
        {
            AudioSystemManager.Instance.Stop(aliveAudioData);
        }
    }
}