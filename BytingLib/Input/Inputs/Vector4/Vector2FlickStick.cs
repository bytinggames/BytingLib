namespace BytingLib
{
    public enum FlickStickType
    {
        RelativeOrientation,
        OrientationForward,
        OrientationForwardInvertBackwards
    }

    public class Vector2FlickStick(InputVector2 child) : InputVector2<Vector2FlickStickState>
    {
        public FlickStickType Type { get; set; } = FlickStickType.OrientationForwardInvertBackwards;

        public InputVector2 Child { get; } = child;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override Vector2 CalculateValue(FullInput input, Vector2FlickStickState state)
        {
            var childState = Child.GetState(state.Updater);
            bool isActive = IsActive(childState.Value);
            bool wasActive = IsActive(childState.LastValue);

            if (isActive)
            {
                float lastAngle;
                if (wasActive)
                {
                    lastAngle = MathF.Atan2(childState.LastValue.Y, childState.LastValue.X);
                }
                else
                {
                    if (Type == FlickStickType.RelativeOrientation)
                    {
                        // use current rotation to have no rotation diff
                        lastAngle = MathF.Atan2(childState.Y, childState.X);
                    }
                    else
                    {
                        // use rotation for when moving thumbstick up for rotation diff
                        lastAngle = -MathHelper.PiOver2;
                    }
                }
                float angle = MathF.Atan2(childState.Y, childState.X);
                float angleDiff = lastAngle.AngleDistance(angle);

                float angleDiffReturn = -angleDiff;

                if (Type == FlickStickType.OrientationForwardInvertBackwards)
                {
                    if (!wasActive)
                    {
                        state.Invert = MathF.Abs(angleDiff) > MathHelper.PiOver2;
                    }
                    else if (state.Invert)
                    {
                        angleDiffReturn = -angleDiffReturn;
                    }
                }

                return new Vector2(angleDiffReturn, 0f);
            }

            return Vector2.Zero;

            bool IsActive(Vector2 v)
            {
                return v.LengthSquared() > 0.8f * 0.8f;
            }
        }

        public override string ToString()
        {
            return "Flick Stick " + Child;
        }

        public override Vector2FlickStickState CreateState(InputUpdater updater)
        {
            return new Vector2FlickStickState(updater);
        }
    }

    public class Vector2FlickStickState: InputVector2State
    {
        public bool Invert { get; set; }

        public Vector2FlickStickState(InputUpdater updater)
            : base(updater)
        {
        }

    }
}
