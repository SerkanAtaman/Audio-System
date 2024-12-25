using Newtonsoft.Json.Linq;
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

        public static float RefreshVolume(this AudioClipContainer container, AudioSystemSettings settings = null)
        {
            if (settings == null) settings = AudioSystemManager.Instance.Settings;
            var category = settings.GetCategoryByID(container.CategoryID);
            var volume = container.CurrentVolume;
            if (category != null)
            {
                volume *= category.Value.Volume;
            }

            volume = Mathf.Clamp(volume, 0f, container.MaxVolume);
            volume *= settings.MasterVolumeMultiplier;
            container.SetPlaybackVolume(volume);
            return volume;
        }

        public static float RefreshPitch(this AudioClipContainer container)
        {
            var alives = AudioSystemManager.Instance.GetAliveDatas(container);

            foreach (var alive in alives)
            {
                alive.Source.pitch = container.Pitch;
            }

            return container.Pitch;
        }

        public static AliveAudioData GetLatestAliveAudioData(this AudioClipContainer container)
        {
            var alives = AudioSystemManager.Instance.GetAliveDatas(container);
            if (alives.Count == 0) return null;

            return alives[^1];
        }

        public static AudioClipContainer GetContainerByID(uint containerID)
        {
            return AudioSystemManager.Instance.Library.GetContainerFromID(containerID);
        }

        public static AliveAudioData Play(this AudioClipContainer container)
        {
            return AudioSystemManager.Instance.Play(container);
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

        public static void SetPlaybackVolume(this AudioClipContainer container, float value)
        {
            value = Mathf.Clamp01(value);
            var alives = AudioSystemManager.Instance.GetAliveDatas(container);

            foreach (var alive in alives)
            {
                alive.Source.volume = value;
            }
        }
    }
}