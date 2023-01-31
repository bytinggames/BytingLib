namespace BytingLib
{
    public class JointTransform
    {
        public readonly Quaternion RotationDefault;
        public readonly Vector3 TranslationDefault;
        public readonly Vector3 ScaleDefault;

        private Quaternion rotation;
        private Vector3 translation;
        private Vector3 scale;

        public JointTransform(Matrix defaultTransform)
        {
            defaultTransform.Decompose(out ScaleDefault, out RotationDefault, out TranslationDefault);
            rotation = RotationDefault;
            scale = ScaleDefault;
            translation = TranslationDefault;
        }

        public bool Dirty { get; set; }

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
