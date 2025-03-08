using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BytingLib
{
    public abstract class InputBinds
    {
        protected static InputBool Ctrl() => new BoolCtrl();
        protected static InputBool Shift() => new BoolShift();
        protected static InputBool Alt() => new BoolAlt();
        protected static InputBool And(params InputBool[] inputs) => new BoolAnd(inputs);
        protected static InputBool Or(params InputBool[] inputs) => new BoolOr(inputs);
        protected static InputBool Not(InputBool input) => new BoolNot(input);
        protected static InputBool Func(Func<InputBool> func) => new BoolFunc(func);
        protected static Vector2FromBools ArrowsOrWasdOrDpad =>
            new Vector2FromBools(
                Or(Keys.Up, Keys.W, Buttons.DPadUp), 
                Or(Keys.Left, Keys.A, Buttons.DPadLeft), 
                Or(Keys.Down, Keys.S, Buttons.DPadDown), 
                Or(Keys.Right, Keys.D, Buttons.DPadRight));

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
