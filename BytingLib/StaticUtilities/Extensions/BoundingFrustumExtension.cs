namespace BytingLib
{
    public static class BoundingFrustumExtension
    {
        public static Matrix GetOrthographicViewProjectionForDirection(this BoundingFrustum frustum, Vector3 normalizedDirection, Vector3 up)
        {
            Vector3[] corners = GetRectangleCornersForDirectionView(frustum, normalizedDirection, out float clipNear, out float clipFar);
            clipNear -= 300f;
            clipFar += 300f;
            //float clipDist = clipFar - clipNear;
            //clipNear -= clipDist; // move clipNear a bit backwards, so that geometry can cast shadows behind you
            //clipFar += clipDist;
            // if shadows don't appear, this might be the reason for it^ maybe it should be pushed back even more then
            return GetOrthographicViewProjectionFromRectangle(corners, normalizedDirection, up, clipNear, clipFar);
        }

        public static Vector3[] GetRectangleCornersForDirectionView(this BoundingFrustum frustum, Vector3 direction, out float clipNear, out float clipFar)
        {
            Matrix rotateToLightView = Matrix.CreateLookAt(Vector3.Zero, direction, Vector3.Up);

            var corners = frustum.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Vector3.Transform(corners[i], rotateToLightView);
            }

            float left, top, right, bottom;
            left = right = corners[0].X;
            top = bottom = corners[0].Y;
            // not sure why, but clipNear and clipFar must be negated
            clipNear = -corners[0].Z;
            clipFar = -corners[0].Z;

            for (int i = 1; i < corners.Length; i++)
            {
                left = MathF.Min(left, corners[i].X);
                top = MathF.Min(top, corners[i].Y);
                clipNear = MathF.Min(clipNear, -corners[i].Z);
                right = MathF.Max(right, corners[i].X);
                bottom = MathF.Max(bottom, corners[i].Y);
                bottom = MathF.Max(bottom, corners[i].Y);
                clipFar = MathF.Max(clipFar, -corners[i].Z);
            }

            Vector2[] lightFrustumCorners = new Vector2[]{
                new(left, top),
                new(right,top),
                new(right,bottom),
                new(left,bottom)
            };

            Matrix rotateToWorldView = Matrix.Invert(rotateToLightView);

            Vector3[] lightFrustumCorners3D = new Vector3[lightFrustumCorners.Length];
            for (int i = 0; i < lightFrustumCorners.Length; i++)
            {
                lightFrustumCorners3D[i] = Vector3.Transform(new Vector3(lightFrustumCorners[i], 0f), rotateToWorldView);
            }

            return lightFrustumCorners3D;
        }

        public static Matrix GetOrthographicViewProjectionFromRectangle(Vector3[] corners, Vector3 normalizedDirection, Vector3 up, float clipNear, float clipFar)
        {
            Vector3 center = (corners[0] + corners[2]) / 2f;
            Vector3 w = corners[1] - corners[0];
            Vector3 h = corners[2] - corners[1];
            w -= normalizedDirection * Vector3.Dot(w, normalizedDirection);
            h -= normalizedDirection * Vector3.Dot(h, normalizedDirection);

            float wLength_2 = w.Length();// / 2f;// / 2 when drawing the frustum. no / 2 when using the matrix for real shadow calculations
            float hLength_2 = h.Length();// / 2f;

            Matrix view = Matrix.CreateLookAt(center, center - normalizedDirection, -up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(-wLength_2, wLength_2, hLength_2, -hLength_2, -clipNear, -clipFar);
            return view * projection;
        }
    }
}