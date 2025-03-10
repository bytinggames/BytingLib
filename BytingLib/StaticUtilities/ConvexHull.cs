public static class ConvexHull
{
    public static List<Vector2> GetConvexHull(IList<Vector2> points)
    {
        if (points == null || points.Count <= 2)
        {
            return new List<Vector2>(); // A convex hull requires at least 3 points
        }

        // Sort points lexicographically (first by x, then by y)
        points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

        List<Vector2> hull = new List<Vector2>();

        // Build lower hull
        foreach (var p in points)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(p);
        }

        // Build upper hull
        int lowerHullCount = hull.Count;
        for (int i = points.Count - 2; i >= 0; i--)
        {
            var p = points[i];
            while (hull.Count > lowerHullCount && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(p);
        }

        // Remove last point because it is duplicated at the beginning
        hull.RemoveAt(hull.Count - 1);

        return hull;
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }
}
