namespace BytingLib
{
    public class BoolWindowActive : BoolInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override bool CalculateValue(FullInput input)
        {
            return input.MetaState.IsActivatedThisUpdate;
        }
    }
}
