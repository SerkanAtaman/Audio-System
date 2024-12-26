namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class OverrideLoopState : AudioClipEffect
    {
        public bool Looped = false;

        public override void Apply(AudioClipContainer container)
        {
            var latestAlive = container.GetLatestAliveAudioData();
            if (latestAlive == null) return;

            latestAlive.Source.loop = Looped;
        }

        public override void Remove(AudioClipContainer container)
        {

        }
    }
}