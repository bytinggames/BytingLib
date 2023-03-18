namespace BytingLib
{
    public static class BoundingFrustumExtension
    {
        public static Matrix GetOrthographicViewProjectionForDirection(this BoundingFrustum frustum, Vector3 normalizedDirection, Vector3 up,
            float clipAddition, Vector2? forceSize = null, Int2? shadowCascadeResolution = null)
        {
            Vector3[] corners = GetRectangleCornersForDirectionView(frustum, normalizedDirection, out float clipNear, out float clipFar);

            float clipDistance = clipFar - clipNear;
            if (clipDistance < clipAddition)
                clipFar += clipAddition - clipDistance;

            clipNear -= clipAddition;

            return GetOrthographicViewProjectionFromRectangle(corners, normalizedDirection, up, clipNear, clipFar, shadowCascadeResolution, forceSize);
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

        public static Matrix GetOrthographicViewProjectionFromRectangle(Vector3[] corners, Vector3 normalizedDirection, Vector3 up,
            float clipNear, float clipFar, Int2? shadowCascadeResolution = null, Vector2? forceSize = null)
        {
            Vector3 center = (corners[0] + corners[2]) / 2f;

            Vector2 projectionPlaneSize;

            if (forceSize.HasValue)
            {
                // keep constant size
                projectionPlaneSize = forceSize.Value;
                // TODO: don't naively adapt to the forced size. Adapt to it in a way, that the new gained size is in the visible space of the camera
            }
            else
            {
                // fit size
                Vector3 w = corners[1] - corners[0];
                Vector3 h = corners[2] - corners[1];
                w -= normalizedDirection * Vector3.Dot(w, normalizedDirection);
                h -= normalizedDirection * Vector3.Dot(h, normalizedDirection);

                projectionPlaneSize = new Vector2(w.Length(), h.Length()) * 2f; // no * 2 when drawing the frustum. * 2 when using the matrix for real shadow calculations
            }

            // round position to light space coordinates
            if (shadowCascadeResolution != null)
            {
                Vector3 rightOrth = Vector3.Normalize(Vector3.Cross(up, normalizedDirection));
                Vector3 upOrth = Vector3.Normalize(Vector3.Cross(rightOrth, normalizedDirection));

                Vector2 pixelsPerMeter = shadowCascadeResolution.Value.ToVector2() / projectionPlaneSize;

                Vector2 posDot = new Vector2(Vector3.Dot(center, rightOrth), Vector3.Dot(center, upOrth));
                Vector2 posDotRounded = posDot * pixelsPerMeter;
                posDotRounded.Round();
                posDotRounded /= pixelsPerMeter;
                Vector2 posDotDifference = posDotRounded - posDot;
                center += posDotDifference.X * rightOrth + posDotDifference.Y * upOrth;
            }

            Matrix view = Matrix.CreateLookAt(center, center - normalizedDirection, -up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(
                -projectionPlaneSize.X / 2f,
                projectionPlaneSize.X / 2f, 
                projectionPlaneSize.Y / 2f,
                -projectionPlaneSize.Y / 2f,
                -clipNear, 
                -clipFar);
            return view * projection;
        }
    }
}