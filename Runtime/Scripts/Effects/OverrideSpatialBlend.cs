using UnityEngine;

namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class OverrideSpatialBlend : AudioClipEffect
    {
        public float SpatialBlend = 0f;

        public override void Apply(AudioClipContainer container)
        {
            var latestAlive = container.GetLatestAliveAudioData();
            if (latestAlive == null) return;

            latestAlive.Source.spatialBlend = Mathf.Clamp01(SpatialBlend);
        }

        public override void Remove(AudioClipContainer container)
        {

        }
    }
}