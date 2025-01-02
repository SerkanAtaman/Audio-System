using System;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AudioSystemSettings : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)]
        private float _masterVolumeMultiplier = 1f;

        public AudioCategory[] Categories;
        public string[] Tags;

        public int AudioSourcePoolStartSize = 20;

        public Action<AudioSystemSettings> OnUpdated;

        public float MasterVolumeMultiplier
        {
            get
            {
                return _masterVolumeMultiplier;
            }
            set
            {
                _masterVolumeMultiplier = Mathf.Clamp01(value);
                OnUpdated?.Invoke(this);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying) OnUpdated?.Invoke(this);
        }

        public void SetCategoryVolume(string categoryName, float volume)
        {
            if (Categories == null) return;

            for (int i = 0; i < Categories.Length; i++)
            {
                if (string.Equals(Categories[i].Name, categoryName))
                {
                    Categories[i].Volume = Mathf.Clamp01(volume);
                    OnUpdated?.Invoke(this);
                    break;
                }
            }
        }

        public void SetCategoryVolume(uint categoryID, float volume)
        {
            if (Categories == null) return;

            for (int i = 0; i < Categories.Length; i++)
            {
                if (Categories[i].ID == categoryID)
                {
                    Categories[i].Volume = Mathf.Clamp01(volume);
                    OnUpdated?.Invoke(this);
                    break;
                }
            }
        }

        public AudioCategory? GetCategoryByName(string name)
        {
            foreach (var category in Categories)
            {
                if (string.Equals(category.Name, name))
                {
                    return category;
                }
            }

            return null;
        }

        public AudioCategory? GetCategoryID(int id)
        {
            foreach (var category in Categories)
            {
                if (category.ID == id)
                {
                    return category;
                }
            }

            return null;
        }
    }
}