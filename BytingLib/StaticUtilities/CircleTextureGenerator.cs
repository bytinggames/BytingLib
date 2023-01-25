namespace BytingLib
{
    /// <summary>
    /// This can probably be replaced by a rendering method that renders a circle polygon onto a texture.
    /// </summary>
    static class CircleTextureGenerator
    {
        public static Texture2D GenerateDisk(GraphicsDevice gDevice, float radius)
        {
            int size = (int)Math.Ceiling(radius * 2f);
            Texture2D tex = new Texture2D(gDevice, size, size);
            Color[] colors = new Color[size * size];

            float radiusSquared = radius * radius;

            // fill texture with "circle pixels"
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 distance = new Vector2(radius, radius) - new Vector2(x + 0.5f, y + 0.5f);
                    float distLengthSquared = distance.LengthSquared();

                    if (distLengthSquared <= radiusSquared)
                    {
                        colors[y * size + x] = Color.White;
                    }
                }
            }

            tex.SetData(colors);
            return tex;
        }

        public static Texture2D GenerateRing(GraphicsDevice gDevice, float radius, float thickness)
        {
            if (thickness >= radius)
            {
                return GenerateDisk(gDevice, radius);
            }

            int size = (int)Math.Ceiling(radius * 2f);
            Texture2D tex = new Texture2D(gDevice, size, size);
            Color[] colors = new Color[size * size];

            float radiusSquared = radius * radius;
            float innerRadius = radius - thickness;
            float innerRadiusSquared = innerRadius * innerRadius;

            // fill texture with "circle pixels"
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 distance = new Vector2(radius, radius) - new Vector2(x + 0.5f, y + 0.5f);
                    float distLengthSquared = distance.LengthSquared();

                    if (distLengthSquared <= radiusSquared && distLengthSquared >= innerRadiusSquared)
                    {
                        colors[y * size + x] = Color.White;
                    }
                }
            }

            tex.SetData(colors);
            return tex;
        }
        public static Texture2D GenerateRingSmooth(GraphicsDevice gDevice, float radius, float thickness, float smoothThickness = 1f)
        {
            if (smoothThickness <= 0f)
            {
                return GenerateRing(gDevice, radius, thickness);
            }
            if (thickness >= radius)
            {
                thickness = radius;
            }


            int size = (int)Math.Ceiling(radius * 2f);
            Texture2D tex = new Texture2D(gDevice, size, size);
            Color[] colors = new Color[size * size];

            float innerRadius = radius - thickness;
            float innerRadiusSmooth = innerRadius + smoothThickness;
            float outerRadiusSmooth = radius - smoothThickness;

            // fill texture with "circle pixels"
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 distance = new Vector2(radius, radius) - new Vector2(x + 0.5f, y + 0.5f);
                    float distLength = distance.Length();

                    if (distLength <= radius && distLength >= innerRadius)
                    {
                        int index = y * size + x;
                        colors[index] = Color.White;

                        if (distLength < innerRadiusSmooth)
                        {
                            float a = (distLength - innerRadius) / (innerRadiusSmooth - innerRadius);
                            colors[index] *= a;
                        }
                        else if (distLength > outerRadiusSmooth)
                        {
                            float a = (distLength - radius) / (outerRadiusSmooth - radius);
                            colors[index] *= a;
                        }
                    }
                }
            }

            tex.SetData(colors);
            return tex;
        }
    }
}
