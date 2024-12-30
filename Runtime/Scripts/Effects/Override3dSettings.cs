using UnityEngine;

namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class Override3dSettings : AudioClipEffect
    {
        public float DopplerLevel = 1;
        public float Spread = 0;
        public float MinDistance = 1;
        public float MaxDistance = 1;
        public AudioRolloffMode RolloffMode;

        public override void Apply(AudioClipContainer container)
        {
            var latestAlive = container.GetLatestAliveAudioData();
            if (latestAlive == null) return;

            latestAlive.Source.dopplerLevel = Mathf.Clamp(DopplerLevel, 0, 5);
            latestAlive.Source.spread = Mathf.Clamp(Spread, 0, 360);
            latestAlive.Source.minDistance = Mathf.Clamp(MinDistance, 0, float.MaxValue);
            latestAlive.Source.maxDistance = Mathf.Clamp(MaxDistance, MinDistance + 0.01f, float.MaxValue);
            latestAlive.Source.rolloffMode = RolloffMode;
        }

        public override void Remove(AudioClipContainer container)
        {

        }
    }
}