namespace BytingLib
{

    public class EffectParameterStack<T> : IEffectParameterStack
    {
        public EffectParameterStack(Ref<Effect> effect, string parameter)
        {
            effectParameter = effect.Value.Parameters[parameter];

            effect.OnReload += RefreshEffect;
            this.effect = effect;
        }

        public void Dispose()
        {
            effect.OnReload -= RefreshEffect;
        }

        public void RefreshEffect(Ref<Effect> effect)
        {
            effectParameter = effect.Value.Parameters[effectParameter.Name];
        }

        public EffectParameterStack(Ref<Effect> effect, string parameter, Func<T, IDisposable?> useChain)
            : this(effect, parameter)
        {
            this.useChain = useChain;
        }

        private readonly Stack<T> valueStack = new();
        private EffectParameter effectParameter;
        private readonly Func<T, IDisposable?>? useChain = null;
        private readonly Ref<Effect> effect;
        private T? lastAppliedValue = default;
        bool dirty;

        public void Apply()
        {
            if (!dirty)
                return;
            //if (lastAppliedValue == null || !lastAppliedValue.Equals(valueStack.Peek()))
            {
                if (valueStack.TryPeek(out lastAppliedValue))
                    effectParameter.SetValueObject(lastAppliedValue!);
                else
                    effectParameter.SetValueObject(default(T)!);
            }
            dirty = false;
        }

        public IDisposable? Use(T val)
        {
            IDisposable? d1 = null;
            if (!valueStack.TryPeek(out T? peek) || !peek!.Equals(val))
            {
                valueStack.Push(val);

                dirty = true;
                d1 = new OnDispose(() => { valueStack.Pop(); dirty = true; });
            }

            if (useChain != null)
            {
                IDisposable? d2 = useChain(val);
                if (d2 != null)
                {
                    if (d1 != null)
                        return new DisposableContainer(d1, d2);
                    else
                        return d2;
                }
            }

            return d1;
        }
        public void Use(T val, Action actionWhile)
        {
            bool valChanged = false;
            if (!valueStack.TryPeek(out T? peek) || !peek!.Equals(val))
            {
                valueStack.Push(val);

                valChanged = true;
                dirty = true;
            }

            if (useChain != null)
            {
                //IDisposable? d2 = useChain(val);
                //if (d2 != null)
                //{
                //    if (d1 != null)
                //        return new DisposableContainer(d1, d2);
                //    else
                //        return d2;
                //}
            }

            actionWhile();

            if (valChanged)
            {
                valueStack.Pop();
                dirty = true;
            }
        }
        public IDisposable? Use(Func<T, T> func)
        {
            T val = func(valueStack.Peek());
            return Use(val);
        }
    }
}
