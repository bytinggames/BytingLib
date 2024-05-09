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
            Vector3 scale = rotationMatrix.GetScale();
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

        public static Matrix CreateFromBaseAxes(Vector3 translation, Vector3 right, Vector3 up, Vector3 backward)
        {
            return new Matrix(new Vector4(right, 0f),
                new Vector4(up, 0f),
                new Vector4(backward, 0f),
                new Vector4(translation, 1f)
            );
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
                // wrap rotation between 0 and MathHelper.PiOver2
                float r1 = r % MathHelper.PiOver2;
                if (r1 < 0f)
                {
                    r1 += MathHelper.PiOver2;
                }
                if (MathHelper.PiOver4 - MathF.Abs(MathHelper.PiOver4 - r1) > precision)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Faster than <see cref="Matrix.Decompose()"/>.</summary>
        public static Vector3 GetScale(this Matrix m)
        {
            // source from Matrix.Decompose()
            float num = (Math.Sign(m.M11 * m.M12 * m.M13 * m.M14) >= 0) ? 1 : (-1);
            float num2 = (Math.Sign(m.M21 * m.M22 * m.M23 * m.M24) >= 0) ? 1 : (-1);
            float num3 = (Math.Sign(m.M31 * m.M32 * m.M33 * m.M34) >= 0) ? 1 : (-1);
            Vector3 scale;
            scale.X = num * MathF.Sqrt(m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13);
            scale.Y = num2 * MathF.Sqrt(m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23);
            scale.Z = num3 * MathF.Sqrt(m.M31 * m.M31 + m.M32 * m.M32 + m.M33 * m.M33);

            return scale;
        }
    }
}
