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
        [SerializeField, Range(0f, 1f)] private float _baseVolume = 1f;
        [SerializeField, Range(-3f, 3f)] private float _pitch = 1f;
        [SerializeField, Range(0f, 1f)] private float _spatialBlend = 0f;
        [SerializeField] private bool _loop = false;
        [SerializeField] private uint _categoryID;
        [SerializeField] private uint _tagID;

        public uint ID => _id;
        public AudioClip AudioClip => _audioClip;
        public string Category => _category;
        public string Tag => _tag;
        public uint CategoryID => _categoryID;
        public uint TagID => _tagID;

        public float BaseVolume
        {
            get
            {
                return _baseVolume;
            }
            set
            {
                _baseVolume = Mathf.Clamp01(value);
                this.RefreshAliveDatas();
            }
        }
        public float Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                _pitch = Mathf.Clamp(value, -3f, 3f);
                this.RefreshAliveDatas();
            }
        }
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                this.RefreshAliveDatas();
            }
        }
        public float SpatialBlend
        {
            get
            {
                return _spatialBlend;
            }
            set
            {
                _spatialBlend = Mathf.Clamp01(value);
                this.RefreshAliveDatas();
            }
        }

        #region EDITOR
#if UNITY_EDITOR
        public void ReceiveEditorData(AudioClip audioClip, uint id)
        {
            _audioClip = audioClip;
            _category = "None";
            _tag = "None";
            _baseVolume = 1f;
            _pitch = 1f;
            _spatialBlend = 0f;
            _loop = false;
            _id = id;
        }
#endif
        #endregion
    }
}