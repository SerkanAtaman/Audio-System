using System;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AudioClipContainer : ScriptableObject
    {
        [SerializeField] private string _identifier;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private string _category;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private bool _loop = false;

        public string Identifier => _identifier;
        public AudioClip AudioClip => _audioClip;
        public string Category => _category;
        public float Volume => _volume;
        public bool Loop => _loop;

        #region EDITOR
#if UNITY_EDITOR
        public void ReceiveEditorData(AudioClip audioClip, string identifier = null)
        {
            _audioClip = audioClip;
            _category = "None";
            _volume = 1f;
            _loop = false;
            _identifier = identifier ?? Guid.NewGuid().ToString();
        }
#endif
        #endregion
    }
}