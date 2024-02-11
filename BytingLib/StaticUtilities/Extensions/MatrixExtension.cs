namespace BytingLib
{
    public static class MatrixExtension
    {
        public static void ToPitchYawRoll(this Matrix rotationMatrix, out float yaw, out float pitch, out float roll)
        {
            // source: https://www.dreamincode.net/forums/topic/349917-convert-from-quaternion-to-euler-angles-vector3/
            float ForwardY = -rotationMatrix.M32;
            if (ForwardY <= -1.0f)
            {
                pitch = -MathHelper.PiOver2;
            }
            else if (ForwardY >= 1.0f)
            {
                pitch = MathHelper.PiOver2;
            }
            else
            {
                pitch = MathF.Asin(ForwardY);
            }
            //Gimbal lock
            if (ForwardY > 0.9999f)
            {
                yaw = 0f;
                roll = MathF.Atan2(rotationMatrix.M13, rotationMatrix.M11);
            }
            else
            {
                yaw = MathF.Atan2(rotationMatrix.M31, rotationMatrix.M33);
                roll = MathF.Atan2(rotationMatrix.M12, rotationMatrix.M22);
            }
        }

        public static void ToPitchYawRollFromScaled(this Matrix rotationMatrix, out float yaw, out float pitch, out float roll)
        {
            rotationMatrix.Decompose(out Vector3 scale, out _, out _);
            (Matrix.CreateScale(Vector3.One / scale) * rotationMatrix).ToPitchYawRoll(out yaw, out pitch, out roll);
        }

        public static Matrix CreateMatrixRotationFromTo(Vector3 from, Vector3 to)
        {
            if (from == to)
            {
                return Matrix.Identity;
            }

            float dot = Vector3.Dot(Vector3.Normalize(from), Vector3.Normalize(to));
            if (dot == 1f)
            {
                return Matrix.Identity;
            }

            float angle = MathF.Acos(dot);
            Vector3 axis = Vector3.Normalize(Vector3.Cross(from, to));
            return Matrix.CreateFromAxisAngle(axis, angle);
        }

        public static Matrix GetInverseTranspose3x3(this Matrix m)
        {
            m = GetInverse3x3(ref m);
            return Matrix.Transpose(m);
        }

        public static Matrix GetInverse3x3(ref Matrix m)
        {
            float a00 = m[0,0], a01 = m[0,1], a02 = m[0,2];
            float a10 = m[1,0], a11 = m[1,1], a12 = m[1,2];
            float a20 = m[2,0], a21 = m[2,1], a22 = m[2,2];

            float b01 = a22 * a11 - a12 * a21;
            float b11 = -a22 * a10 + a12 * a20;
            float b21 = a21 * a10 - a11 * a20;

            float det = a00 * b01 + a01 * b11 + a02 * b21;

            return new Matrix(b01 / det, (-a22 * a01 + a02 * a21) / det, (a12 * a01 - a02 * a11) / det, 0,
                b11 / det, (a22 * a00 - a02 * a20) / det, (-a12 * a00 + a02 * a10) / det, 0,
                b21 / det, (-a21 * a00 + a01 * a20) / det, (a11 * a00 - a01 * a10) / det, 0,
                0, 0, 0, 1) ;
        }

        /// <summary>Checks wether the matrix is a result of rotations that are only around the base axes by a multiple of 90 degrees.</summary>
        public static bool RotatesBaseAxisToParallelBaseAxis(this Matrix m, float precision = 0.0001f)
        {
            m.ToPitchYawRoll(out float yaw, out float pitch, out float roll);
            return RotatesBaseAxisToParallelBaseAxisInner(yaw, pitch, roll, precision);
        }
        /// <summary>Checks wether the matrix is a result of rotations that are only around the base axes by a multiple of 90 degrees.</summary>
        public static bool RotatesBaseAxisToParallelBaseAxisFromScaled(this Matrix m, float precision = 0.0001f)
        {
            m.ToPitchYawRollFromScaled(out float yaw, out float pitch, out float roll);
            return RotatesBaseAxisToParallelBaseAxisInner(yaw, pitch, roll, precision);
        }

        private static bool RotatesBaseAxisToParallelBaseAxisInner(float yaw, float pitch, float roll, float precision)
        {
            float[] toCheck = new float[] { yaw, pitch, roll };
            foreach (var r in toCheck)
            {
                if (MathHelper.PiOver4 - MathF.Abs(MathHelper.PiOver4 - r % MathHelper.PiOver2) > precision)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
