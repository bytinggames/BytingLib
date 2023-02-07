using System.Text.Json.Nodes;

namespace BytingLib
{
    public class SkinGL
    {
        static int globalTransformCalculationId;

        public readonly Matrix[] InverseBindMatrices;
        public readonly NodeGL[] Joints;

        private Matrix[] jointMatrices;

        public readonly string? Name;
        public override string ToString() => "Skin: " + Name;

        public SkinGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();


            int inverseBindMatricesId = n["inverseBindMatrices"]!.GetValue<int>();
            int[] joints = n["joints"]!.AsArray().Select(f => f!.GetValue<int>()).ToArray();


            Joints = new NodeGL[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                Joints[i] = model.Nodes!.Get(joints[i], null);
            }

            byte[] inverseBindAccessorData = model.GetBytesFromBuffer(inverseBindMatricesId);
            InverseBindMatrices = BufferHelper.ByteArrayToMatrixArray(inverseBindAccessorData);


            jointMatrices = new Matrix[Joints.Length * 2];
        }

        public void ComputeJointMatrices(Matrix globalTransform)
        {
            globalTransformCalculationId++;
            for (int i = 0; i < Joints.Length; i++)
            {
                Joints[i].CalculateGlobalTransform(globalTransformCalculationId);
            }

            Matrix globalTransformInverse = Matrix.Invert(globalTransform);
            for (int i = 0; i < Joints.Length; i++)
            {
                jointMatrices[i] = InverseBindMatrices[i]
                    * Joints[i].GlobalJointTransform
                    * globalTransformInverse;

                // inverse transpose matrices
                jointMatrices[i + Joints.Length] = jointMatrices[i].GetInverseTranspose3x3();
            }
        }

        internal IDisposable? Use(IShaderSkinned shader, Matrix globalTransform)
        {
            ComputeJointMatrices(globalTransform);
            return shader.JointMatrices.Use(jointMatrices);
        }
    }
}
