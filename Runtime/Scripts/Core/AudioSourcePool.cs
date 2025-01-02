using System.Collections.Generic;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AudioSourcePool
    {
        private readonly Transform _poolHolder;

        private readonly int _poolStartSize;

        private readonly List<AudioSource> _pool;

        private int _poolSize;

        public AudioSourcePool(Transform poolHolder, int startSize)
        {
            _poolHolder = poolHolder;
            _poolStartSize = startSize;
            _poolSize = 0;
            _pool = new();

            FillRemainingItems();
        }

        private void FillRemainingItems()
        {
            if (_poolSize >= _poolStartSize) return;

            int step = _poolStartSize - _poolSize;
            for (int i = 0; i < step; i++)
            {
                SpawnNewItem();
            }
        }

        private void SpawnNewItem()
        {
            var pref = new GameObject("audioSourcePref");
            var comp = pref.AddComponent<AudioSource>();
            comp.playOnAwake = false;
            comp.transform.SetParent(_poolHolder);
            PushItem(comp);
        }

        public void PushItem(AudioSource source)
        {
            _poolSize++;
            source.gameObject.SetActive(false);
            source.transform.SetParent(_poolHolder);
            _pool.Add(source);
        }

        public AudioSource Pull()
        {
            if (_poolSize <= 0) SpawnNewItem();

            var item = _pool[0];
            _poolSize--;
            _pool.RemoveAt(0);
            return item;
        }
    }
}
