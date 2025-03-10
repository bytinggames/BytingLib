namespace BytingLib
{
    public class InputCanvasTransformed(IInputCanvas sourceInput, Func<Matrix> getUITransform) : IInputCanvas
    {
        public InputVector2State MousePosition => Transform(sourceInput.MousePosition, getUITransform);
        public InputBoolState Click => sourceInput.Click;
        public InputIntState Scroll => sourceInput.Scroll;
        public InputUpdater Updater => sourceInput.Updater;
        public InputVector2State Navigate => sourceInput.Navigate;
        public InputVector2State NavigateWithLetters => sourceInput.NavigateWithLetters;
        public InputBoolState Enter => sourceInput.Enter;

        private static InputVector2State Transform(InputVector2State state, Func<Matrix> getUITransform)
        {
            Matrix transform = getUITransform();
            InputVector2State stateOutput = new(state.Updater);
            stateOutput.Update(Vector2.Transform(state.LastValue, transform));
            stateOutput.Update(Vector2.Transform(state.Value, transform));
            return stateOutput;
        }

        public void SetMousePosition(Vector2 position)
        {
            position = Vector2.Transform(position, Matrix.Invert(getUITransform()));
            sourceInput.SetMousePosition(position);
        }
    }
}
