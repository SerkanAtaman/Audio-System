namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class TweenClipVolumeEffect : AudioClipEffect
    {
        public bool TimeScaleIndependent = false;
        public float StartVolume;
        public float EndVolume;
        public float Duration;

        private System.Collections.IEnumerator _coroutine;

        public override void Apply(AudioClipContainer container)
        {
            AudioSystemManager.StopRunningCoroutine(_coroutine);

            _coroutine = AudioSystemManager.RunVolumeCoroutine(StartVolume, EndVolume, Duration, !TimeScaleIndependent, (delta, value) =>
            {
                var alives = container.GetAllAliveAudioData();
                var target = container.GetTargetVolume();
                foreach (var alive in alives)
                {
                    alive.Source.volume = target * value;
                }

                if (delta >= 1)
                {
                    _coroutine = null;
                }
            });
        }

        public override void Remove(AudioClipContainer container)
        {
            AudioSystemManager.StopRunningCoroutine(_coroutine);
            _coroutine = null;
        }
    }
}