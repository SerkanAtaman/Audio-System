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
            container.Pitch = pitch;
        }

        public override void Remove(AudioClipContainer container)
        {
            
        }
    }
}