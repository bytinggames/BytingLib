using System.Text.Json.Nodes;

namespace BytingLib
{
    public class SkinGL
    {
        private static int globalTransformCalculationId;

        public Matrix[] InverseBindMatrices { get; }
        public NodeGL[] Joints { get; }
        public string? Name { get; set; }

        private Matrix[] jointMatrices;

        public SkinGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();


            int inverseBindMatricesId = n["inverseBindMatrices"]!.GetValue<int>();
            int[] joints = n["joints"]!.AsArray().Select(f => f!.GetValue<int>()).ToArray();


            Joints = new NodeGL[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                Joints[i] = model.Nodes!.Get(joints[i], null);
                Joints[i].IsBone = true;
            }

            byte[] inverseBindAccessorData = model.GetBytesFromBuffer(inverseBindMatricesId);
            InverseBindMatrices = BufferHelper.ByteArrayToMatrixArray(inverseBindAccessorData);


            jointMatrices = new Matrix[Joints.Length * 2];
        }

        public override string ToString() => "Skin: " + Name;

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

        internal IDisposable? Use(IShaderSkin shader, Matrix globalTransform)
        {
            ComputeJointMatrices(globalTransform);
            return shader.JointMatrices.Use(jointMatrices);
        }
    }
}
