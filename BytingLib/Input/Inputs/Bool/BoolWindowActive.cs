namespace BytingLib
{
    public class BoolWindowActive : InputBoolSimple
    {
        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return input.MetaState.IsActivatedThisUpdate;
        }
    }
}
