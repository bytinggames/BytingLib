namespace BytingLib
{
    public class BoolValueFunc(Func<bool> getBool) : InputBoolSimple
    {
        private readonly Func<bool> getBool = getBool;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return getBool();
        }
    }
}

