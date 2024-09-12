namespace BytingLib
{
    public class TransparencyBatch
    {
        private readonly List<SortedList<float, Action>> drawLayers = new();
        private readonly Func<Vector3> getViewPos;
        private readonly float fixZOffset;
        private readonly float variableZOffset;
        public int MaxBatchCount { get; set; }

        public TransparencyBatch(Func<Vector3> getViewPos, float fixZOffset = 0.001f, float variableZOffset = 0.003f)
        {
            this.getViewPos = getViewPos;
            this.fixZOffset = fixZOffset;
            this.variableZOffset = variableZOffset;
        }

        public void Begin()
        {
            if (drawLayers.Count >= MaxBatchCount)
            {
                throw new Exception("maximum batch count reached. Either call End() or increase maxBatchCount. maxBatchCount is " + MaxBatchCount);
            }

            drawLayers.Add(new(new DuplicateKeyComparerDescending<float>())); // add a new draw layer
        }

        public void End(int index = 0)
        {
            if (drawLayers.Count == 0)
            {
                throw new Exception("TransparencyBatch.End() must be called after TransparencyBatch.Begin()");
            }

            if (index >= drawLayers.Count)
            {
                throw new Exception($"TransparencyBatch.End() index {index} is out of range of drawLayers.Count {drawLayers.Count}");
            }

            foreach (var draw in drawLayers[index])
            {
                draw.Value();
            }

            drawLayers.RemoveAt(index);
        }

        public void EndLastLayer()
        {
            End(drawLayers.Count - 1);
        }

        public void DrawLater(Vector3 centerPosition, Action draw)
        {
            float depthSquared = GetDepthSquared(centerPosition);
            DrawLater(depthSquared, draw);
        }

        public void DrawLater(float depthSquared, Action draw)
        {
            if (drawLayers.Count == 0)
            {
                return; // skip transparency draw call
            }

            drawLayers[^1].Add(depthSquared, draw);
        }

        public float GetDepthSquared(Vector3 pos)
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