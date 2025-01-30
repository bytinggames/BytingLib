namespace BytingLib
{
    public struct MetaInputState
    {
        public MetaInputState(bool isGameActive)
        {
            IsGameActive = isGameActive;
        }

        public bool IsGameActive { get; internal set; }

        public static bool operator ==(MetaInputState a, MetaInputState b)
        {
            return a.IsGameActive == b.IsGameActive;
        }
        
        public static bool operator !=(MetaInputState a, MetaInputState b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is MetaInputState state &&
                   IsGameActive == state.IsGameActive;
        }

        public override int GetHashCode()
        {
            return IsGameActive ? 1 : 0;
        }
    }
}
