namespace BytingLib
{
    public class JointTransform
    {
        public Quaternion RotationDefault { get; }
        public Vector3 TranslationDefault { get; }
        public Vector3 ScaleDefault { get; }
        public bool Dirty { get; set; }

        private Quaternion rotation;
        private Vector3 translation;
        private Vector3 scale;

        public JointTransform(Matrix defaultTransform)
        {
            defaultTransform.Decompose(out var s, out var r, out var t);
            ScaleDefault = s;
            RotationDefault = r;
            TranslationDefault = t;

            rotation = RotationDefault;
            scale = ScaleDefault;
            translation = TranslationDefault;
        }

        public Quaternion Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                Dirty = true;
            }
        }
        public Vector3 Translation
        {
            get => translation;
            set
            {
                translation = value;
                Dirty = true;
            }
        }
        public Vector3 Scale
        {
            get => scale;
            set
            {
                scale = value;
                Dirty = true;
            }
        }

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
