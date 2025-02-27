using UnityEngine;

namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class RandomizeClipPitchEffect : AudioClipEffect
    {
        public float MinPitch = 0.98f;
        public float MaxPitch = 1.02f;

        public override void Apply(AudioClipContainer container)
        {
            var pitch = UnityEngine.Random.Range(MinPitch, MaxPitch);
            var latestAlive = container.GetLatestAliveAudioData();

            if (latestAlive == null) return;

            latestAlive.Source.pitch = Mathf.Clamp(pitch, -3f, 3f);
        }

        public override void Remove(AudioClipContainer container)
        {
            
        }
    }
}