namespace BytingLib
{
    /// <summary>
    /// Only sets Down and Pressed to true, if Pressed of Child is true.
    /// </summary>
    public class BoolOnPress : InputBoolSimple
    {
        public InputBool Child { get; }

        public BoolOnPress(InputBool child)
        {
            Child = child;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return Child.GetState(state.Updater).Pressed;
        }

        public override string ToString()
        {
            return $"BoolOnPress( {Child} )";
        }
    }
}
