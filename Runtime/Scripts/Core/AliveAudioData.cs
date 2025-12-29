using System;
using UnityEngine;

namespace SeroJob.AudioSystem
{
    public class AliveAudioData : IDisposable
    {
        public AudioClipContainer Container { get; private set; }
        public AudioSource Source { get; private set; }
        public AudioClipPlayer Player { get; internal set; }

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

                return Time.time > BornTime + LifeTime + 0.05f + TotalPauseTime;
            }
        }

        public AliveAudioData(AudioClipContainer container, AudioSource source, bool isSourceCustom)
        {
            Player = null;
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

        public void Restart()
        {
            if (Source == null) return;

            if (IsPaused)
            {
                IsPaused = false;
                TotalPauseTime += Time.time - PauseStartTime;
            }

            BornTime = Time.time;
            Source.Stop();
            Source.Play();
        }

        public void Dispose()
        {
            Container = null;
            Source = null;
            Player = null;
            IsDisposed = true;
        }
    }
}