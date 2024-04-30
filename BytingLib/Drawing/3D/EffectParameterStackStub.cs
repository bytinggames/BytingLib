namespace BytingLib
{
    public class EffectParameterStackStub<T> : EffectParameterStack<T>
    {
        public override IDisposable? Use(Func<T, T> func)
        {
            return null;
        }

        public override void Use(T val, Action actionWhile)
        {
        }
    }
}
