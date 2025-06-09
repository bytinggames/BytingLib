using System.Text.Json.Serialization;

namespace BytingLib
{
    // this class is used to generate all the other Input... classes (InputInt, InputFloat etc.)
    // No, it was not possible to make them generic, as we have implicit operators and because of the State : InputFloatState

    [JsonDerivedType(typeof(FloatSwitch), "FloatSwitch")]
    [JsonDerivedType(typeof(FloatConst), "FloatConst")]
    public abstract partial class InputFloat : Input
    {
        public abstract InputFloatState GetState(InputUpdater updater);
    }

    public abstract class InputFloat<State> : InputFloat where State : InputFloatState
    {
        private readonly Dictionary<InputUpdater, State> states = new();

        protected abstract float CalculateValue(FullInput input, State state);

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

    public abstract class InputFloatSimple : InputFloat<InputFloatState>
    {
        public override InputFloatState CreateState(InputUpdater updater) => new InputFloatState(updater);

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

    public class FloatFunc(Func<InputFloat> getFloat) : InputFloatSimple
    {
        private readonly Func<InputFloat> getFloat = getFloat;

        protected override float CalculateValue(FullInput input, InputFloatState state)
        {
            return getFloat().GetState(state.Updater);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return getFloat();
        }
    }

    public class FloatFuncValue(Func<float> getValue) : InputFloatSimple
    {
        private readonly Func<float> getValue = getValue;

        protected override float CalculateValue(FullInput input, InputFloatState state)
        {
            return getValue();
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

    public class FloatConst(float value) : InputFloatSimple
    {
        public float Value { get; } = value;

        protected override float CalculateValue(FullInput input, InputFloatState state)
        {
            return Value;
        }
    }

    public partial class FloatSwitch(InputBool condition, InputFloat onTrue, InputFloat onFalse) : InputFloatSimple
    {
        public InputBool Condition { get; } = condition;
        public InputFloat OnTrue { get; } = onTrue;
        public InputFloat OnFalse { get; } = onFalse;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Condition;
            yield return OnTrue;
            yield return OnFalse;
        }

        protected override float CalculateValue(FullInput input, InputFloatState state)
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
