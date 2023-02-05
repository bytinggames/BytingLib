using System;
using System.Collections.Generic;

namespace BytingLibGame.IngameSpline
{
    public abstract class CatmullRomSpline<T> where T : struct
    {
        public CatmullRomSpline(List<T> points)
        {
            this.Points = points;
        }

        public List<T> Points { get; set; }

        public T? Sample(float t)
        {
            if (Points == null || Points.Count < 4)
                return null;

            if (t < 0)
                t = 0;
            else if (t > Points.Count - 1)
                t = Points.Count - 1;

            int tInt = (int)MathF.Floor(t);

            t = t - tInt;
            if (t == 0f)
                return Points[tInt];

            int p0, p1, p2, p3;
            p0 = tInt - 1;
            p1 = p0 + 1;
            p2 = p1 + 1;
            p3 = p2 + 1;

            float tt = t * t;
            float ttt = tt * t;

            float q0 = 0.5f * (-ttt + 2f * tt - t);
            float q1 = 0.5f * (3f * ttt - 5f * tt + 2f);
            float q2 = 0.5f * (-3f * ttt + 4f * tt + t);
            float q3 = 0.5f * (ttt - tt);

            return GetWeighted(p0, p1, p2, p3, q0, q1, q2, q3);
        }

        protected abstract T GetWeighted(int p0, int p1, int p2, int p3, float q0, float q1, float q2, float q3);
    }

    public class CatmullRomSpline1 : CatmullRomSpline<float>
    {
        public CatmullRomSpline1(List<float> points) : base(points)
        {
        }

        protected override float GetWeighted(int p0, int p1, int p2, int p3, float q0, float q1, float q2, float q3)
        {
            return Points[p0] * q0 + Points[p1] * q1 + Points[p2] * q2 + Points[p3] * q3;
        }
    }
    public class CatmullRomSpline2 : CatmullRomSpline<Vector2>
    {
        public CatmullRomSpline2(List<Vector2> points) : base(points)
        {
        }

        protected override Vector2 GetWeighted(int p0, int p1, int p2, int p3, float q0, float q1, float q2, float q3)
        {
            return Points[p0] * q0 + Points[p1] * q1 + Points[p2] * q2 + Points[p3] * q3;
        }
    }
    public class CatmullRomSpline3 : CatmullRomSpline<Vector3>
    {
        public CatmullRomSpline3(List<Vector3> points) : base(points)
        {
        }

        protected override Vector3 GetWeighted(int p0, int p1, int p2, int p3, float q0, float q1, float q2, float q3)
        {
            return Points[p0] * q0 + Points[p1] * q1 + Points[p2] * q2 + Points[p3] * q3;
        }
    }

}
