namespace BytingLib
{
    public class TransparencyBatch
    {
        private readonly SortedList<float, Action> drawLater = new SortedList<float, Action>(new DuplicateKeyComparerDescending<float>());
        private readonly Func<Vector3> getViewPos;
        private readonly float fixZOffset;
        private readonly float variableZOffset;
        private bool listeningForDrawCalls = false;

        public TransparencyBatch(Func<Vector3> getViewPos, float fixZOffset = 0.001f, float variableZOffset = 0.003f)
        {
            this.getViewPos = getViewPos;
            this.fixZOffset = fixZOffset;
            this.variableZOffset = variableZOffset;
        }

        public void Begin()
        {
            if (listeningForDrawCalls || drawLater.Count > 0)
            {
                throw new Exception("TransparencyBatch.End() must be called after TransparencyBatch.Begin()");
            }

            listeningForDrawCalls = true;

        }

        public void End()
        {
            listeningForDrawCalls = false;

            foreach (var draw in drawLater)
            {
                draw.Value();
            }

            drawLater.Clear();
        }

        public void DrawLater(Vector3 centerPosition, Action draw)
        {
            float depthSquared = GetDepthSquared(centerPosition);
            drawLater.Add(depthSquared, draw);
        }

        private float GetDepthSquared(Vector3 pos)
        {
            return (getViewPos() - pos).LengthSquared();
        }

        public Vector3 GetOffsetAgainstZFighting(Vector3 pos, Vector3 forwardNormalized)
        {
            //float depth = Vector3.Dot(GetCamPos() - pos, forwardNormalized) * 0.003f + 0.01f;
            float depth = (getViewPos() - pos).Length() * variableZOffset + fixZOffset; // TODO: improve performance: don't use length()
            return depth * forwardNormalized;
        }
    }
}