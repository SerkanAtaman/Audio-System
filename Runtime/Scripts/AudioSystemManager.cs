using UnityEngine;
using SeroJob.ObjectPooling;
using System.Collections.Generic;
using System;

namespace SeroJob.AudioSystem
{
    [DefaultExecutionOrder(-1)]
    public class AudioSystemManager : MonoBehaviour
    {
        public static AudioSystemManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var manager = new GameObject("AudioSystemManager");
                    var comp = manager.AddComponent<AudioSystemManager>();
                    comp.Init();
                    _instance = comp;
                }

                return _instance;
            }
        }
        private static AudioSystemManager _instance;

        public bool IsInitialized { get; private set; } = false;

        public Action<AliveAudioData> OnAudioDied;

        private ObjectPool<AudioSource> _audioSourcePool;
        private List<AliveAudioData> _aliveAudioData;
        private List<AliveAudioData> _deadAudioData;

        private float _lastUpdateTime;

        private void Awake()
        {
            Init();
        }

        private void LateUpdate()
        {
            if (Time.time - _lastUpdateTime < 1f) return;

            _lastUpdateTime = Time.time;

            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData.HasDied)
                {
                    Stop(aliveData);
                    _deadAudioData.Add(aliveData);
                }
            }

            foreach (var deadData in _deadAudioData)
            {
                _aliveAudioData.Remove(deadData);
            }

            _deadAudioData.Clear();
        }

        private void OnDisable()
        {
            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData == null) continue;
                Stop(aliveData);
            }
            _aliveAudioData.Clear();
        }

        private void Init()
        {
            if (IsInitialized) return;
            var pref = new GameObject("audioSourcePref");
            var comp = pref.AddComponent<AudioSource>();
            comp.playOnAwake = false;

            _audioSourcePool = new(pref, transform, 20, false);
            _aliveAudioData = new();
            _deadAudioData = new();

            _lastUpdateTime = 0;

            Destroy(pref);

            IsInitialized = true;
            _instance = this;
        }

        public AliveAudioData Play(AudioClipContainer container)
        {
            var source = _audioSourcePool.Pull();
            source.clip = container.AudioClip;
            source.transform.localPosition = Vector3.zero;
            source.volume = container.Volume;
            source.loop = container.Loop;
            source.gameObject.SetActive(true);
            source.Play();

            var aliveData = new AliveAudioData(container, source);
            _aliveAudioData.Add(aliveData);
            return aliveData;
        }

        public void Stop(AliveAudioData aliveAudioData)
        {
            if (aliveAudioData == null) return;
            if (aliveAudioData.IsDisposed) return;
            aliveAudioData.Source.clip = null;
            aliveAudioData.Source.Stop();
            _audioSourcePool.PushItem(aliveAudioData.Source.gameObject, !aliveAudioData.Source.transform.IsChildOf(transform));

            OnAudioDied?.Invoke(aliveAudioData);
            
            aliveAudioData.Dispose();
        }

        public void Pause(AliveAudioData aliveAudioData)
        {
            if (aliveAudioData == null) return;
            if (aliveAudioData.IsDisposed) return;

            aliveAudioData.Pause();
        }

        public void Resume(AliveAudioData aliveAudioData)
        {
            if (aliveAudioData == null) return;
            if (aliveAudioData.IsDisposed) return;

            aliveAudioData.Resume();
        }
    }
}