namespace BytingLib
{
    public abstract class InputVector4Simple : InputVector4<InputVector4State>
    {
        public override InputVector4State CreateState(InputUpdater updater) => new InputVector4State(updater);

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

}
