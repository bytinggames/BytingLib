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

        public static Matrix CreateMatrixRotationFromTo(Vector3 from, Vector3 to)
        {
            if (from == to)
                return Matrix.Identity;
            float dot = Vector3.Dot(Vector3.Normalize(from), Vector3.Normalize(to));
            if (dot == 1f)
                return Matrix.Identity;
            float angle = MathF.Acos(dot);
            Vector3 axis = Vector3.Normalize(Vector3.Cross(from, to));
            return Matrix.CreateFromAxisAngle(axis, angle);
        }

        public static Matrix GetInverseTransposeWithoutTranslation(this Matrix m)
        {
            m.Translation = Vector3.Zero;
            return Matrix.Transpose(Matrix.Invert(m));
        }
    }
}
