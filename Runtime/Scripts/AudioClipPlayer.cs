using System.Collections;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AudioClipPlayer : MonoBehaviour
    {
        public AudioClipContainer Container;

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

            _aliveAudioData = AudioSystemManager.Instance.Play(Container);
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