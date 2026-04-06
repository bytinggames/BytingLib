namespace BytingLib
{
    /// <summary>Whether an input is the last child is important. It has to be pressed last as well.</summary>
    public class BoolSequence : InputBool<BoolSequenceState>
    {
        public InputBool[] Children { get; }
        public int MaxUpdatesBetweenInputs { get; set; } = 20;

        public BoolSequence(params InputBool[] children)
        {
            this.Children = children;
        }

        protected override bool CalculateValue(FullInput input, BoolSequenceState state)
        {
            if (Children.Length == 0)
            {
                return false;
            }

            InputBoolState[] states = Children.Select(f => f.GetState(state.Updater)).ToArray();

            for (int i = 0; i < state.Sequences.Count; i++)
            {
                var sequence = state.Sequences[i];
                if (sequence.ChildIndex == Children.Length) // cycled through all children?
                {
                    if (states[^1].Down) // still down?
                    {
                        // remove all other running sequences
                        if (i > 0)
                        {
                            state.Sequences.RemoveRange(0, i);
                        }
                        if (i + 1 < state.Sequences.Count)
                        {
                            state.Sequences.RemoveRange(i + 1, state.Sequences.Count - (i + 1));
                        }
                        return true;
                    }
                    else
                    {
                        // last bind released - reset
                        state.Sequences.RemoveAt(i--);
                    }
                }
                else if (states[sequence.ChildIndex].Pressed)
                {
                    // forward to next child
                    sequence.ChildIndex++;
                    sequence.UpdatesPassedAtCurrentIndex = 0;
                }
                else
                {
                    sequence.UpdatesPassedAtCurrentIndex++;
                    if (sequence.UpdatesPassedAtCurrentIndex > MaxUpdatesBetweenInputs)
                    {
                        // took to long - reset
                        state.Sequences.RemoveAt(i--);
                    }
                }
            }

            // start new sequence?
            if (states[0].Pressed)
            {
                state.Sequences.Add(new BoolSequenceState.RunningSequence());

                if (states.Length == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<Input> GetChildren()
        {
            return Children;
        }

        public override string ToString()
        {
            return string.Join(" > ", (object?[])Children);
        }

        public override BoolSequenceState CreateState(InputUpdater updater)
        {
            return new BoolSequenceState(updater);
        }
    }

    public class BoolSequenceState : InputBoolState
    {
        public class RunningSequence
        {
            public int ChildIndex = 1; // directly start at 2nd child
            public int UpdatesPassedAtCurrentIndex;
        }

        public List<RunningSequence> Sequences = new();
        
        public BoolSequenceState(InputUpdater updater) : base(updater)
        {
        }
    }
}
