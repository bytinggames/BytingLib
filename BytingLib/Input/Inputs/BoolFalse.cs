namespace BytingLib
{
    public class BoolFalse : IBoolDelta
    {
        public bool Down => false;
        public bool Pressed => false;
        public int DownTime => 0;
        public bool Released => false;
        public int ReleasedTime => 0;
    }
}
