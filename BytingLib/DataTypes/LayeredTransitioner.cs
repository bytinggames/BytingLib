
namespace BytingLib
{
    /// <summary>Used for transitioning between animations f.ex.</summary>
    public class LayeredTransitioner<TValue>
    {
        TValue oldestValue;
        List<Transition<TValue>> transitions = new();

        float currentSecond = 0f;

        public LayeredTransitioner(TValue startValue)
        {
            oldestValue = startValue;
        }

        public void TransitTo(TValue value, float transitionDurationSeconds)
        {
            transitions.Add(new Transition<TValue>(value, transitionDurationSeconds));
        }

        public void Update(float elapsedSeconds)
        {
            // iterate from newest to oldest, cause if the newest transition finishes faster than the older ones, the older ones will be deleted anyways
            for (int i = transitions.Count - 1; i >= 0; i--)
            {
                transitions[i].Update(elapsedSeconds);

                if (transitions[i].HasTransitionFinished())
                {
                    oldestValue = transitions[i].GetEndValue();
                    transitions.RemoveRange(0, i + 1);
                    break; // no need to continue further, the older transitions have been deleted
                }
            }
        }

        public void ApplyBlend(Action<TValue> blendBase, Action<TValue, float> interpolate)
        {
            blendBase(oldestValue);
            for (int i = 0; i < transitions.Count; i++)
            {
                interpolate(transitions[i].GetEndValue(), transitions[i].CurrentBlendAmount);
            }
        }

        public IEnumerable<TValue> GetAllValues()
        {
            yield return oldestValue;
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
                    CurrentBlendAmount = 1;
                else if (CurrentBlendAmount < 0) // just in case someone puts in negative elapsedSeconds
                    CurrentBlendAmount = 0;
            }

            public bool HasTransitionFinished()
            {
                return CurrentBlendAmount >= 1;
            }

            public T GetEndValue() => endValue;
        }
    }
}
