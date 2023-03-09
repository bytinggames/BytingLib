using System.Diagnostics;

namespace BytingLib
{
    public class EffectParameterStack<T> : IEffectParameterStack
    {
        private readonly Stack<T> valueStack = new();
        private EffectParameter? effectParameter;
        private Func<T, IDisposable?>? useChain = null;
        private readonly Ref<Effect> effect;
        private readonly string parameterName;
        private T? lastAppliedValue = default;
        bool dirty;

        public EffectParameterStack(Ref<Effect> effect, string parameter, T? _default)
        {
            effectParameter = effect.Value.Parameters[parameter];
            if (effectParameter == null)
            {
                string msg = "couldn't find parameter '" + parameter + "' in effect " + effect.Value.Name;
                Debug.WriteLine("Warning: " + msg);
                //throw new KeyNotFoundException(msg);
            }
            effect.OnReload += RefreshEffect;
            this.effect = effect;
            this.parameterName = parameter;
            if (_default != null)
            {
                PushAndSetDirty(_default);
            }
        }

        public EffectParameterStack(Ref<Effect> effect, string parameter)
            : this(effect, parameter, GetDefault())
        {
        }

        public EffectParameterStack(Ref<Effect> effect, string parameter, Func<T, IDisposable?> useChain)
            : this(effect, parameter)
        {
            this.useChain = useChain;
        }

        public EffectParameterStack(Ref<Effect> effect, string parameter, Func<T, IDisposable?> useChain, T _default)
            : this(effect, parameter, _default)
        {
            this.useChain = useChain;
        }

        private static T? GetDefault()
        {
            if (typeof(T) == typeof(Matrix))
                return (T)(object)Matrix.Identity;
            else
                return default;
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
            effectParameter = effect.Value.Parameters[parameterName];
        }

        public void Apply()
        {
            if (!dirty || effectParameter == null)
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
            if (!IsEqualToPeek(val))
            {
                PushAndSetDirty(val);
                d1 = new OnDispose(PopAndSetDirty);
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
            if (!IsEqualToPeek(val))
            {
                PushAndSetDirty(val);

                valChanged = true;
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
                PopAndSetDirty();
        }

        private bool IsEqualToPeek(T? val)
        {
            if (!valueStack.TryPeek(out T? peek))
                return false;
            if (peek == null)
                return val == null;
            else
                return peek.Equals(val);
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

        private void PushAndSetDirty(T val)
        {
            valueStack.Push(val);
            dirty = true;
        }
        private void PopAndSetDirty()
        {
            valueStack.Pop();
            dirty = true;
        }
    }
}
