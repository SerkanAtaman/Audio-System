using DG.Tweening;

namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public class TweenClipVolumeEffect : AudioClipEffect
    {
        public bool SetStartVolume = true;
        public bool TimeScaleIndependent = false;
        public float StartVolume;
        public float EndVolume;
        public float Duration;

        private Tween _tween;

        public override void Apply(AudioClipContainer container)
        {
            _tween?.Kill();

            float startVolume = container.CurrentVolume;
            if (SetStartVolume) startVolume = StartVolume;

            _tween = DOVirtual.Float(startVolume, EndVolume, Duration, (value) =>
            {
                container.CurrentVolume = value;
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