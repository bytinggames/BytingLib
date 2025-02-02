using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolOutput : BoolInput, IPointerValue, IInputOutput
    {
        public BoolInput Value { get; private set; }

        public BoolOutput(BoolInput Value)
        {
            this.Value = Value;
        }

        public object GetPointerValue() => Value;

        public Type GetDeclaredPointerValueType() => typeof(BoolInput);

        protected override bool CalculateValue(FullInput input)
        {
            return Value.Down;
        }

        public void SetPointerValue(object obj)
        {
            if (obj is BoolInput delta)
            {
                Value.Dispose();
                Value = delta;
                Value.Initialize(updater);
            }
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return "->" + Value;
        }

        public void Disable()
        {
            SetPointerValue(new BoolConst(false));
        }

        public static implicit operator BoolOutput(Keys key) => new BoolOutput(new BoolKey(key));
        public static implicit operator BoolOutput(MouseButton mouseButton) => new BoolOutput(new BoolMouse(mouseButton));
        public static implicit operator BoolOutput(Buttons gamePadButton) => new BoolOutput(new BoolGamePad(gamePadButton));
    }
}
