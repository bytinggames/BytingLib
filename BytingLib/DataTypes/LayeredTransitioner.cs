﻿
namespace BytingLib
{
    /// <summary>Used for transitioning between animations f.ex.</summary>
    public class LayeredTransitioner<TValue>
    {
        public TValue OldestValue { get; private set; }
        List<Transition<TValue>> transitions = new();
        public event Action<TValue>? OnTransitionDone;

        public LayeredTransitioner(TValue startValue)
        {
            OldestValue = startValue;
        }

        public int TransitionCount => transitions.Count;

        public void TransitTo(TValue value, float transitionDurationSeconds)
        {
            if (transitionDurationSeconds <= 0)
            {
                OnTransitionDone?.Invoke(OldestValue);
                OldestValue = value;
                while (transitions.Count > 0)
                {
                    OnTransitionDone?.Invoke(transitions[0].GetEndValue());
                    transitions.RemoveAt(0);
                }
            }
            else
            {
                transitions.Add(new Transition<TValue>(value, transitionDurationSeconds));
            }
        }

        public void Update(float elapsedSeconds)
        {
            // iterate from newest to oldest, cause if the newest transition finishes faster than the older ones, the older ones will be deleted anyways
            for (int i = transitions.Count - 1; i >= 0; i--)
            {
                transitions[i].Update(elapsedSeconds);

                if (transitions[i].HasTransitionFinished())
                {
                    OnTransitionDone?.Invoke(OldestValue);
                    OldestValue = transitions[i].GetEndValue();

                    int keepCount = transitions.Count - i;
                    while (transitions.Count > keepCount) // skip all older transitions
                    {
                        OnTransitionDone?.Invoke(transitions[0].GetEndValue());
                        transitions.RemoveAt(0);
                    }
                    transitions.RemoveAt(0); // remove the transition that was set to the OldestValue
                    break; // no need to continue further, the older transitions have been deleted
                }
            }
        }

        public void ApplyBlend(Action<TValue> blendBase, Action<TValue, float> interpolate)
        {
            blendBase(OldestValue);
            for (int i = 0; i < transitions.Count; i++)
            {
                interpolate(transitions[i].GetEndValue(), transitions[i].CurrentBlendAmount);
            }
        }

        public IEnumerable<TValue> GetAllValues()
        {
            yield return OldestValue;
            for (int i = 0; i < transitions.Count; i++)
            {
                yield return transitions[i].GetEndValue();
            }
        }

        class Transition<T>
        {
            readonly T endValue;
            public float CurrentBlendAmount { get; private set; } = 0f;
            readonly float durationSeconds;

            public Transition(T endValue, float durationSeconds)
            {
                this.endValue = endValue;
                this.durationSeconds = durationSeconds;
            }

            internal void Update(float elapsedSeconds)
            {
                CurrentBlendAmount += elapsedSeconds / durationSeconds;
                if (CurrentBlendAmount >= 1)
                {
                    CurrentBlendAmount = 1;
                }
                else if (CurrentBlendAmount < 0) // just in case someone puts in negative elapsedSeconds
                {
                    CurrentBlendAmount = 0;
                }
            }

            public bool HasTransitionFinished()
            {
                return CurrentBlendAmount >= 1;
            }

            public T GetEndValue() => endValue;
        }
    }
}
