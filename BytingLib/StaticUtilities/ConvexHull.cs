public static class ConvexHull
{
    public static List<Vector2> BuildConvexHull(IList<Vector2> sourcePoints)
    {
        if (sourcePoints == null || sourcePoints.Count < 3)
        {
            return new List<Vector2>();
        }

        // Copy to a modifiable list
        var points = new List<Vector2>(sourcePoints);

        // Sort points by X, then by Y without LINQ
        points.Sort((a, b) =>
        {
            int compareX = a.X.CompareTo(b.X);
            return compareX != 0 ? compareX : a.Y.CompareTo(b.Y);
        });

        var hull = new List<Vector2>();

        // Build lower part
        foreach (var pt in points)
        {
            while (hull.Count >= 2 && ComputeCross(hull[hull.Count - 2], hull[hull.Count - 1], pt) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(pt);
        }

        // Build upper part
        int lowerCount = hull.Count;
        for (int i = points.Count - 2; i >= 0; i--)
        {
            var pt = points[i];
            while (hull.Count > lowerCount && ComputeCross(hull[hull.Count - 2], hull[hull.Count - 1], pt) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(pt);
        }

        // Remove the duplicate last point
        if (hull.Count > 0)
        {
            hull.RemoveAt(hull.Count - 1);
        }

        return hull;
    }

    private static float ComputeCross(Vector2 origin, Vector2 a, Vector2 b)
    {
        return (a.X - origin.X) * (b.Y - origin.Y) - (a.Y - origin.Y) * (b.X - origin.X);
    }
}
