using DG.Tweening;

namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class TweenClipVolumeEffect : AudioClipEffect
    {
        public bool TimeScaleIndependent = false;
        public float StartVolume;
        public float EndVolume;
        public float Duration;

        private Tween _tween;

        public override void Apply(AudioClipContainer container)
        {
            _tween?.Kill();

            _tween = DOVirtual.Float(StartVolume, EndVolume, Duration, (value) =>
            {
                var alives = container.GetAllAliveAudioData();
                var target = container.GetTargetVolume();
                foreach (var alive in alives)
                {
                    alive.Source.volume = target * value;
                }
            });
            _tween.onComplete += () =>
            {
                _tween = null;
            };

            if (TimeScaleIndependent) _tween.SetUpdate(true);
        }

        public override void Remove(AudioClipContainer container)
        {
            _tween?.Kill();
        }
    }
}