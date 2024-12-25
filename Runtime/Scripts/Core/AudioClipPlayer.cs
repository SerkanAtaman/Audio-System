using System.Collections;
using UnityEngine;
using SeroJob.FancyAttributes;
using System.Collections.Generic;

namespace SeroJob.AudioSystem
{
    public class AudioClipPlayer : MonoBehaviour
    {
        public enum PlayType
        {
            Container = 0,
            ContainerWithID = 1,
            RandomByTag = 2
        }

        public enum AutoPlayMode
        {
            None = 0,
            OnEnable = 1,
            OnStart = 2
        }

        public PlayType Type = PlayType.Container;
        public AutoPlayMode PlayMode = AutoPlayMode.None;
        public bool AllowSimultaneousPlay = false;

        public uint[] ContainerIDs;
        public uint TagID;
        public AudioClipContainer[] Containers;

        [ChildReferenceDropdown(typeof(AudioClipEffect))]
        [SerializeReference] public AudioClipEffect[] Effects;

        public bool SyncAudioSourceTransform = false;

        public bool IsPlaying
        {
            get
            {
                bool isPlaying = false;
                foreach (var item in AliveAudioDatas)
                {
                    if (!item.HasDied)
                    {
                        isPlaying = true;
                        break;
                    }
                }

                return isPlaying;
            }
        }

        public List<AliveAudioData> AliveAudioDatas
        {
            get
            {
                _aliveAudioDatas ??= new();
                return _aliveAudioDatas;
            }
        }
        private List<AliveAudioData> _aliveAudioDatas;

        private void OnEnable()
        {
            Stop();
            if (PlayMode == AutoPlayMode.OnEnable) Play();
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
            if (IsPlaying && !AllowSimultaneousPlay) return;

            AliveAudioData data = null;

            switch (Type)
            {
                case PlayType.Container:
                    data = Containers.GetRandomElement().Play();
                    break;
                case PlayType.ContainerWithID:
                    data = AudioSystemManager.Instance.Play(ContainerIDs.GetRandomElement());
                    break;
                case PlayType.RandomByTag:
                    var containers = AudioSystemManager.Instance.Library.GetContainersByTag(TagID);
                    data = containers.GetRandomElement().Play();
                    break;
            }

            if (data == null)
            {
                Debug.LogWarning("Failed to play audio clip", gameObject);
                return;
            }

            AliveAudioDatas.Add(data);

            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    effect.Apply(data.Container);
                }
            }

            if (AliveAudioDatas.Count < 2) AudioSystemManager.Instance.OnAudioDied += OnAudioDied;

            if (SyncAudioSourceTransform) StartCoroutine(SyncSourceTransform());
        }

        public void Pause()
        {
            foreach (var item in AliveAudioDatas)
            {
                item.Pause();
            }
        }

        public void Resume()
        {
            foreach (var item in AliveAudioDatas)
            {
                item.Resume();
            }
        }

        public void Stop()
        {
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
        }

        private void OnAudioDied(AliveAudioData aliveAudioData)
        {
            if (AliveAudioDatas.Contains(aliveAudioData))
                AliveAudioDatas.Remove(aliveAudioData);

            if (AliveAudioDatas.Count < 1) AudioSystemManager.Instance.OnAudioDied -= OnAudioDied;
        }

        private IEnumerator SyncSourceTransform()
        {
            while(AliveAudioDatas.Count > 0)
            {
                foreach (var item in AliveAudioDatas)
                {
                    item.Source.transform.position = transform.position;
                }

                yield return null;
            }
        }
    }
}