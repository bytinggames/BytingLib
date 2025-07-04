using System.Text.Json.Serialization;

namespace BytingLib
{
    [JsonDerivedType(typeof(Vector4Relative), "Vector4Relative")]
    [JsonDerivedType(typeof(Vector4Absolute), "Vector4Absolute")]
    [JsonDerivedType(typeof(Vector4MaxLength), "Vector4MaxLength")]
    public abstract partial class InputVector4 : Input
    {
        public abstract InputVector4State GetState(InputUpdater updater);
    }

    public abstract class InputVector4<State> : InputVector4 where State : InputVector4State
    {
        private readonly Dictionary<InputUpdater, State> states = new();

        protected abstract Vector4 CalculateValue(FullInput input, State state);

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

    public class InputVector4State : InputState<Vector4>
    {
        public InputVector4State(InputUpdater updater)
            : base(updater)
        {
        }

        public Vector4 LastValue { get; private set; }
        public Vector4 Value { get; private set; }
        public Vector4 Delta => Updater.CurrentStamp >= 2 ? Value - LastValue : Vector4.Zero;
        public float X => Value.X;
        public float Y => Value.Y;
        public float Z => Value.Z;
        public float W => Value.W;

        public override void Update(Vector4 value)
        {
            LastValue = Value;
            Value = value;
        }

        public static implicit operator Vector4(InputVector4State f) => f.Value;
    }
}
