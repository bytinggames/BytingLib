namespace BytingLib
{
    public class BoolWindowActive : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        protected override bool CalculateValue(FullInput input)
        {
            return input.MetaState.IsActivatedThisUpdate;
        }
    }
}
