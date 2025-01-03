using UnityEngine;
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
                }

                return _instance;
            }
        }
        private static AudioSystemManager _instance;

        public AudioSystemLibraryCollection Library { get; private set; }
        public AudioSystemSettings Settings { get; private set; }

        public bool IsInitialized { get; private set; } = false;

        public Action<AliveAudioData> OnAudioDied;

        private AudioSourcePool _audioSourcePool;
        private List<AliveAudioData> _aliveAudioData;
        private List<AliveAudioData> _deadAudioData;

        private float _lastUpdateTime;

        private void Awake()
        {
            Init();
        }

        private void LateUpdate()
        {
            if (Time.time - _lastUpdateTime < 0.1f) return;

            _lastUpdateTime = Time.time;

            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData.HasDied)
                {
                    _deadAudioData.Add(aliveData);
                }
            }

            foreach (var deadData in _deadAudioData)
            {
                _aliveAudioData.Remove(deadData);
                Stop(deadData);
            }

            _deadAudioData.Clear();
        }

        private void OnEnable()
        {
            Settings.OnUpdated += OnSettingsUpdated;
        }

        private void OnDisable()
        {
            Settings.OnUpdated -= OnSettingsUpdated;
            KillAll();
        }

        private void Init()
        {
            if (IsInitialized) return;

            Settings = GetSettings();
            Library = new(GetLibrary());

            _audioSourcePool = new(transform, Settings.AudioSourcePoolStartSize);
            _aliveAudioData = new();
            _deadAudioData = new();
            _lastUpdateTime = 0;

            IsInitialized = true;
            _instance = this;

            AudioSystemUtils.SetAllCategoryMuteStates(false, Settings);
            DontDestroyOnLoad(gameObject);
        }

        private void OnSettingsUpdated(AudioSystemSettings settings)
        {
            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData.PlayerInstanceId != 0)
                    aliveData.Container.RefreshAliveDatas();
            }
        }

        public AliveAudioData Play(AudioClipContainer container)
        {
            if (container == null) return null;

            var source = _audioSourcePool.Pull();
            source.clip = container.AudioClip;
            source.transform.localPosition = Vector3.zero;
            source.gameObject.SetActive(true);

            var aliveData = new AliveAudioData(container, source, false);
            _aliveAudioData.Add(aliveData);

            aliveData.Refresh();
            source.Play();

            return aliveData;
        }

        public AliveAudioData Play(AudioClipContainer container, AudioSource audioSource)
        {
            if (container == null) return null;
            if (audioSource == null) return null;

            audioSource.clip = container.AudioClip;
            audioSource.gameObject.SetActive(true);

            var aliveData = new AliveAudioData(container, audioSource, true);
            _aliveAudioData.Add(aliveData);

            aliveData.Refresh();
            audioSource.Play();

            return aliveData;
        }

        public AliveAudioData Play(uint containerID)
        {
            var container = Library.GetContainerFromID(containerID);
            if (container == null) return null;
            return Play(container);
        }

        public void Stop(AliveAudioData aliveAudioData)
        {
            if (aliveAudioData == null) return;
            if (aliveAudioData.IsDisposed) return;

            if (aliveAudioData.Source != null)
            {
                aliveAudioData.Source.clip = null;
                aliveAudioData.Source.Stop();
                if (!aliveAudioData.IsSourceCustom)
                    _audioSourcePool.PushItem(aliveAudioData.Source);
            }

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

        public void KillAll(AudioCategory? category = null)
        {
            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData == null) continue;
                if (category != null && aliveData.Container.CategoryID != category.Value.ID) continue;
                Stop(aliveData);
            }
            _aliveAudioData.Clear();
        }

        public void PauseAll(AudioCategory? category = null)
        {
            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData == null) continue;
                if (category != null && aliveData.Container.CategoryID != category.Value.ID) continue;
                Pause(aliveData);
            }
        }

        public void ResumeAll(AudioCategory? category = null)
        {
            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData == null) continue;
                if (category != null && aliveData.Container.CategoryID != category.Value.ID) continue;
                Resume(aliveData);
            }
        }

        public List<AliveAudioData> GetAliveDatas(AudioClipContainer container)
        {
            var result = new List<AliveAudioData>();

            if (container == null) return result;
            if (_aliveAudioData == null) return result;

            foreach (var aliveData in _aliveAudioData)
            {
                if (aliveData == null) continue;
                if (aliveData.Container == null) continue;

                if (aliveData.Container.ID == container.ID) result.Add(aliveData);
            }

            return result;
        }

        private AudioContainerLibrary GetLibrary()
        {
            var path = "Serojob-AudioSystem/AudioContainerLibrary";

            return Resources.Load<AudioContainerLibrary>(path);
        }

        private AudioSystemSettings GetSettings()
        {
            var path = "Serojob-AudioSystem/AudioSystemSettings";

            return Resources.Load<AudioSystemSettings>(path);
        }
    }
}