namespace BytingLib
{

    public class EffectParameterStack<T> : IEffectParameterStack
    {
        private readonly Stack<T> valueStack = new();
        private EffectParameter effectParameter;
        private Func<T, IDisposable?>? useChain = null;
        private readonly Ref<Effect> effect;
        private T? lastAppliedValue = default;
        bool dirty;

        public EffectParameterStack(Ref<Effect> effect, string parameter)
        {
            effectParameter = effect.Value.Parameters[parameter];
            if (effectParameter == null)
                throw new KeyNotFoundException("couldn't find parameter '" + parameter + "' in effect " + effect.Value.Name);

            effect.OnReload += RefreshEffect;
            this.effect = effect;
        }

        public EffectParameterStack(Ref<Effect> effect, string parameter, Func<T, IDisposable?> useChain)
            : this(effect, parameter)
        {
            this.useChain = useChain;
        }

        public void Dispose()
        {
            effect.OnReload -= RefreshEffect;
        }

        public void SetUseChain(Func<T, IDisposable?> useChain)
        {
            this.useChain = useChain;
        }

        public void RefreshEffect(Ref<Effect> effect)
        {
            effectParameter = effect.Value.Parameters[effectParameter.Name];
        }

        public void Apply()
        {
            if (!dirty)
                return;
            //if (lastAppliedValue == null || !lastAppliedValue.Equals(valueStack.Peek()))
            {
                if (valueStack.TryPeek(out lastAppliedValue))
                    effectParameter.SetValueObject(lastAppliedValue!);
                else if (typeof(T).IsValueType)
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
        public T? GetValue()
        {
            valueStack.TryPeek(out T? t);
            return t;
        }
    }
}
