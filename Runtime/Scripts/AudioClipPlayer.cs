using System.Collections;
using UnityEngine;

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

        public PlayType Type = PlayType.Container;

        public uint[] ContainerIDs;
        public uint TagID;
        public AudioClipContainer[] Containers;

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
                    _aliveAudioData = AudioSystemManager.Instance.Play(Containers.GetRandomElement());
                    break;
                case PlayType.ContainerWithID:
                    _aliveAudioData = AudioSystemManager.Instance.Play(ContainerIDs.GetRandomElement());
                    break;
                case PlayType.RandomByTag:
                    var containers = AudioSystemManager.Instance.Library.GetContainersByTag(TagID);
                    _aliveAudioData = AudioSystemManager.Instance.Play(containers.GetRandomElement());
                    break;
            }

            if (_aliveAudioData == null)
            {
                Debug.LogWarning("Failed to play audio clip", gameObject);
                return;
            }

            AudioSystemManager.Instance.OnAudioDied += OnAudioDied;

            if (SyncAudioSourceTransform) StartCoroutine(SyncSourceTransform());
        }

        public void Pause()
        {
            if (_aliveAudioData == null) return;

            AudioSystemManager.Instance.Pause(_aliveAudioData);
        }

        public void Resume()
        {
            if (_aliveAudioData == null) return;

            AudioSystemManager.Instance.Resume(_aliveAudioData);
        }

        public void Stop()
        {
            if (_aliveAudioData == null) return;

            AudioSystemManager.Instance.Stop(_aliveAudioData);
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