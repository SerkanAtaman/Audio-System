namespace SeroJob.AudioSystem
{
    [System.Serializable]
    public struct AudioCategory
    {
        public string Name;
        public uint ID;
        public float Volume;
        public bool Muted;
    }
}