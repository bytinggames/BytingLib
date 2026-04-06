using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public abstract class InputBinds
    {
        public static InputBool Ctrl() => new BoolCtrl();
        public static InputBool Shift() => new BoolShift();
        public static InputBool Alt() => new BoolAlt();
        public static InputBool And(params InputBool[] inputs) => new BoolAnd(inputs);
        public static InputBool Or(params InputBool[] inputs) => new BoolOr(inputs);
        public static InputBool Not(InputBool input) => new BoolNot(input);
        public static InputBool Twice(InputBool input) => new BoolSequence(input, input);
        public static InputBool Func(Func<InputBool> func) => new BoolFunc(func);
        public static InputVector2 Wasd => new Vector2FromBools(Keys.W, Keys.A, Keys.S, Keys.D);
        public static InputVector2 Arrows => new Vector2FromBools(Keys.Up, Keys.Left, Keys.Down, Keys.Right);
        public static InputVector2 DPad => new Vector2FromBools(Buttons.DPadUp, Buttons.DPadLeft, Buttons.DPadDown, Buttons.DPadRight);
        public static Vector2FromBools ArrowsOrWasdOrDpad =>
            new Vector2FromBools(
                Or(Keys.Up, Keys.W, Buttons.DPadUp), 
                Or(Keys.Left, Keys.A, Buttons.DPadLeft), 
                Or(Keys.Down, Keys.S, Buttons.DPadDown), 
                Or(Keys.Right, Keys.D, Buttons.DPadRight));
        public static Vector2FromBools ArrowsOrDpad =>
            new Vector2FromBools(
                Or(Keys.Up, Buttons.DPadUp),
                Or(Keys.Left, Buttons.DPadLeft),
                Or(Keys.Down, Buttons.DPadDown),
                Or(Keys.Right, Buttons.DPadRight));
        public static InputBool False => InputBool.False;
        public static InputBool True => InputBool.True;

        protected static InputBool Modify(InputBool input, bool ctrl = false, bool shift = false, bool alt = false)
        {
            var c = Ctrl();
            var s = Shift();
            var a = Alt();
            if (!ctrl)
            {
                c = Not(c);
            }
            if (!shift)
            {
                s = Not(s);
            }
            if (!alt)
            {
                a = Not(a);
            }

            return And(c, s, a, input);
        }

        public IEnumerable<PropertyInfo> GetRemappableProperties()
        {
            var props = GetType().GetProperties(
                BindingFlags.SetProperty
                | BindingFlags.GetProperty
                | BindingFlags.Public
                | BindingFlags.Instance
            ).Where(f => f.CanWrite && f.CanRead && f.GetCustomAttribute<JsonIgnoreAttribute>() == null);

            return props;
        }

        protected virtual void SetControllerBinds()
        {
        }

        public void SetDefault()
        {
            var binds = Activator.CreateInstance(GetType()) as InputBinds;
            if (binds == null)
            {
                return;
            }
            BindsSerializer serializer = new();

            // reset to default
            serializer.Deserialize("{}", this);
        }

        /// <summary>Experimental. Only used for debugging</summary>
        public void SetDefaultAndControllerBinds()
        {
            var binds = Activator.CreateInstance(GetType()) as InputBinds;
            if (binds == null)
            {
                return;
            }
            binds.SetControllerBinds();
            BindsSerializer serializer = new();
            string controllerJson = serializer.Serialize(binds);

            // reset to default
            serializer.Deserialize("{}", this);
            // add controller
            serializer.Deserialize(controllerJson, this, false);
        }

        public void OverrideWithDefaultControllerBinds()
        {
            var binds = Activator.CreateInstance(GetType()) as InputBinds;
            if (binds == null)
            {
                return;
            }
            binds.SetControllerBinds();
            BindsSerializer serializer = new();
            string controllerJson = serializer.Serialize(binds);

            serializer.Deserialize(controllerJson, this);
        }
    }
}
