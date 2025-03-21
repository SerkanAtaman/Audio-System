using System.Collections;
using UnityEngine;
using SeroJob.FancyAttributes;
using System.Collections.Generic;
using UnityEngine.Events;

namespace SeroJob.AudioSystem
{
    public class AudioClipPlayer : MonoBehaviour
    {
        public enum PlayType
        {
            Container = 0,
            ContainerWithID = 1,
            RandomByTag = 2,
        }

        public enum AutoPlayMode
        {
            None = 0,
            OnEnable = 1,
            OnStart = 2
        }

        public enum State
        {
            Idle = 0,
            Playing = 1,
            Paused = 2
        }

        public PlayType Type = PlayType.Container;
        public AutoPlayMode PlayMode = AutoPlayMode.None;
        public bool AllowSimultaneousPlay = false;
        public bool ChooseClipsRespectively = false;
        public bool ClearRespectiveDataWhenStopped = true;
        public bool SyncAudioSourceTransform = false;
        public bool DestroyPlayerWhenStopped = false;

        public uint[] ContainerIDs;
        public uint TagID;
        public AudioClipContainer[] Containers;
        public AudioSource CustomAudioSource;

        public UnityEvent OnPlayStarted;
        public UnityEvent OnPaused;
        public UnityEvent OnStopped;
        public UnityEvent OnPlayFinished;

        [ChildReferenceDropdown]
        [SerializeReference] public AudioClipEffect[] Effects;

        [SerializeField] private float _volume = 1f;
        [SerializeField] private State _state;
        [SerializeField] private List<AudioClipContainer> _respectiveContainers = null;

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = Mathf.Clamp01(value);
                if (_aliveAudioDatas == null) return;
                foreach (var data in _aliveAudioDatas)
                {
                    data.Source.volume = _volume * data.Container.GetTargetVolume();
                }
            }
        }

        public State CurrentState => _state;

        public List<AliveAudioData> AliveAudioDatas
        {
            get
            {
                _aliveAudioDatas ??= new();
                return _aliveAudioDatas;
            }
        }
        private List<AliveAudioData> _aliveAudioDatas;

        

        private void Awake()
        {
            if (CustomAudioSource != null) CustomAudioSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            Stop(false);
            if (CustomAudioSource != null) CustomAudioSource.playOnAwake = false;
            if (PlayMode == AutoPlayMode.OnEnable) Play();
        }

        private void OnValidate()
        {
            Volume = _volume;
        }

        private void Start()
        {
            if (PlayMode == AutoPlayMode.OnStart) Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            if (!isActiveAndEnabled) return;
            if (_state != State.Idle && !AllowSimultaneousPlay) return;

            var container = GetContainerToPlay();

            if (container == null)
            {
                Debug.LogWarning("Failed to get container to play", gameObject);
                return;
            }

            var data = container.Play(CustomAudioSource);
            data.PlayerInstanceId = gameObject.GetInstanceID();
            data.Source.volume *= Volume;
            AliveAudioDatas.Add(data);

            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    effect.Apply(data.Container);
                }
            }

            SetState(State.Playing, false);

            if (AliveAudioDatas.Count < 2) AudioSystemManager.Instance.OnAudioDied += OnAudioDied;
            if (SyncAudioSourceTransform) StartCoroutine(SyncSourceTransform());
        }

        public void Pause()
        {
            SetState(State.Paused, true);

            foreach (var item in AliveAudioDatas)
            {
                item.Pause();
            }
        }

        public void Resume()
        {
            SetState(State.Playing, true);
            foreach (var item in AliveAudioDatas)
            {
                item.Resume();
            }
        }

        public void Stop(bool canDestroy = true, bool isForced = true)
        {
            if (AudioSystemManager.Instance != null)
                AudioSystemManager.Instance.OnAudioDied -= OnAudioDied;

            foreach (var item in AliveAudioDatas)
            {
                foreach (var effect in Effects)
                {
                    effect.Remove(item.Container);
                }
                item.Stop();
            }

            AliveAudioDatas.Clear();
            if (ClearRespectiveDataWhenStopped)
            {
                _respectiveContainers?.Clear();
                _respectiveContainers = null;
            }
            SetState(State.Idle, isForced);

            if (DestroyPlayerWhenStopped && canDestroy) Destroy(gameObject);
        }

        private void SetState(State state, bool isForced)
        {
            if (_state == state) return;

            _state = state;

            switch (_state)
            {
                case State.Playing:
                    OnPlayStarted?.Invoke();
                    break;
                case State.Paused:
                    OnPaused?.Invoke();
                    break;
                case State.Idle:
                    if (isForced) OnStopped?.Invoke();
                    else OnPlayFinished?.Invoke();
                    break;
            }
        }

        private AudioClipContainer GetContainerToPlay()
        {
            AudioClipContainer container = null;

            switch (Type)
            {
                case PlayType.Container:
                    if (!ChooseClipsRespectively)
                    {
                        container = Containers.GetRandomElement();
                    }
                    else
                    {
                        container = GetContainerRespectively();
                    }
                    break;
                case PlayType.ContainerWithID:
                    container = AudioSystemUtils.GetContainerByID(ContainerIDs.GetRandomElement());
                    break;
                case PlayType.RandomByTag:
                    var containers = AudioSystemManager.Instance.Library.GetContainersByTag(TagID);
                    container = containers.GetRandomElement();
                    break;
            }

            return container;
        }

        private AudioClipContainer GetContainerRespectively()
        {
            _respectiveContainers ??= new(Containers);

            if (_respectiveContainers.Count == 0) _respectiveContainers.AddRange(Containers);

            var result = _respectiveContainers[0];
            _respectiveContainers.RemoveAt(0);
            return result;
        }

        private void OnAudioDied(AliveAudioData aliveAudioData)
        {
            if (AliveAudioDatas.Contains(aliveAudioData))
                AliveAudioDatas.Remove(aliveAudioData);

            if (AliveAudioDatas.Count < 1) Stop(true, false);
        }

        private IEnumerator SyncSourceTransform()
        {
            while (AliveAudioDatas != null && AliveAudioDatas.Count > 0)
            {
                foreach (var item in AliveAudioDatas)
                {
                    if (item == null) continue;

                    item.Source.transform.position = transform.position;
                }

                yield return null;
            }
        }
    }
}