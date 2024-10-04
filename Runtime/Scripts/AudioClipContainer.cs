using System;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AudioClipContainer : ScriptableObject
    {
        [SerializeField] private uint _id;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private string _category;
        [SerializeField] private string _tag;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private bool _loop = false;
        [SerializeField] private uint _categoryID;
        [SerializeField] private uint _tagID;

        public uint ID => _id;
        public AudioClip AudioClip => _audioClip;
        public string Category => _category;
        public string Tag => _tag;
        public float Volume => _volume;
        public bool Loop => _loop;
        public uint CategoryID => _categoryID;
        public uint TagID => _tagID;

        #region EDITOR
#if UNITY_EDITOR
        public void ReceiveEditorData(AudioClip audioClip, uint id)
        {
            _audioClip = audioClip;
            _category = "None";
            _tag = "None";
            _volume = 1f;
            _loop = false;
            _id = id;
        }
#endif
        #endregion
    }
}