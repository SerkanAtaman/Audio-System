namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public abstract class AudioClipEffect
    {
        public abstract void Apply(AudioClipContainer container);
        public abstract void Remove(AudioClipContainer container);
    }
}