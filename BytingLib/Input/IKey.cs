namespace BytingLib
{
    public interface IKey
    {
        public bool Down { get; }
        public bool Pressed { get; }
        public bool Released { get; }
    }
}