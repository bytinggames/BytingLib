namespace BytingLib
{
    public class JointTransform
    {
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Translation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public bool Dirty = true;

        public Matrix GetTransform()
        {
            Matrix m = Matrix.CreateFromQuaternion(Rotation);
            m.M11 *= Scale.X;
            m.M22 *= Scale.Y;
            m.M33 *= Scale.Z;
            m.Translation = Translation;
            return m;
        }
    }
}
