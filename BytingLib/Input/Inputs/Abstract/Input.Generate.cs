
using System.Text.Json.Serialization;

namespace BytingLib
{
    // this class is used to generate all the other Input... classes (InputInt, InputBool etc.)
    // No, it was not possible to make them generic, as we have implicit operators and because of the State : InputBoolState

    [JsonDerivedType(typeof(BoolSwitch), "BoolSwitch")]
    [JsonDerivedType(typeof(BoolConst), "BoolConst")]
    public abstract partial class InputBool : Input
    {
        public abstract InputBoolState GetState(InputUpdater updater);
    }

    public abstract class InputBool<State> : InputBool where State : InputBoolState
    {
        private readonly Dictionary<InputUpdater, State> states = new();

        protected abstract bool CalculateValue(FullInput input, State state);

        protected override void UpdateSelf(InputUpdater updater, FullInput input)
        {
            State state = states[updater];
            state.Update(CalculateValue(input, state));
        }

        public abstract State CreateState(InputUpdater updater);

        public override State GetState(InputUpdater updater)
        {
            return states[updater];
        }

        public override bool IsRegistered(InputUpdater updater)
        {
            return states.ContainsKey(updater);
        }

        protected override void RegisterSelf(InputUpdater updater)
        {
            states.TryAdd(updater, CreateState(updater));
        }

        public override List<InputUpdater> GetUpdaters()
        {
            return states.Keys.ToList();
        }

        protected override void UnregisterSelf(List<InputUpdater> updaters)
        {
            for (int i = 0; i < updaters.Count; i++)
            {
                states.Remove(updaters[i]);
            }
        }
    }

    public abstract class InputBoolSimple : InputBool<InputBoolState>
    {
        public override InputBoolState CreateState(InputUpdater updater) => new InputBoolState(updater);

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

    public class BoolFunc(Func<InputBool> getBool) : InputBoolSimple
    {
        private readonly Func<InputBool> getBool = getBool;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return getBool().GetState(state.Updater);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return getBool();
        }
    }

    public class BoolConst(bool value) : InputBoolSimple
    {
        public bool Value { get; } = value;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return Value;
        }
    }

    public partial class BoolSwitch(InputBool condition, InputBool onTrue, InputBool onFalse) : InputBoolSimple
    {
        public InputBool Condition { get; } = condition;
        public InputBool OnTrue { get; } = onTrue;
        public InputBool OnFalse { get; } = onFalse;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Condition;
            yield return OnTrue;
            yield return OnFalse;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            if (Condition.GetState(state.Updater))
            {
                return OnTrue.GetState(state.Updater);
            }
            else
            {
                return OnFalse.GetState(state.Updater);
            }
        }
    }

    // this class is used to generate all the other Input... classes (InputInt, InputInt etc.)
    // No, it was not possible to make them generic, as we have implicit operators and because of the State : InputIntState

    [JsonDerivedType(typeof(IntSwitch), "IntSwitch")]
    [JsonDerivedType(typeof(IntConst), "IntConst")]
    public abstract partial class InputInt : Input
    {
        public abstract InputIntState GetState(InputUpdater updater);
    }

    public abstract class InputInt<State> : InputInt where State : InputIntState
    {
        private readonly Dictionary<InputUpdater, State> states = new();

        protected abstract int CalculateValue(FullInput input, State state);

        protected override void UpdateSelf(InputUpdater updater, FullInput input)
        {
            State state = states[updater];
            state.Update(CalculateValue(input, state));
        }

        public abstract State CreateState(InputUpdater updater);

        public override State GetState(InputUpdater updater)
        {
            return states[updater];
        }

        public override bool IsRegistered(InputUpdater updater)
        {
            return states.ContainsKey(updater);
        }

        protected override void RegisterSelf(InputUpdater updater)
        {
            states.TryAdd(updater, CreateState(updater));
        }

        public override List<InputUpdater> GetUpdaters()
        {
            return states.Keys.ToList();
        }

        protected override void UnregisterSelf(List<InputUpdater> updaters)
        {
            for (int i = 0; i < updaters.Count; i++)
            {
                states.Remove(updaters[i]);
            }
        }
    }

    public abstract class InputIntSimple : InputInt<InputIntState>
    {
        public override InputIntState CreateState(InputUpdater updater) => new InputIntState(updater);

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

    public class IntFunc(Func<InputInt> getInt) : InputIntSimple
    {
        private readonly Func<InputInt> getInt = getInt;

        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            return getInt().GetState(state.Updater);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return getInt();
        }
    }

    public class IntConst(int value) : InputIntSimple
    {
        public int Value { get; } = value;

        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            return Value;
        }
    }

    public partial class IntSwitch(InputBool condition, InputInt onTrue, InputInt onFalse) : InputIntSimple
    {
        public InputBool Condition { get; } = condition;
        public InputInt OnTrue { get; } = onTrue;
        public InputInt OnFalse { get; } = onFalse;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Condition;
            yield return OnTrue;
            yield return OnFalse;
        }

        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            if (Condition.GetState(state.Updater))
            {
                return OnTrue.GetState(state.Updater);
            }
            else
            {
                return OnFalse.GetState(state.Updater);
            }
        }
    }

    // this class is used to generate all the other Input... classes (InputInt, InputVector2 etc.)
    // No, it was not possible to make them generic, as we have implicit operators and because of the State : InputVector2State

    [JsonDerivedType(typeof(Vector2Switch), "Vector2Switch")]
    [JsonDerivedType(typeof(Vector2Const), "Vector2Const")]
    public abstract partial class InputVector2 : Input
    {
        public abstract InputVector2State GetState(InputUpdater updater);
    }

    public abstract class InputVector2<State> : InputVector2 where State : InputVector2State
    {
        private readonly Dictionary<InputUpdater, State> states = new();

        protected abstract Vector2 CalculateValue(FullInput input, State state);

        protected override void UpdateSelf(InputUpdater updater, FullInput input)
        {
            State state = states[updater];
            state.Update(CalculateValue(input, state));
        }

        public abstract State CreateState(InputUpdater updater);

        public override State GetState(InputUpdater updater)
        {
            return states[updater];
        }

        public override bool IsRegistered(InputUpdater updater)
        {
            return states.ContainsKey(updater);
        }

        protected override void RegisterSelf(InputUpdater updater)
        {
            states.TryAdd(updater, CreateState(updater));
        }

        public override List<InputUpdater> GetUpdaters()
        {
            return states.Keys.ToList();
        }

        protected override void UnregisterSelf(List<InputUpdater> updaters)
        {
            for (int i = 0; i < updaters.Count; i++)
            {
                states.Remove(updaters[i]);
            }
        }
    }

    public abstract class InputVector2Simple : InputVector2<InputVector2State>
    {
        public override InputVector2State CreateState(InputUpdater updater) => new InputVector2State(updater);

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

    public class Vector2Func(Func<InputVector2> getVector2) : InputVector2Simple
    {
        private readonly Func<InputVector2> getVector2 = getVector2;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return getVector2().GetState(state.Updater);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return getVector2();
        }
    }

    public class Vector2Const(Vector2 value) : InputVector2Simple
    {
        public Vector2 Value { get; } = value;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return Value;
        }
    }

    public partial class Vector2Switch(InputBool condition, InputVector2 onTrue, InputVector2 onFalse) : InputVector2Simple
    {
        public InputBool Condition { get; } = condition;
        public InputVector2 OnTrue { get; } = onTrue;
        public InputVector2 OnFalse { get; } = onFalse;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Condition;
            yield return OnTrue;
            yield return OnFalse;
        }

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            if (Condition.GetState(state.Updater))
            {
                return OnTrue.GetState(state.Updater);
            }
            else
            {
                return OnFalse.GetState(state.Updater);
            }
        }
    }
}

