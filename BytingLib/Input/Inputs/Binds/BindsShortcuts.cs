namespace BytingLib
{
    public class BindsShortcuts
    {
        protected static InputBool Ctrl() => new BoolCtrl();
        protected static InputBool Shift() => new BoolShift();
        protected static InputBool Alt() => new BoolAlt();
        protected static InputBool And(params InputBool[] inputs) => new BoolAnd(inputs);
        protected static InputBool Or(params InputBool[] inputs) => new BoolOr(inputs);
        protected static InputBool Not(InputBool input) => new BoolNot(input);
    }
}
