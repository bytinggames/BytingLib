namespace BytingLib
{
    public class BoolAnd : InputBoolSimple
    {
        public InputBool[] Children { get; }

        public BoolAnd(params InputBool[] children)
        {
            this.Children = children;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            if (Children.Length == 0)
            {
                return false;
            }
            long shortestDownTime = long.MaxValue;
            for (int i = 0; i < Children.Length - 1; i++)
            {
                var s = Children[i].GetState(state.Updater);
                if (!s.Down)
                {
                    return false;
                }
                shortestDownTime = s.DownTime;
            }

            var lastState = Children[^1];
            return lastState.GetState(state.Updater).Down && lastState.GetState(state.Updater).DownTime < shortestDownTime;
        }

        public override IEnumerable<Input> GetChildren()
        {
            return Children;
        }

        public override string ToString()
        {
            return string.Join(" + ", (object?[])Children);
        }
    }
}
