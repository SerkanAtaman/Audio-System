using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AudioSystemLibraryCollection
    {
#if UNITY_EDITOR
        private AudioContainerLibrary _library;

        public AudioSystemLibraryCollection(AudioContainerLibrary library)
        {
            _library = library;
        }

        public AudioClipContainer GetContainerFromID(string id, AudioSystemSettings settings)
        {
            foreach (var item in _library.Containers)
            {
                if(string.Equals(item.Identifier, id)) return item;
            }

            return null;
        }

        public List<AudioClipContainer> GetAllContainersInCategory(string category)
        {
            List<AudioClipContainer> result = new();

            foreach (var item in _library.Containers)
            {
                if (string.Equals(item.Category, category)) result.Add(item);
            }

            return result;
        }
#endif

#if !UNITY_EDITOR
        // category - identifier - container
        private Dictionary<string, Dictionary<string, AudioClipContainer>> _containers;

        public AudioSystemLibraryCollection(AudioContainerLibrary library)
        {
            _containers = new();
            foreach (var container in library.Containers)
            {
                if (!_containers.ContainsKey(container.Category))
                {
                    _containers.Add(container.Category, new Dictionary<string, AudioClipContainer>());
                    _containers[container.Category].Add(container.Identifier, container);
                }
                else
                {
                    _containers[container.Category].Add(container.Identifier, container);
                }
            }
        }

        public AudioClipContainer GetContainerFromID(string id, AudioSystemSettings settings)
        {
            AudioClipContainer result = null;
            int count = settings.Categories.Length;
            int index = 0;

            while (index < count && result == null)
            {
                try
                {
                    result = _containers[settings.Categories[index]][id];
                    index++;
                }
                catch
                {
                    result = null;
                    index++;
                }
            }

            return result;
        }

        public List<AudioClipContainer> GetAllContainersInCategory(string category)
        {
            List<AudioClipContainer> result;

            try
            {
                result = _containers[category].Values.ToList();
            }
            catch
            {
                result = new();
            }

            return result;
        }
#endif
    }
}