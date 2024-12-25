using UnityEngine;

namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class SetPitchEffect : AudioClipEffect
    {
        public float Pitch = 1f;

        public override void Apply(AudioClipContainer container)
        {
            var latestAlive = container.GetLatestAliveAudioData();
            if (latestAlive == null) return;

            latestAlive.Source.pitch = Mathf.Clamp(Pitch, -3f, 3f);
        }

        public override void Remove(AudioClipContainer container)
        {

        }
    }
}