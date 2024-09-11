namespace BytingLib
{
    public class TransparencyBatchSwapper(
        Func<Vector3> getViewPos, 
        Func<IDisposable>[] useShading, 
        float fixZOffset = 0.001f, 
        float variableZOffset = 0.003f)
    {
        private readonly SortedList<float, Item> drawCalls = new(new DuplicateKeyComparerDescending<float>());
        private readonly Func<Vector3> getViewPos = getViewPos;
        private readonly Func<IDisposable>[] useShading = useShading;
        private readonly float fixZOffset = fixZOffset;
        private readonly float variableZOffset = variableZOffset;
        private bool begun = false;

        public void Begin()
        {
            if (begun)
            {
                throw new Exception("Forgot to call End() after the first Begin()");
            }

            begun = true;
        }

        public void End()
        {
            if (!begun)
            {
                throw new Exception("Call Begin() before End()");
            }

            int techniqueIndex = -1;
            IDisposable? currentShading = null;

            foreach (var draw in drawCalls)
            {
                // if baked toggles, toggle shading behaviour
                if (techniqueIndex != draw.Value.TechniqueIndex)
                {
                    currentShading?.Dispose();
                    currentShading = null;

                    techniqueIndex = draw.Value.TechniqueIndex;
                    currentShading = useShading[techniqueIndex]();
                }
                draw.Value.DrawAction();
            }

            currentShading?.Dispose();

            drawCalls.Clear();

            begun = false;
        }

        public void DrawLater(Vector3 centerPosition, int techniqueIndex, Action draw)
        {
            float depthSquared = GetDepthSquared(centerPosition);
            DrawLater(depthSquared, techniqueIndex, draw);
        }

        public void DrawLater(float depthSquared, int techniqueIndex, Action draw)
        {
            if (!begun)
            {
                return; // skip transparency draw call
            }
            if (techniqueIndex >= useShading.Length)
            {
                throw new Exception($"techniqueIndex is not in range of useShading.Length: {techniqueIndex} >= {useShading.Length}");
            }

            drawCalls.Add(depthSquared, new(draw, techniqueIndex));
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

        struct Item
        {
            public Action DrawAction { get; set; }
            public int TechniqueIndex { get; set; }

            public Item(Action drawAction, int techniqueIndex)
            {
                DrawAction = drawAction;
                TechniqueIndex = techniqueIndex;
            }
        }
    }
}