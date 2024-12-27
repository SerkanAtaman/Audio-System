using System;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AliveAudioData : IDisposable
    {
        public AudioClipContainer Container { get; private set; }
        public AudioSource Source { get; private set; }

        public int PlayerInstanceId { get; set; } = 0;

        public float BornTime { get; private set; }
        public float LifeTime { get; private set; }
        public float TotalPauseTime { get; private set; }
        public float PauseStartTime { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsSourceCustom { get; private set; }

        public bool HasDied
        {
            get
            {
                if (IsDisposed) return true;
                if (Source.loop) return false;

                return Time.time > BornTime + LifeTime + 1f + TotalPauseTime;
            }
        }

        public AliveAudioData(AudioClipContainer container, AudioSource source, bool isSourceCustom)
        {
            PlayerInstanceId = 0;
            Container = container;
            Source = source;
            BornTime = Time.time;
            LifeTime = Container.Loop ? float.MaxValue : Container.AudioClip.length;
            IsPaused = false;
            TotalPauseTime = 0f;
            IsDisposed = false;
            IsSourceCustom = isSourceCustom;
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            PauseStartTime = Time.time;
            Source.Pause();
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            TotalPauseTime += Time.time - PauseStartTime;
            Source.Play();
        }

        public void Dispose()
        {
            Container = null;
            Source = null;
            IsDisposed = true;
        }
    }
}