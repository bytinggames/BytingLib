namespace BytingLib
{
    public struct MetaInputState
    {
        public MetaInputState(bool isActiveThisUpdate)
        {
            IsActivatedThisUpdate = isActiveThisUpdate;
        }

        public bool IsActivatedThisUpdate { get; internal set; }

        public static bool operator ==(MetaInputState a, MetaInputState b)
        {
            return a.IsActivatedThisUpdate == b.IsActivatedThisUpdate;
        }
        
        public static bool operator !=(MetaInputState a, MetaInputState b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is MetaInputState state &&
                   IsActivatedThisUpdate == state.IsActivatedThisUpdate;
        }

        public override int GetHashCode()
        {
            return IsActivatedThisUpdate ? 1 : 0;
        }
    }
}
