namespace BytingLib
{
    public interface IBoolDelta
    {
        public bool Down { get; }
        public bool Pressed { get; }
        public int DownTime { get; }
        public bool Released { get; }
        public int ReleasedTime { get; }
    }
}
