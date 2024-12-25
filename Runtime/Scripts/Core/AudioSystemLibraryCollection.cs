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

        public AudioClipContainer GetContainerFromID(uint id)
        {
            AudioClipContainer result = null;

            foreach (var container in _library.Containers)
            {
                if (container.ID == id)
                {
                    result = container;
                    break;
                }
            }

            return result;
        }

        public List<AudioClipContainer> GetContainersByTag(uint tagID)
        {
            List<AudioClipContainer> result = new();

            foreach (var container in _library.Containers)
            {
                if (container.TagID == tagID) result.Add(container);
            }

            return result;
        }

        public List<AudioClipContainer> GetContainersByCategory(uint categoryID)
        {
            List<AudioClipContainer> result = new();

            foreach (var container in _library.Containers)
            {
                if (container.CategoryID == categoryID) result.Add(container);
            }

            return result;
        }
#endif

#if !UNITY_EDITOR
        private AudioClipContainer[] _allContainers;

        public AudioSystemLibraryCollection(AudioContainerLibrary library)
        {
            _allContainers = library.Containers.ToArray();
            library.Containers.Clear();
            library.Containers = null;
        }

        public AudioClipContainer GetContainerFromID(uint id)
        {
            AudioClipContainer result = null;

            foreach (var container in _allContainers)
            {
                if (container.ID == id)
                {
                    result = container;
                    break;
                }
            }

            return result;
        }

        public List<AudioClipContainer> GetContainersByTag(uint tagID)
        {
            List<AudioClipContainer> result = new();

            foreach (var container in _allContainers)
            {
                if (container.TagID == tagID) result.Add(container);
            }

            return result;
        }

        public List<AudioClipContainer> GetContainersByCategory(uint categoryID)
        {
            List<AudioClipContainer> result = new();

            foreach (var container in _allContainers)
            {
                if (container.CategoryID == categoryID) result.Add(container);
            }

            return result;
        }
#endif
    }
}