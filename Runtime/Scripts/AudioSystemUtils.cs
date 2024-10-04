using System.Collections.Generic;

namespace SeroJob.AudioSystem
{
    public static class AudioSystemUtils
    {
        public static T GetRandomElement<T>(this IList<T> list)
        {
            if (list == null) return default;
            if (list.Count == 0) return default;
            if (list.Count == 1) return list[0];

            var randIndex = UnityEngine.Random.Range(0, list.Count);

            return list[randIndex];
        }
    }
}