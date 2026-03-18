namespace BytingLib
{
    /// <summary>
    /// Only sets Down and Pressed to true, if Released of Child is true.
    /// </summary>
    public class BoolOnRelease : InputBoolSimple
    {
        public InputBool Child { get; }

        public BoolOnRelease(InputBool child)
        {
            Child = child;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return Child.GetState(state.Updater).Released;
        }

        public override string ToString()
        {
            return $"BoolOnReleased( {Child} )";
        }
    }
}
