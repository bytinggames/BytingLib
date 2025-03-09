namespace BytingLib
{
    public class HoldToRepeat
    {
        public int HoldUpdatesToTriggerPress { get; set; } = 15;
        public int HoldToPressInterval { get; set; } = 3;

        internal bool HoldIsRepeat(long currentStamp, long pressedStamp)
        {
            long heldForPress = currentStamp - pressedStamp! - HoldUpdatesToTriggerPress;
            if (heldForPress >= 0)
            {
                heldForPress = heldForPress % HoldToPressInterval;
                if (heldForPress == 0)
                {
                    // repeated press
                    return true;
                }
            }
            return false;
        }
    }
}
