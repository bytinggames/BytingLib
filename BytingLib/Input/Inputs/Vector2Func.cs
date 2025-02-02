namespace BytingLib
{
    public class Vector2Func : Vector2Input
    {
        private readonly Func<Vector2>? getValue;
        private readonly Func<Vector2Input, Vector2>? getValueFromChild;
        private readonly Vector2Input? child;

        public Vector2Func(Func<Vector2> getValue)
        {
            this.getValue = getValue;
        }

        public Vector2Func(Func<Vector2Input, Vector2> getValue, Vector2Input child)
        {
            this.getValueFromChild = getValue;
            this.child = child;
        }


        public override IEnumerable<InputUpdate> GetChildren()
        {
            if (child != null)
            {
                yield return child;
            }
        }

        protected override Vector2 CalculateValue(FullInput input)
        {
            if (child != null && getValueFromChild != null)
            {
                return getValueFromChild(child);
            }
            else if (getValue != null)
            {
                return getValue();
            }
            throw new Exception("everything null in Vector2Fun");
        }

        public override string ToString()
        {
            return " Func " + (child == null ? "" : child);
        }
    }
}
