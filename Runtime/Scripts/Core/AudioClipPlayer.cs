using System.Collections;
using UnityEngine;
using SeroJob.FancyAttributes;

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
                if (_aliveAudioData == null) return false;

                return !_aliveAudioData.HasDied;
            }
        }

        private AliveAudioData _aliveAudioData;

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
            if (IsPlaying) return;

            switch (Type)
            {
                case PlayType.Container:
                    _aliveAudioData = Containers.GetRandomElement().Play();
                    break;
                case PlayType.ContainerWithID:
                    _aliveAudioData = AudioSystemManager.Instance.Play(ContainerIDs.GetRandomElement());
                    break;
                case PlayType.RandomByTag:
                    var containers = AudioSystemManager.Instance.Library.GetContainersByTag(TagID);
                    _aliveAudioData = containers.GetRandomElement().Play();
                    break;
            }

            if (_aliveAudioData == null)
            {
                Debug.LogWarning("Failed to play audio clip", gameObject);
                return;
            }

            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    effect.Apply(_aliveAudioData.Container);
                }
            }

            AudioSystemManager.Instance.OnAudioDied += OnAudioDied;

            if (SyncAudioSourceTransform) StartCoroutine(SyncSourceTransform());
        }

        public void Pause()
        {
            if (_aliveAudioData == null) return;

            _aliveAudioData.Pause();
        }

        public void Resume()
        {
            if (_aliveAudioData == null) return;

            _aliveAudioData.Resume();
        }

        public void Stop()
        {
            if (_aliveAudioData == null) return;

            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    effect.Remove(_aliveAudioData.Container);
                }
            }

            _aliveAudioData.Stop();
        }

        private void OnAudioDied(AliveAudioData aliveAudioData)
        {
            AudioSystemManager.Instance.OnAudioDied -= OnAudioDied;

            if (_aliveAudioData == null) return;
            if(_aliveAudioData == aliveAudioData)
            {
                _aliveAudioData = null;
            }
        }

        private IEnumerator SyncSourceTransform()
        {
            while(_aliveAudioData != null)
            {
                _aliveAudioData.Source.transform.position = transform.position;

                yield return null;
            }
        }
    }
}